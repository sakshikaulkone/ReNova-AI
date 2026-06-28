from pathlib import Path
from typing import Dict, List, Any, Optional
import json
import re


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
    ".yaml": "YAML",
    ".yml": "YAML",
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


MODERNIZATION_CATEGORIES = [
    "VB6 Legacy",
    "Old .NET Framework / C#",
    "Modern .NET",
    "Database / SQL",
    "Configuration",
    "Documentation",
    "Unknown / Other",
]

VB6_EXTENSIONS = {".bas", ".frm", ".cls", ".vbp"}
SQL_EXTENSIONS = {".sql"}
DOC_EXTENSIONS = {".md", ".txt"}
CONFIG_EXTENSIONS = {".config", ".json", ".xml", ".yaml", ".yml"}

# Patterns that indicate old .NET Framework usage in C# files
CSHARP_OLD_SIGNALS: Dict[str, str] = {
    "using System.Web": "Uses System.Web",
    "using System.Data.SqlClient": "Uses System.Data.SqlClient",
    "ConfigurationManager.AppSettings": "Uses ConfigurationManager.AppSettings",
    "AppDomain.CurrentDomain": "Uses AppDomain.CurrentDomain",
    "HttpContext.Current": "Uses HttpContext.Current",
    "SqlConnection": "Uses SqlConnection",
    "SqlCommand": "Uses SqlCommand",
    "SqlDataAdapter": "Uses SqlDataAdapter",
    "System.Windows.Forms": "Uses System.Windows.Forms",
    "Page_Load": "Uses Page_Load (WebForms)",
    "System.Web.UI": "Uses System.Web.UI (WebForms)",
}


def detect_language(file_path: Path) -> str:
    """Detects the likely language or file type using the file extension."""
    return LANGUAGE_BY_EXTENSION.get(file_path.suffix.lower(), "Unknown")


def is_legacy_file(file_path: Path) -> bool:
    """Flags files that are likely part of a legacy VB6 / old .NET application."""
    return file_path.suffix.lower() in LEGACY_EXTENSIONS


def should_ignore(path: Path) -> bool:
    """Skips folders that should not be scanned."""
    return any(part in IGNORE_FOLDERS for part in path.parts)


def initialize_category_counts() -> Dict[str, int]:
    """Returns a dict with all modernization categories initialized to 0."""
    return {category: 0 for category in MODERNIZATION_CATEGORIES}


def detect_csharp_modernization_signals(content: str) -> List[str]:
    """
    Checks a C# file's content for old .NET Framework patterns.
    Returns a list of human-readable signal descriptions found.
    """
    return [
        description
        for pattern, description in CSHARP_OLD_SIGNALS.items()
        if pattern in content
    ]


def analyze_csproj_file(file_path: Path) -> Dict[str, Any]:
    """
    Inspects a .csproj file and returns project style, target framework,
    and modernization guidance.
    """
    try:
        content = file_path.read_text(encoding="utf-8", errors="ignore")
    except Exception:
        return {
            "path": file_path.name,
            "project_style": "unknown",
            "target_framework": "unknown",
            "modernization_needed": True,
            "recommended_target": ".NET 8",
            "risk": "High",
            "signals": ["Could not read project file"],
        }

    signals: List[str] = []

    # Detect SDK-style vs old-style project
    is_sdk_style = bool(re.search(r'<Project\s+Sdk=', content))
    project_style = "sdk-style" if is_sdk_style else "old-style"

    if not is_sdk_style:
        signals.append("Old non-SDK-style project file")

    # Detect TargetFramework or TargetFrameworks (SDK-style)
    target_framework = "unknown"
    tf_match = re.search(r'<TargetFrameworks?>(.*?)</TargetFrameworks?>', content)
    if tf_match:
        target_framework = tf_match.group(1).strip()

    # Detect TargetFrameworkVersion (old-style)
    tfv_match = re.search(r'<TargetFrameworkVersion>(.*?)</TargetFrameworkVersion>', content)
    if tfv_match:
        version = tfv_match.group(1).strip()
        target_framework = f".NET Framework {version.lstrip('v')}"
        signals.append(f"Uses TargetFrameworkVersion {version}")

    # Decide if modernization is needed
    modernization_needed = False
    if project_style == "old-style":
        modernization_needed = True
    elif target_framework.startswith(".NET Framework"):
        modernization_needed = True
    elif any(
        target_framework.lower().startswith(prefix)
        for prefix in ("netframework", "net4", "net3", "net2", "net1")
    ):
        modernization_needed = True

    risk = "High" if modernization_needed else "Low"

    return {
        "path": file_path.name,
        "project_style": project_style,
        "target_framework": target_framework,
        "modernization_needed": modernization_needed,
        "recommended_target": ".NET 8",
        "risk": risk,
        "signals": signals,
    }


