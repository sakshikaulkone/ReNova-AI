import sys
import io
from pathlib import Path
from typing import List

from strands import Agent

# Force UTF-8 encoding for stdout to handle Unicode characters on Windows
if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

from agent.config.model_factory import create_model_from_config
from agent.tools.prompt_loader import load_text_file, load_yaml_file, render_prompt
from agent.tools.code_extractor import extract_and_save_from_report


CONFIG_PATH = "agent/config/agent_config.yaml"


def load_selected_files(file_paths: List[str]) -> str:
    """
    Reads each file and combines them into a single text block
    with clear file headers. If a file is missing, includes a warning.
    """
    sections = []

    for path in file_paths:
        header = f"--- FILE: {path} ---"
        file_path = Path(path)

        if not file_path.exists():
            sections.append(f"{header}\n[WARNING: File not found - {path}]\n")
        else:
            try:
                content = file_path.read_text(encoding="utf-8", errors="ignore")
                sections.append(f"{header}\n{content}\n")
            except Exception as e:
                sections.append(f"{header}\n[WARNING: Could not read file - {e}]\n")

    return "\n".join(sections)


def load_first_existing_file(paths: List[str], fallback_message: str) -> str:
    """
    Returns the content of the first file that exists in the list.
    If none exist, returns the fallback_message.
    """
    for path in paths:
        if Path(path).exists():
            return load_text_file(path)
    return fallback_message


def save_generated_code_placeholder(output_dir: str, report_text: str) -> None:
    """
    Creates the generated code output directory and a README explaining
    that generated code is currently inside the conversion report.
    """
    dir_path = Path(output_dir)
    dir_path.mkdir(parents=True, exist_ok=True)

    readme_path = dir_path / "README.md"
    readme_path.write_text(
        "# Generated Code\n\n"
        "The generated modern .NET code from the Code Conversion Agent is currently\n"
        "included in the conversion report appendix:\n\n"
        "  sandbox/reports/code_conversion_report.md\n\n"
        "See **Appendix A. Generated Code Blocks** in that report for all proposed\n"
        "modern C# files.\n\n"
        "Future iterations will extract these into individual .cs files here.\n",
        encoding="utf-8",
    )


def run_code_conversion_agent() -> str:
    """
    Runs the Code Conversion Agent end-to-end:
    1. Loads config and prompts
    2. Loads modernization context, SME answers, and legacy understanding
    3. Loads selected legacy files
    4. Renders the task prompt
    5. Creates a Strands agent backed by Bedrock
    6. Calls the LLM
    7. Saves report and generated code placeholder
    8. Returns the report text
    """
    config = load_yaml_file(CONFIG_PATH)

    paths = config["paths"]
    prompts_config = config["prompts"]

    system_prompt = load_text_file(prompts_config["code_conversion_system"])
    task_template = load_text_file(prompts_config["code_conversion_task"])
    output_format = load_text_file(prompts_config["code_conversion_output_format"])

    report_path = paths["code_conversion_report_path"]
    output_dir = paths["generated_code_output_dir"]
    file_paths = paths["code_conversion_files"]

    print("Loading modernization request context...")
    modernization_context = load_first_existing_file(
        [
            paths.get("modernization_request_report_path", ""),
            paths.get("modernization_request_path", ""),
        ],
        "(No modernization request context available.)",
    )

    print("Loading SME answers...")
    sme_answers = load_text_file(paths["sme_answers_path"])

    print("Loading legacy understanding report...")
    legacy_understanding = load_first_existing_file(
        [paths.get("legacy_understanding_report_path", "")],
        "(Legacy understanding report not available. Using selected source files directly.)",
    )

    print(f"Loading {len(file_paths)} legacy files for conversion...")
    legacy_file_contents = load_selected_files(file_paths)

    task_prompt = render_prompt(task_template, {
        "MODERNIZATION_REQUEST_CONTEXT": modernization_context,
        "SME_ANSWERS": sme_answers,
        "LEGACY_UNDERSTANDING_REPORT": legacy_understanding,
        "LEGACY_FILE_CONTENTS": legacy_file_contents,
    })
    final_user_prompt = task_prompt + "\n\n" + output_format

    print("Creating Bedrock model connection...")
    model = create_model_from_config(config)

    print("Running Code Conversion Agent...")
    agent = Agent(model=model, system_prompt=system_prompt)

    try:
        response = agent(final_user_prompt)
        report_text = str(response)
    except Exception as e:
        _handle_bedrock_error(e)
        raise

    output_file = Path(report_path)
    output_file.parent.mkdir(parents=True, exist_ok=True)
    output_file.write_text(report_text, encoding="utf-8")

    print(f"\nCode conversion report saved to: {report_path}")

    # Extract code blocks and save as individual .cs files
    print("\nExtracting code blocks to individual files...")
    try:
        file_count = extract_and_save_from_report(report_path, output_dir)
        if file_count > 0:
            print(f"Generated {file_count} C# files in: {output_dir}")
        else:
            print(f"WARNING: No code files were extracted. Report may be incomplete.")
            print(f"Placeholder README saved to: {output_dir}/README.md")
            save_generated_code_placeholder(output_dir, report_text)
    except Exception as e:
        print(f"WARNING: Could not extract code files: {e}")
        print(f"Report is still available at: {report_path}")
        save_generated_code_placeholder(output_dir, report_text)

    return report_text


def _handle_bedrock_error(error: Exception) -> None:
    """Prints a helpful diagnostic message for common Bedrock errors."""
    error_msg = str(error)
    print(f"\nBedrock Error: {error_msg}", file=sys.stderr)

    if "AccessDeniedException" in error_msg:
        print(
            "Likely cause: IAM role/user does not have bedrock:InvokeModel permission.",
            file=sys.stderr,
        )
    elif "UnrecognizedClientException" in error_msg:
        print(
            "Likely cause: AWS credentials are invalid or expired. "
            "Check AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY in .env.",
            file=sys.stderr,
        )
    elif "ValidationException" in error_msg:
        print(
            "Likely cause: Model ID may be incorrect or not available in your region. "
            "Check AWS_BEDROCK_MODEL_ID in .env.",
            file=sys.stderr,
        )
    elif "Could not connect" in error_msg:
        print(
            "Likely cause: Wrong AWS region or network issue. "
            "Check AWS_DEFAULT_REGION in .env.",
            file=sys.stderr,
        )
    elif "on-demand throughput" in error_msg:
        print(
            "Likely cause: This model requires an inference profile or provisioned throughput. "
            "Try a different model ID or request model access in the AWS console.",
            file=sys.stderr,
        )
    elif "ExpiredTokenException" in error_msg or "expired" in error_msg.lower():
        print(
            "Likely cause: AWS session token has expired. "
            "Refresh your credentials and update .env.",
            file=sys.stderr,
        )


if __name__ == "__main__":
    run_code_conversion_agent()
