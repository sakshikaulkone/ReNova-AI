from typing import Any, Dict

from agent.config.openai_connection import create_openai_model

# Other providers (kept for reference):
# from agent.config.bedrock_connection import create_bedrock_model
# from agent.config.anthropic_connection import create_anthropic_model


def create_model_from_config(config: Dict[str, Any]):
    """
    Reads the model provider from config and returns the appropriate model object.
    Currently supports 'openai' (for KONE Gecko gateway and OpenAI-compatible APIs).
    Bedrock and Anthropic direct API support is commented out.
    Raises ValueError for unknown providers.
    """
    model_config = config.get("model", {})
    provider = model_config.get("provider", "openai")

    if provider == "openai":
        openai_config = model_config.get("openai", model_config)
        return create_openai_model(openai_config)

    # Bedrock support (for direct AWS access):
    # if provider == "bedrock":
    #     bedrock_config = model_config.get("bedrock", model_config)
    #     return create_bedrock_model(bedrock_config)

    # Anthropic direct API (not compatible with KONE gateway):
    # if provider == "anthropic":
    #     anthropic_config = model_config.get("anthropic", model_config)
    #     return create_anthropic_model(anthropic_config)

    raise ValueError(f"Unsupported model provider: {provider}")