def classify_modernization_category(
    file_path: Path,
    content: Optional[str] = None,
    packages_config_exists: bool = False,
) -> str:
    """
    Classifies a file into one of the modernization categories based on
    its extension, name, and optionally its content.
    """
    ext = file_path.suffix.lower()
    name = file_path.name.lower()

    if ext in VB6_EXTENSIONS:
        return "VB6 Legacy"

    if ext in SQL_EXTENSIONS:
        return "Database / SQL"

    if ext in DOC_EXTENSIONS:
        return "Documentation"

    # Configuration: special names or config extensions
    if name == "appsettings.json" or ext in CONFIG_EXTENSIONS:
        return "Configuration"

    # C# project files — inspect content when available
    if ext == ".csproj":
        if content:
            is_sdk = bool(re.search(r'<Project\s+Sdk=', content))
            tfv_match = re.search(r'<TargetFrameworkVersion>(.*?)</TargetFrameworkVersion>', content)
            tf_match = re.search(r'<TargetFrameworks?>(.*?)</TargetFrameworks?>', content)

            if not is_sdk or tfv_match:
                return "Old .NET Framework / C#"

            if tf_match:
                fw = tf_match.group(1).strip().lower()
                if any(fw.startswith(p) for p in ("net6", "net7", "net8", "net9")):
                    return "Modern .NET"
                if any(fw.startswith(p) for p in ("netframework", "net4", "net3", "net2", "net1")):
                    return "Old .NET Framework / C#"
        return "Old .NET Framework / C#"

    # C# source files — check content for old patterns or modern patterns
    if ext == ".cs":
        if content:
            if detect_csharp_modernization_signals(content):
                return "Old .NET Framework / C#"
            if "WebApplication.CreateBuilder" in content or "Microsoft.AspNetCore" in content:
                return "Modern .NET"
        # Without clear modern signals, default based on project context
        return "Old .NET Framework / C#"

    return "Unknown / Other"


def _get_recommended_action(category: str, signals: List[str]) -> str:
    """Returns a recommended action string based on the modernization category."""
    if category == "VB6 Legacy":
        return "Rewrite in C# / .NET 8; extract business logic first."
    if category == "Old .NET Framework / C#":
        base = "Review for .NET 8 migration"
        if signals:
            return base + "; replace old data access/configuration patterns where needed."
        return base + "."
    if category == "Modern .NET":
        return "No immediate action required; keep dependencies up to date."
    if category == "Database / SQL":
        return "Review for compatibility with target .NET 8 data access layer."
    if category == "Configuration":
        return "Migrate to appsettings.json / environment variables where applicable."
    if category == "Documentation":
        return "Update documentation to reflect modernization changes."
    return "Review and classify manually."


def _determine_modernization_needed(category: str) -> bool:
    """Returns True if files in this category typically require modernization work."""
    return category in ("VB6 Legacy", "Old .NET Framework / C#", "Configuration")


def build_modernization_summary(
    files: List[Dict[str, Any]],
    category_counts: Dict[str, int],
) -> Dict[str, Any]:
    """Builds the top-level modernization summary for the scan result."""
    total = len(files)
    requiring_mod = sum(1 for f in files if f.get("modernization_needed", False))

    highest_risk = "Unknown / Other"
    if category_counts.get("VB6 Legacy", 0) > 0:
        highest_risk = "VB6 Legacy"
    elif category_counts.get("Old .NET Framework / C#", 0) > 0:
        highest_risk = "Old .NET Framework / C#"

    return {
        "total_files_scanned": total,
        "total_files_requiring_modernization": requiring_mod,
        "highest_risk_category": highest_risk,
        "recommended_next_step": (
            "Start with VB6 Legacy and Old .NET Framework / C# files. "
            "Build a business logic extraction agent next."
        ),
    }


