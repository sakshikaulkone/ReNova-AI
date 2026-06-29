import json
import sys
from pathlib import Path

from strands import Agent

from agent.config.model_factory import create_model_from_config
from agent.tools.prompt_loader import load_text_file, load_yaml_file, render_prompt
from agent.tools.repo_scanner import save_scan_result, scan_repo


CONFIG_PATH = "agent/config/agent_config.yaml"


def run_repo_intake_agent() -> str:
    """
    Runs the Repo Intake Agent end-to-end:
    1. Loads config and prompts
    2. Runs the scanner
    3. Renders the task prompt with scanner output
    4. Creates a Strands agent backed by Bedrock
    5. Calls the LLM
    6. Saves and returns the report
    """
    config = load_yaml_file(CONFIG_PATH)

    paths = config["paths"]
    prompts_config = config["prompts"]

    system_prompt = load_text_file(prompts_config["repo_intake_system"])
    task_template = load_text_file(prompts_config["repo_intake_task"])
    output_format = load_text_file(prompts_config["repo_intake_output_format"])

    sample_repo_path = paths["sample_repo_path"]
    scanner_output_path = paths["scanner_output_path"]
    report_path = paths["repo_intake_report_path"]

    print(f"Scanning repository: {sample_repo_path}")
    scan_result = scan_repo(sample_repo_path)
    save_scan_result(scan_result, scanner_output_path)
    print(f"Scanner output saved to: {scanner_output_path}")

    scanner_json = json.dumps(scan_result, indent=2)

    task_prompt = render_prompt(task_template, {"SCANNER_OUTPUT": scanner_json})
    final_user_prompt = task_prompt + "\n\n" + output_format

    print("Creating Bedrock model connection...")
    model = create_model_from_config(config)

    print("Running Repo Intake Agent...")
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

    print(f"\nRepo intake report saved to: {report_path}")
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


if __name__ == "__main__":
    run_repo_intake_agent()
