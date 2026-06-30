import sys
from pathlib import Path

from strands import Agent

from agent.config.model_factory import create_model_from_config
from agent.tools.prompt_loader import load_text_file, load_yaml_file, render_prompt


CONFIG_PATH = "agent/config/agent_config.yaml"


def run_modernization_request_agent() -> str:
    """
    Runs the Modernization Request Intake Agent end-to-end:
    1. Loads config and prompts
    2. Loads the modernization request markdown
    3. Renders the task prompt with the request content
    4. Creates a Strands agent backed by Bedrock
    5. Calls the LLM
    6. Saves and returns the report
    """
    config = load_yaml_file(CONFIG_PATH)

    paths = config["paths"]
    prompts_config = config["prompts"]

    system_prompt = load_text_file(prompts_config["modernization_request_system"])
    task_template = load_text_file(prompts_config["modernization_request_task"])
    output_format = load_text_file(prompts_config["modernization_request_output_format"])

    request_path = paths["modernization_request_path"]
    report_path = paths["modernization_request_report_path"]

    print(f"Loading modernization request: {request_path}")
    request_content = load_text_file(request_path)

    task_prompt = render_prompt(task_template, {"MODERNIZATION_REQUEST": request_content})
    final_user_prompt = task_prompt + "\n\n" + output_format

    print("Creating Bedrock model connection...")
    model = create_model_from_config(config)

    print("Running Modernization Request Intake Agent...")
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

    print(f"\nModernization request report saved to: {report_path}")
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
    run_modernization_request_agent()
