from unittest.mock import patch

import pytest

from agent.config.model_factory import create_model_from_config


def test_unsupported_provider_raises():
    """Unknown provider should raise ValueError."""
    config = {"model": {"provider": "openai"}}
    with pytest.raises(ValueError, match="Unsupported model provider: openai"):
        create_model_from_config(config)


def test_missing_provider_raises():
    """Missing provider key should raise ValueError."""
    config = {"model": {}}
    with pytest.raises(ValueError, match="Unsupported model provider: None"):
        create_model_from_config(config)


@patch("agent.config.model_factory.create_bedrock_model")
def test_bedrock_provider_calls_create_bedrock_model(mock_create):
    """provider='bedrock' should route to create_bedrock_model."""
    mock_create.return_value = "mock_model"

    config = {
        "model": {
            "provider": "bedrock",
            "model_id_env": "AWS_BEDROCK_MODEL_ID",
            "region_env": "AWS_DEFAULT_REGION",
            "fallback_model_id": "test-model",
            "fallback_region": "us-east-1",
            "temperature": 0.2,
            "top_p": 0.9,
            "max_tokens": 2000,
            "streaming": False,
        }
    }

    result = create_model_from_config(config)
    assert result == "mock_model"
    mock_create.assert_called_once_with(config["model"])
