import sys
from pathlib import Path
from typing import List

from strands import Agent

from agent.config.model_factory import create_model_from_config
from agent.tools.prompt_loader import load_text_file, load_yaml_file, render_prompt


CONFIG_PATH = "agent/config/agent_config.yaml"


def load_legacy_files(file_paths: List[str]) -> str:
    """
    Reads each legacy file and combines them into a single text block
    with clear file headers. If a file is missing, includes a warning instead.
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


def load_modernization_context(config: dict) -> str:
    """
    Loads modernization context. Prefers the generated report from a prior agent run.
    Falls back to the raw modernization request file if no report exists yet.
    """
    report_path = config["paths"].get("modernization_request_report_path", "")

    if report_path and Path(report_path).exists():
        return load_text_file(report_path)

    fallback_path = config["paths"].get("modernization_request_path", "")
    if fallback_path and Path(fallback_path).exists():
        return load_text_file(fallback_path)

    return "(No modernization request context available.)"


def run_legacy_understanding_agent() -> str:
    """
    Runs the Legacy Understanding Agent end-to-end:
    1. Loads config and prompts
    2. Loads modernization context
    3. Loads selected legacy files
    4. Renders the task prompt
    5. Creates a Strands agent backed by Bedrock
    6. Calls the LLM
    7. Saves and returns the report
    """
    config = load_yaml_file(CONFIG_PATH)

    paths = config["paths"]
    prompts_config = config["prompts"]

    system_prompt = load_text_file(prompts_config["legacy_understanding_system"])
    task_template = load_text_file(prompts_config["legacy_understanding_task"])
    output_format = load_text_file(prompts_config["legacy_understanding_output_format"])

    report_path = paths["legacy_understanding_report_path"]
    file_paths = paths["legacy_files_to_understand"]

    print("Loading modernization context...")
    modernization_context = load_modernization_context(config)

    print(f"Loading {len(file_paths)} legacy files for analysis...")
    legacy_file_contents = load_legacy_files(file_paths)

    task_prompt = render_prompt(task_template, {
        "MODERNIZATION_REQUEST_REPORT": modernization_context,
        "LEGACY_FILE_CONTENTS": legacy_file_contents,
    })
    final_user_prompt = task_prompt + "\n\n" + output_format

    print("Creating Bedrock model connection...")
    model = create_model_from_config(config)

    print("Running Legacy Understanding Agent...")
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

    print(f"\nLegacy understanding report saved to: {report_path}")
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
    run_legacy_understanding_agent()
