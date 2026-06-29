from unittest.mock import patch, MagicMock

import pytest

from agent.config.bedrock_connection import create_bedrock_model


@patch("agent.config.bedrock_connection.load_dotenv")
def test_create_bedrock_model_raises_if_no_model_id(mock_dotenv, monkeypatch):
    """If no model ID is available from env or config, raise ValueError."""
    monkeypatch.delenv("AWS_BEDROCK_MODEL_ID", raising=False)

    config = {
        "model_id_env": "AWS_BEDROCK_MODEL_ID",
        "region_env": "AWS_DEFAULT_REGION",
        # no fallback_model_id
    }

    with pytest.raises(ValueError, match="model ID is missing"):
        create_bedrock_model(config)


@patch("agent.config.bedrock_connection.load_dotenv")
@patch("agent.config.bedrock_connection.boto3.Session")
@patch("agent.config.bedrock_connection.BedrockModel")
def test_create_bedrock_model_uses_env_vars(mock_bedrock_model, mock_session, mock_dotenv, monkeypatch):
    """Should read model_id and region from environment variables."""
    monkeypatch.setenv("AWS_BEDROCK_MODEL_ID", "anthropic.claude-3-haiku-20240307-v1:0")
    monkeypatch.setenv("AWS_DEFAULT_REGION", "us-west-2")
    monkeypatch.setenv("AWS_ACCESS_KEY_ID", "fake-key")
    monkeypatch.setenv("AWS_SECRET_ACCESS_KEY", "fake-secret")
    monkeypatch.setenv("AWS_SESSION_TOKEN", "fake-token")

    config = {
        "model_id_env": "AWS_BEDROCK_MODEL_ID",
        "region_env": "AWS_DEFAULT_REGION",
        "fallback_model_id": "fallback-model",
        "fallback_region": "us-east-1",
        "temperature": 0.3,
        "top_p": 0.8,
        "max_tokens": 1500,
        "streaming": False,
    }

    create_bedrock_model(config)

    mock_session.assert_called_once_with(
        aws_access_key_id="fake-key",
        aws_secret_access_key="fake-secret",
        aws_session_token="fake-token",
        region_name="us-west-2",
    )
    mock_bedrock_model.assert_called_once()
    call_kwargs = mock_bedrock_model.call_args[1]
    assert call_kwargs["model_id"] == "anthropic.claude-3-haiku-20240307-v1:0"
    assert "region_name" not in call_kwargs
    assert call_kwargs["boto_session"] == mock_session.return_value


@patch("agent.config.bedrock_connection.load_dotenv")
@patch("agent.config.bedrock_connection.boto3.Session")
@patch("agent.config.bedrock_connection.BedrockModel")
def test_create_bedrock_model_uses_fallback(mock_bedrock_model, mock_session, mock_dotenv, monkeypatch):
    """Should fall back to YAML values when env vars are not set."""
    monkeypatch.delenv("AWS_BEDROCK_MODEL_ID", raising=False)
    monkeypatch.delenv("AWS_DEFAULT_REGION", raising=False)
    monkeypatch.delenv("AWS_ACCESS_KEY_ID", raising=False)
    monkeypatch.delenv("AWS_SECRET_ACCESS_KEY", raising=False)
    monkeypatch.delenv("AWS_SESSION_TOKEN", raising=False)

    config = {
        "model_id_env": "AWS_BEDROCK_MODEL_ID",
        "region_env": "AWS_DEFAULT_REGION",
        "fallback_model_id": "anthropic.claude-3-haiku-20240307-v1:0",
        "fallback_region": "eu-west-1",
        "temperature": 0.2,
        "top_p": 0.9,
        "max_tokens": 2000,
        "streaming": False,
    }

    create_bedrock_model(config)

    mock_session.assert_called_once_with(
        aws_access_key_id=None,
        aws_secret_access_key=None,
        aws_session_token=None,
        region_name="eu-west-1",
    )
    call_kwargs = mock_bedrock_model.call_args[1]
    assert call_kwargs["model_id"] == "anthropic.claude-3-haiku-20240307-v1:0"
    assert "region_name" not in call_kwargs
