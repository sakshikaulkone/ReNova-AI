from agent.tools.prompt_loader import load_text_file, load_yaml_file, render_prompt


CONFIG_PATH = "agent/config/agent_config.yaml"


def test_modernization_request_file_loads():
    """The modernization request markdown file should load successfully."""
    config = load_yaml_file(CONFIG_PATH)
    request_path = config["paths"]["modernization_request_path"]
    content = load_text_file(request_path)
    assert len(content) > 0
    assert "KST-LCE" in content


def test_modernization_request_contains_key_sections():
    """The request file should contain the expected sections."""
    config = load_yaml_file(CONFIG_PATH)
    content = load_text_file(config["paths"]["modernization_request_path"])
    assert "Current State" in content
    assert "Modernization Goal" in content
    assert "SME" in content


def test_task_prompt_placeholder_renders():
    """{{MODERNIZATION_REQUEST}} should be replaced in the task template."""
    config = load_yaml_file(CONFIG_PATH)
    template = load_text_file(config["prompts"]["modernization_request_task"])
    assert "{{MODERNIZATION_REQUEST}}" in template

    rendered = render_prompt(template, {"MODERNIZATION_REQUEST": "Test request content"})
    assert "{{MODERNIZATION_REQUEST}}" not in rendered
    assert "Test request content" in rendered


def test_system_prompt_loads():
    """The system prompt should load and contain agent role definition."""
    config = load_yaml_file(CONFIG_PATH)
    content = load_text_file(config["prompts"]["modernization_request_system"])
    assert "Modernization Request Intake Agent" in content


def test_output_format_loads():
    """The output format should load and define the expected report sections."""
    config = load_yaml_file(CONFIG_PATH)
    content = load_text_file(config["prompts"]["modernization_request_output_format"])
    assert "Executive Summary" in content
    assert "Human Review Questions" in content
    assert "Recommended Next Step" in content


def test_config_has_modernization_request_paths():
    """agent_config.yaml should have the new modernization request paths."""
    config = load_yaml_file(CONFIG_PATH)
    assert "modernization_request_path" in config["paths"]
    assert "modernization_request_report_path" in config["paths"]


def test_config_has_modernization_request_prompts():
    """agent_config.yaml should have the new modernization request prompt paths."""
    config = load_yaml_file(CONFIG_PATH)
    prompts = config["prompts"]
    assert "modernization_request_system" in prompts
    assert "modernization_request_task" in prompts
    assert "modernization_request_output_format" in prompts
