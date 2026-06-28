from agent.tools.repo_scanner import detect_language, is_legacy_file, scan_repo
from pathlib import Path


def test_detect_language_for_vb6_file():
    file_path = Path("FaultTrend.bas")
    assert detect_language(file_path) == "VB6"


def test_detect_language_for_csharp_file():
    file_path = Path("FaultDataAccess.cs")
    assert detect_language(file_path) == "C#"


def test_is_legacy_file_for_vb6_module():
    file_path = Path("FaultTrend.bas")
    assert is_legacy_file(file_path) is True


def test_scan_repo_finds_legacy_files():
    result = scan_repo("examples/kst_lce_legacy_sample")

    assert result["total_files"] >= 3
    assert result["legacy_file_count"] >= 2

    file_names = [file["name"] for file in result["files"]]

    assert "FaultTrend.bas" in file_names
    assert "FaultDataAccess.cs" in file_names
    assert "app.config" in file_names