def scan_repo(repo_path: str) -> Dict[str, Any]:
    """
    Walk through a repository folder and return a structured summary
    with modernization classification for each file.

    Args:
        repo_path: Path to the repository or source code folder.

    Returns:
        Dictionary containing file inventory, category counts, grouped files,
        .NET project analysis, and modernization summary.
    """
    root = Path(repo_path)

    if not root.exists():
        raise FileNotFoundError(f"Path does not exist: {repo_path}")

    if not root.is_dir():
        raise NotADirectoryError(f"Path is not a directory: {repo_path}")

    # Pre-scan to check for packages.config (affects C# classification)
    packages_config_exists = any(
        f.name.lower() == "packages.config"
        for f in root.rglob("*")
        if f.is_file() and not should_ignore(f)
    )

    files: List[Dict[str, Any]] = []
    language_counts: Dict[str, int] = {}
    legacy_file_count = 0
    category_counts = initialize_category_counts()
    files_by_category: Dict[str, List[str]] = {cat: [] for cat in MODERNIZATION_CATEGORIES}
    dotnet_projects: List[Dict[str, Any]] = []
    packages_config_files: List[str] = []
    config_files: List[str] = []

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

        ext = file_path.suffix.lower()
        name_lower = file_path.name.lower()

        # Read content only for file types that need it
        content: Optional[str] = None
        if ext in (".cs", ".csproj"):
            try:
                content = file_path.read_text(encoding="utf-8", errors="ignore")
            except Exception:
                content = None

        category = classify_modernization_category(file_path, content, packages_config_exists)
        modernization_needed = _determine_modernization_needed(category)

        signals: List[str] = []
        if ext == ".cs" and content:
            signals = detect_csharp_modernization_signals(content)

        recommended_action = _get_recommended_action(category, signals)

        category_counts[category] = category_counts.get(category, 0) + 1
        files_by_category[category].append(file_path.name)

        if ext == ".csproj":
            dotnet_projects.append(analyze_csproj_file(file_path))

        if name_lower == "packages.config":
            packages_config_files.append(file_path.name)
        if name_lower in ("app.config", "web.config"):
            config_files.append(file_path.name)

        file_entry: Dict[str, Any] = {
            "path": str(file_path.relative_to(root)),
            "name": file_path.name,
            "extension": ext,
            "language": language,
            "modernization_category": category,
            "modernization_needed": modernization_needed,
            "recommended_action": recommended_action,
            "size_bytes": file_path.stat().st_size,
        }

        if signals:
            file_entry["signals"] = signals

        files.append(file_entry)

    modernization_summary = build_modernization_summary(files, category_counts)

    return {
        "repo_path": str(root),
        "total_files": len(files),
        "legacy_file_count": legacy_file_count,
        "language_counts": language_counts,
        "modernization_category_counts": category_counts,
        "files_by_modernization_category": files_by_category,
        "modernization_summary": modernization_summary,
        "dotnet_analysis": {
            "projects": dotnet_projects,
            "packages_config_files": packages_config_files,
            "config_files": config_files,
        },
        "files": files,
    }


def save_scan_result(scan_result: Dict[str, Any], output_path: str) -> None:
    """Saves the scan result to a JSON file."""
    output = Path(output_path)
    output.parent.mkdir(parents=True, exist_ok=True)

    with output.open("w", encoding="utf-8") as f:
        json.dump(scan_result, f, indent=2)


if __name__ == "__main__":
    sample_repo_path = "examples/kst_lce_legacy_sample"
    output_path = "sandbox/repo_scan_result.json"

    result = scan_repo(sample_repo_path)
    save_scan_result(result, output_path)

    summary = result["modernization_summary"]
    counts = result["modernization_category_counts"]
    projects = result["dotnet_analysis"]["projects"]

    print(f"\nScanned repo: {sample_repo_path}")
    print(f"Total files: {result['total_files']}")
    print(f"Files requiring modernization: {summary['total_files_requiring_modernization']}")

    print("\nModernization category counts:")
    for cat, count in counts.items():
        print(f"  - {cat}: {count}")

    if projects:
        print("\n.NET projects found:")
        for proj in projects:
            print(
                f"  - {proj['path']} | {proj['project_style']} | "
                f"{proj['target_framework']} | modernization needed: {proj['modernization_needed']}"
            )

    print(f"\nSaved result to: {output_path}")
