import os
import tempfile

from agent.agents.legacy_understanding_agent import load_legacy_files, load_modernization_context
from agent.tools.prompt_loader import load_text_file, load_yaml_file


CONFIG_PATH = "agent/config/agent_config.yaml"


def test_load_legacy_files_includes_file_headers():
    """load_legacy_files should include file headers and content for each file."""
    with tempfile.NamedTemporaryFile(
        mode="w", suffix=".bas", delete=False, encoding="utf-8"
    ) as f1:
        f1.write("Public Function Foo() As String\nEnd Function")
        path1 = f1.name

    with tempfile.NamedTemporaryFile(
        mode="w", suffix=".cs", delete=False, encoding="utf-8"
    ) as f2:
        f2.write("public class Bar {}")
        path2 = f2.name

    try:
        result = load_legacy_files([path1, path2])
        assert "--- FILE:" in result
        assert "Public Function Foo()" in result
        assert "public class Bar" in result
    finally:
        os.unlink(path1)
        os.unlink(path2)


def test_load_legacy_files_handles_missing_file():
    """load_legacy_files should include a warning for missing files, not crash."""
    result = load_legacy_files(["/nonexistent/missing_file.bas"])
    assert "WARNING" in result
    assert "missing_file.bas" in result


def test_load_legacy_files_mixed_existing_and_missing():
    """Should handle a mix of existing and missing files."""
    with tempfile.NamedTemporaryFile(
        mode="w", suffix=".cs", delete=False, encoding="utf-8"
    ) as f:
        f.write("namespace Test {}")
        existing_path = f.name

    try:
        result = load_legacy_files([existing_path, "/no/such/file.bas"])
        assert "namespace Test" in result
        assert "WARNING" in result
    finally:
        os.unlink(existing_path)


def test_legacy_understanding_prompt_has_required_placeholders():
    """The task prompt must contain both required placeholders."""
    config = load_yaml_file(CONFIG_PATH)
    template = load_text_file(config["prompts"]["legacy_understanding_task"])
    assert "{{MODERNIZATION_REQUEST_REPORT}}" in template
    assert "{{LEGACY_FILE_CONTENTS}}" in template


def test_config_has_legacy_understanding_paths():
    """agent_config.yaml should have all legacy understanding config keys."""
    config = load_yaml_file(CONFIG_PATH)

    assert "legacy_understanding_report_path" in config["paths"]
    assert "legacy_files_to_understand" in config["paths"]
    assert isinstance(config["paths"]["legacy_files_to_understand"], list)
    assert len(config["paths"]["legacy_files_to_understand"]) >= 1

    prompts = config["prompts"]
    assert "legacy_understanding_system" in prompts
    assert "legacy_understanding_task" in prompts
    assert "legacy_understanding_output_format" in prompts


def test_load_modernization_context_uses_fallback():
    """Should fall back to raw request file if report doesn't exist."""
    config = load_yaml_file(CONFIG_PATH)
    # Point report to a non-existent path to force fallback
    config["paths"]["modernization_request_report_path"] = "/nonexistent/report.md"
    result = load_modernization_context(config)
    assert "KST-LCE" in result
