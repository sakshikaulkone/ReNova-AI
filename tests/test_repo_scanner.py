import os
import tempfile
from pathlib import Path

import pytest

from agent.tools.repo_scanner import (
    analyze_csproj_file,
    build_modernization_summary,
    classify_modernization_category,
    detect_csharp_modernization_signals,
    detect_language,
    initialize_category_counts,
    is_legacy_file,
    scan_repo,
)


# ---------------------------------------------------------------------------
# Existing tests (kept)
# ---------------------------------------------------------------------------

def test_detect_language_for_vb6_file():
    assert detect_language(Path("FaultTrend.bas")) == "VB6"


def test_detect_language_for_csharp_file():
    assert detect_language(Path("FaultDataAccess.cs")) == "C#"


def test_is_legacy_file_for_vb6_module():
    assert is_legacy_file(Path("FaultTrend.bas")) is True


def test_scan_repo_finds_legacy_files():
    result = scan_repo("examples/kst_lce_legacy_sample")

    assert result["total_files"] >= 3
    assert result["legacy_file_count"] >= 2

    file_names = [f["name"] for f in result["files"]]
    assert "FaultTrend.bas" in file_names
    assert "FaultDataAccess.cs" in file_names
    assert "app.config" in file_names


# ---------------------------------------------------------------------------
# Requirement 1 & 2: modernization category classification
# ---------------------------------------------------------------------------

def test_vb6_file_classification():
    """FaultTrend.bas must be classified as VB6 Legacy."""
    category = classify_modernization_category(Path("FaultTrend.bas"))
    assert category == "VB6 Legacy"


def test_vb6_frm_classification():
    category = classify_modernization_category(Path("MainForm.frm"))
    assert category == "VB6 Legacy"


def test_old_csharp_classification_by_signals():
    """C# file containing SqlDataAdapter should be Old .NET Framework / C#."""
    content = "using System.Data.SqlClient;\nSqlDataAdapter adapter = new SqlDataAdapter();"
    category = classify_modernization_category(Path("FaultDataAccess.cs"), content)
    assert category == "Old .NET Framework / C#"


def test_app_config_classification():
    """app.config should be classified as Configuration."""
    category = classify_modernization_category(Path("app.config"))
    assert category == "Configuration"


def test_sql_file_classification():
    category = classify_modernization_category(Path("FaultEvents.sql"))
    assert category == "Database / SQL"


def test_markdown_classification():
    category = classify_modernization_category(Path("README.md"))
    assert category == "Documentation"


# ---------------------------------------------------------------------------
# Requirement 4: .csproj analysis
# ---------------------------------------------------------------------------

OLD_CSPROJ_CONTENT = '''\
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildToolsPath)\\Microsoft.CSharp.targets" />
  <PropertyGroup>
    <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
    <OutputType>WinExe</OutputType>
  </PropertyGroup>
</Project>'''

MODERN_CSPROJ_CONTENT = '''\
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
</Project>'''


def _write_tmp_csproj(content: str) -> Path:
    f = tempfile.NamedTemporaryFile(
        mode="w", suffix=".csproj", delete=False, encoding="utf-8"
    )
    f.write(content)
    f.close()
    return Path(f.name)


def test_old_csproj_detection():
    """Old-style .csproj with TargetFrameworkVersion v4.7.2 should be flagged."""
    tmp = _write_tmp_csproj(OLD_CSPROJ_CONTENT)
    try:
        result = analyze_csproj_file(tmp)
        assert result["project_style"] == "old-style"
        assert "4.7.2" in result["target_framework"]
        assert result["modernization_needed"] is True
        assert result["recommended_target"] == ".NET 8"
    finally:
        os.unlink(tmp)


def test_modern_csproj_detection():
    """SDK-style .csproj targeting net8.0 should NOT require modernization."""
    tmp = _write_tmp_csproj(MODERN_CSPROJ_CONTENT)
    try:
        result = analyze_csproj_file(tmp)
        assert result["project_style"] == "sdk-style"
        assert result["target_framework"] == "net8.0"
        assert result["modernization_needed"] is False
    finally:
        os.unlink(tmp)


def test_old_csproj_classifies_correctly():
    """Old .csproj content should classify as Old .NET Framework / C#."""
    category = classify_modernization_category(
        Path("LegacyApp.csproj"), OLD_CSPROJ_CONTENT
    )
    assert category == "Old .NET Framework / C#"


def test_modern_csproj_classifies_correctly():
    """SDK-style net8.0 .csproj should classify as Modern .NET."""
    category = classify_modernization_category(
        Path("ModernApp.csproj"), MODERN_CSPROJ_CONTENT
    )
    assert category == "Modern .NET"


# ---------------------------------------------------------------------------
# Requirement 5: C# modernization signal detection
# ---------------------------------------------------------------------------

