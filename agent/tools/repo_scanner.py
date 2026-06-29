from pathlib import Path
from typing import Dict, List, Any, Optional
import json

# ##
# What files exist?
# What languages are in the repo?
# How much legacy code is there?
# Which files should be analyzed first?
# ##
LANGUAGE_BY_EXTENSION = {
    ".bas": "VB6",
    ".frm": "VB6",
    ".cls": "VB6",
    ".vbp": "VB6 Project",
    ".cs": "C#",
    ".csproj": "C# Project",
    ".sln": "Visual Studio Solution",
    ".vb": "VB.NET",
    ".config": "Config",
    ".json": "JSON",
    ".xml": "XML",
    ".sql": "SQL",
    ".md": "Markdown",
    ".txt": "Text",
}


LEGACY_EXTENSIONS = {
    ".bas",
    ".frm",
    ".cls",
    ".vbp",
    ".config",
}


IGNORE_FOLDERS = {
    ".git",
    ".venv",
    "__pycache__",
    "bin",
    "obj",
    ".vs",
    ".idea",
    ".vscode",
    "node_modules",
}


def detect_language(file_path: Path) -> str:
    """
    Detects the likely language or file type using the file extension.
    """
    return LANGUAGE_BY_EXTENSION.get(file_path.suffix.lower(), "Unknown")


def is_legacy_file(file_path: Path) -> bool:
    """
    Flags files that are likely part of a legacy VB6 / old .NET application.
    """
    return file_path.suffix.lower() in LEGACY_EXTENSIONS


def should_ignore(path: Path) -> bool:
    """
    Skips folders that should not be scanned.
    """
    return any(part in IGNORE_FOLDERS for part in path.parts)


def scan_repo(repo_path: str) -> Dict[str, Any]:
    """
    Walk through a repository folder and return a structured summary.

    Args:
        repo_path: Path to the repository or source code folder.

    Returns:
        Dictionary containing file inventory and summary statistics.
    """
    root = Path(repo_path)

    if not root.exists():
        raise FileNotFoundError(f"Path does not exist: {repo_path}")

    if not root.is_dir():
        raise NotADirectoryError(f"Path is not a directory: {repo_path}")

    files: List[Dict[str, Any]] = []
    language_counts: Dict[str, int] = {}
    legacy_file_count = 0

    for file_path in root.rglob("*"):
        if should_ignore(file_path):
            continue

        if not file_path.is_file():
            continue

        language = detect_language(file_path)
        legacy = is_legacy_file(file_path)

        if legacy:
            legacy_file_count += 1

        language_counts[language] = language_counts.get(language, 0) + 1

        files.append(
            {
                "path": str(file_path.relative_to(root)),
                "name": file_path.name,
                "extension": file_path.suffix.lower(),
                "language": language,
                "is_legacy_candidate": legacy,
                "size_bytes": file_path.stat().st_size,
            }
        )

    return {
        "repo_path": str(root),
        "total_files": len(files),
        "legacy_file_count": legacy_file_count,
        "language_counts": language_counts,
        "files": files,
    }


def save_scan_result(scan_result: Dict[str, Any], output_path: str) -> None:
    """
    Saves the scan result to a JSON file.
    """
    output = Path(output_path)
    output.parent.mkdir(parents=True, exist_ok=True)

    with output.open("w", encoding="utf-8") as f:
        json.dump(scan_result, f, indent=2)


if __name__ == "__main__":
    sample_repo_path = "examples/kst_lce_legacy_sample"
    output_path = "sandbox/repo_scan_result.json"

    result = scan_repo(sample_repo_path)
    save_scan_result(result, output_path)

    print(f"Scanned repo: {sample_repo_path}")
    print(f"Total files: {result['total_files']}")
    print(f"Legacy candidate files: {result['legacy_file_count']}")
    print(f"Saved result to: {output_path}")
