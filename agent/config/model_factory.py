from typing import Any, Dict

from agent.config.bedrock_connection import create_bedrock_model


def create_model_from_config(config: Dict[str, Any]):
    """
    Reads the model provider from config and returns the appropriate model object.
    Currently supports 'bedrock'. Raises ValueError for unknown providers.
    """
    model_config = config.get("model", {})
    provider = model_config.get("provider")

    if provider == "bedrock":
        return create_bedrock_model(model_config)

    raise ValueError(f"Unsupported model provider: {provider}")
