import os
from typing import Any, Dict

import boto3
from dotenv import load_dotenv
from strands.models.bedrock import BedrockModel


def create_bedrock_model(model_config: Dict[str, Any]) -> BedrockModel:
    """
    Creates a Strands BedrockModel using environment variables for credentials
    and YAML config for model parameters.

    Builds an explicit boto3 session from .env credentials to avoid
    conflicts with any AWS_PROFILE setting on the machine.
    """
    load_dotenv()

    model_id_env = model_config.get("model_id_env", "AWS_BEDROCK_MODEL_ID")
    region_env = model_config.get("region_env", "AWS_DEFAULT_REGION")

    model_id = os.getenv(model_id_env) or model_config.get("fallback_model_id")
    region = os.getenv(region_env) or model_config.get("fallback_region", "us-east-1")

    if not model_id:
        raise ValueError(
            "Bedrock model ID is missing. Set AWS_BEDROCK_MODEL_ID in .env "
            "or fallback_model_id in agent_config.yaml."
        )

    # Clear AWS_PROFILE so boto3 doesn't try to resolve a named profile
    # that may not exist locally. We use explicit credentials from .env instead.
    os.environ.pop("AWS_PROFILE", None)
    os.environ.pop("AWS_DEFAULT_PROFILE", None)

    boto_session = boto3.Session(
        aws_access_key_id=os.getenv("AWS_ACCESS_KEY_ID"),
        aws_secret_access_key=os.getenv("AWS_SECRET_ACCESS_KEY"),
        aws_session_token=os.getenv("AWS_SESSION_TOKEN"),
        region_name=region,
    )

    return BedrockModel(
        model_id=model_id,
        boto_session=boto_session,
        temperature=model_config.get("temperature", 0.2),
        top_p=model_config.get("top_p", 0.9),
        max_tokens=model_config.get("max_tokens", 2000),
        streaming=model_config.get("streaming", False),
    )
