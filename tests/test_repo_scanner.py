from pathlib import Path

from agent.tools.repo_scanner import detect_language, is_legacy_file, scan_repo


def test_detect_language_for_vb6_file():
    assert detect_language(Path("FaultTrend.bas")) == "VB6"


def test_detect_language_for_csharp_file():
    assert detect_language(Path("FaultDataAccess.cs")) == "C#"


def test_is_legacy_file_for_vb6_module():
    assert is_legacy_file(Path("FaultTrend.bas")) is True


def test_is_legacy_file_for_non_legacy():
    assert is_legacy_file(Path("README.md")) is False


def test_scan_repo_finds_legacy_files():
    result = scan_repo("examples/kst_lce_legacy_sample")

    assert result["total_files"] >= 3
    assert result["legacy_file_count"] >= 2

    file_names = [f["name"] for f in result["files"]]
    assert "FaultTrend.bas" in file_names
    assert "FaultDataAccess.cs" in file_names
    assert "app.config" in file_names


def test_scan_repo_returns_language_counts():
    result = scan_repo("examples/kst_lce_legacy_sample")
    assert "language_counts" in result
    assert result["language_counts"].get("VB6", 0) >= 1


def test_scan_repo_file_entry_structure():
    result = scan_repo("examples/kst_lce_legacy_sample")
    file_entry = result["files"][0]
    assert "path" in file_entry
    assert "name" in file_entry
    assert "extension" in file_entry
    assert "language" in file_entry
    assert "is_legacy_candidate" in file_entry
    assert "size_bytes" in file_entry
