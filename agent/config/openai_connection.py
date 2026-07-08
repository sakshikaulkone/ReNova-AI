import os
from typing import Any, Dict

from dotenv import load_dotenv
from strands.models.openai import OpenAIModel


def create_openai_model(model_config: Dict[str, Any]) -> OpenAIModel:
    """
    Creates a Strands OpenAIModel for OpenAI-compatible APIs.

    This works with:
    - Direct OpenAI API
    - KONE AI Gateway (Gecko) - uses OpenAI format with Claude models
    - Other OpenAI-compatible gateways
    """
    load_dotenv()

    api_key_env = model_config.get("api_key_env", "OPENAI_API_KEY")
    model_id_env = model_config.get("model_id_env", "OPENAI_MODEL_ID")
    base_url_env = model_config.get("base_url_env", "OPENAI_BASE_URL")

    api_key = os.getenv(api_key_env)
    model_id = os.getenv(model_id_env) or model_config.get(
        "fallback_model_id",
        "gpt-4",
    )
    base_url = os.getenv(base_url_env) or model_config.get("base_url")

    if not api_key:
        raise ValueError(
            f"API key is missing. Set {api_key_env} in your local .env file."
        )

    client_args = {"api_key": api_key}
    if base_url:
        client_args["base_url"] = base_url

    return OpenAIModel(
        client_args=client_args,
        model_id=model_id,
        params={
            "temperature": model_config.get("temperature", 0.2),
            "max_tokens": model_config.get("max_tokens", 4096),
        },
    )
