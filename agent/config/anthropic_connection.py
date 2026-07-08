import os
from typing import Any, Dict

from dotenv import load_dotenv
from strands.models.anthropic import AnthropicModel


def create_anthropic_model(model_config: Dict[str, Any]) -> AnthropicModel:
    """
    Creates a Strands AnthropicModel using environment variables for credentials
    and YAML config for model parameters.

    Supports custom base URLs for company proxies/gateways (like KONE's AI gateway).
    Uses the local API key from .env to connect to Anthropic API or company gateway.
    """
    load_dotenv()

    api_key_env = model_config.get("api_key_env", "ANTHROPIC_API_KEY")
    model_id_env = model_config.get("model_id_env", "ANTHROPIC_MODEL_ID")
    base_url_env = model_config.get("base_url_env", "ANTHROPIC_BASE_URL")

    api_key = os.getenv(api_key_env)
    model_id = os.getenv(model_id_env) or model_config.get(
        "fallback_model_id",
        "claude-sonnet-4-5",
    )
    base_url = os.getenv(base_url_env) or model_config.get("base_url")

    if not api_key:
        raise ValueError(
            f"Anthropic API key is missing. Set {api_key_env} in your local .env file."
        )

    client_args = {"api_key": api_key}
    if base_url:
        client_args["base_url"] = base_url

    return AnthropicModel(
        client_args=client_args,
        model_id=model_id,
        max_tokens=model_config.get("max_tokens", 3500),
        params={
            "temperature": model_config.get("temperature", 0.2),
        },
    )