def test_csharp_signals_sqlclient():
    content = "using System.Data.SqlClient;\nvar conn = new SqlConnection();"
    signals = detect_csharp_modernization_signals(content)
    assert "Uses System.Data.SqlClient" in signals
    assert "Uses SqlConnection" in signals


def test_csharp_signals_sqldataadapter():
    content = "SqlDataAdapter adapter = new SqlDataAdapter(cmd);"
    signals = detect_csharp_modernization_signals(content)
    assert "Uses SqlDataAdapter" in signals


def test_csharp_signals_system_web():
    content = "using System.Web;\nusing System.Web.UI;"
    signals = detect_csharp_modernization_signals(content)
    assert "Uses System.Web" in signals
    assert "Uses System.Web.UI (WebForms)" in signals


def test_csharp_signals_none_for_clean_file():
    content = "using System;\nnamespace Modern { public class Foo {} }"
    signals = detect_csharp_modernization_signals(content)
    assert signals == []


# ---------------------------------------------------------------------------
# Requirement 2 & 3: scan_repo returns category counts and grouped files
# ---------------------------------------------------------------------------

def test_scan_repo_returns_category_counts():
    result = scan_repo("examples/kst_lce_legacy_sample")
    assert "modernization_category_counts" in result
    counts = result["modernization_category_counts"]
    for cat in [
        "VB6 Legacy",
        "Old .NET Framework / C#",
        "Modern .NET",
        "Database / SQL",
        "Configuration",
        "Documentation",
        "Unknown / Other",
    ]:
        assert cat in counts, f"Missing category: {cat}"
    # We know there is at least one VB6 file
    assert counts["VB6 Legacy"] >= 1


def test_scan_repo_returns_files_by_category():
    result = scan_repo("examples/kst_lce_legacy_sample")
    assert "files_by_modernization_category" in result
    by_cat = result["files_by_modernization_category"]
    assert "FaultTrend.bas" in by_cat["VB6 Legacy"]
    assert "FaultEvents.sql" in by_cat["Database / SQL"]


def test_scan_repo_csproj_in_old_dotnet_category():
    result = scan_repo("examples/kst_lce_legacy_sample")
    by_cat = result["files_by_modernization_category"]
    assert "LegacyKstLce.csproj" in by_cat["Old .NET Framework / C#"]


# ---------------------------------------------------------------------------
# Requirement 6: modernization summary
# ---------------------------------------------------------------------------

def test_scan_repo_returns_modernization_summary():
    result = scan_repo("examples/kst_lce_legacy_sample")
    assert "modernization_summary" in result
    summary = result["modernization_summary"]
    assert summary["total_files_scanned"] == result["total_files"]
    assert "total_files_requiring_modernization" in summary
    assert "highest_risk_category" in summary
    assert "recommended_next_step" in summary


def test_modernization_summary_vb6_is_highest_risk():
    result = scan_repo("examples/kst_lce_legacy_sample")
    summary = result["modernization_summary"]
    assert summary["highest_risk_category"] == "VB6 Legacy"


def test_modernization_summary_requiring_count():
    result = scan_repo("examples/kst_lce_legacy_sample")
    summary = result["modernization_summary"]
    # We have at least 1 VB6 file, 1 C# old file, 1 config file — so count >= 3
    assert summary["total_files_requiring_modernization"] >= 3


# ---------------------------------------------------------------------------
# Requirement 4: dotnet_analysis in scan output
# ---------------------------------------------------------------------------

def test_scan_repo_dotnet_analysis_present():
    result = scan_repo("examples/kst_lce_legacy_sample")
    assert "dotnet_analysis" in result
    da = result["dotnet_analysis"]
    assert "projects" in da
    assert "packages_config_files" in da
    assert "config_files" in da


def test_scan_repo_detects_csproj_project():
    result = scan_repo("examples/kst_lce_legacy_sample")
    projects = result["dotnet_analysis"]["projects"]
    project_names = [p["path"] for p in projects]
    assert "LegacyKstLce.csproj" in project_names


def test_scan_repo_detects_packages_config():
    result = scan_repo("examples/kst_lce_legacy_sample")
    assert "packages.config" in result["dotnet_analysis"]["packages_config_files"]


def test_scan_repo_detects_app_config():
    result = scan_repo("examples/kst_lce_legacy_sample")
    assert "app.config" in result["dotnet_analysis"]["config_files"]


# ---------------------------------------------------------------------------
# Utility
# ---------------------------------------------------------------------------

def test_initialize_category_counts_has_all_categories():
    counts = initialize_category_counts()
    expected = [
        "VB6 Legacy",
        "Old .NET Framework / C#",
        "Modern .NET",
        "Database / SQL",
        "Configuration",
        "Documentation",
        "Unknown / Other",
    ]
    for cat in expected:
        assert cat in counts
        assert counts[cat] == 0
