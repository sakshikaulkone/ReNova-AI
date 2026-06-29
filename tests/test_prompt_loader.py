import os
import tempfile
from pathlib import Path

from agent.tools.prompt_loader import load_text_file, load_yaml_file, render_prompt


def test_render_prompt_replaces_placeholder():
    template = "Here is the data:\n{{SCANNER_OUTPUT}}\nEnd."
    values = {"SCANNER_OUTPUT": '{"total_files": 5}'}
    result = render_prompt(template, values)
    assert '{"total_files": 5}' in result
    assert "{{SCANNER_OUTPUT}}" not in result


def test_render_prompt_multiple_placeholders():
    template = "Name: {{NAME}}, Age: {{AGE}}"
    values = {"NAME": "ReNova", "AGE": "1"}
    result = render_prompt(template, values)
    assert result == "Name: ReNova, Age: 1"


def test_render_prompt_no_matching_placeholder():
    template = "No placeholders here."
    values = {"SCANNER_OUTPUT": "data"}
    result = render_prompt(template, values)
    assert result == "No placeholders here."


def test_load_text_file_reads_content():
    with tempfile.NamedTemporaryFile(
        mode="w", suffix=".md", delete=False, encoding="utf-8"
    ) as f:
        f.write("Hello ReNova")
        tmp_path = f.name

    try:
        content = load_text_file(tmp_path)
        assert content == "Hello ReNova"
    finally:
        os.unlink(tmp_path)


def test_load_text_file_raises_on_missing():
    try:
        load_text_file("/nonexistent/path/file.md")
        assert False, "Should have raised FileNotFoundError"
    except FileNotFoundError:
        pass


def test_load_yaml_file_reads_dict():
    with tempfile.NamedTemporaryFile(
        mode="w", suffix=".yaml", delete=False, encoding="utf-8"
    ) as f:
        f.write("app:\n  name: ReNova\n  version: 1\n")
        tmp_path = f.name

    try:
        data = load_yaml_file(tmp_path)
        assert data["app"]["name"] == "ReNova"
        assert data["app"]["version"] == 1
    finally:
        os.unlink(tmp_path)


def test_load_yaml_file_empty_returns_empty_dict():
    with tempfile.NamedTemporaryFile(
        mode="w", suffix=".yaml", delete=False, encoding="utf-8"
    ) as f:
        f.write("")
        tmp_path = f.name

    try:
        data = load_yaml_file(tmp_path)
        assert data == {}
    finally:
        os.unlink(tmp_path)


def test_load_yaml_file_raises_on_missing():
    try:
        load_yaml_file("/nonexistent/config.yaml")
        assert False, "Should have raised FileNotFoundError"
    except FileNotFoundError:
        pass
