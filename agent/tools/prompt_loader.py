from pathlib import Path
from typing import Any, Dict

import yaml


def load_text_file(path: str) -> str:
    """Reads a text/markdown file and returns its content as a string."""
    file_path = Path(path)
    if not file_path.exists():
        raise FileNotFoundError(f"Prompt file not found: {path}")
    return file_path.read_text(encoding="utf-8")


def load_yaml_file(path: str) -> Dict[str, Any]:
    """Reads a YAML configuration file and returns it as a dictionary."""
    file_path = Path(path)
    if not file_path.exists():
        raise FileNotFoundError(f"YAML config file not found: {path}")
    with file_path.open("r", encoding="utf-8") as f:
        data = yaml.safe_load(f)
    return data if data is not None else {}


def render_prompt(template: str, values: Dict[str, str]) -> str:
    """
    Replaces placeholders like {{KEY}} in a template string with values
    from the provided dictionary.
    """
    result = template
    for key, value in values.items():
        placeholder = "{{" + key + "}}"
        result = result.replace(placeholder, value)
    return result
