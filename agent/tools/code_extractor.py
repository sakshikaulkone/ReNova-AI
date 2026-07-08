"""
Utility to extract code blocks from code conversion Markdown reports
and save them as individual C# files.
"""
import re
from pathlib import Path
from typing import List, Tuple


def extract_code_blocks(markdown_content: str) -> List[Tuple[str, str]]:
    """
    Extracts code blocks and their file paths from Markdown report.

    Returns list of (file_path, code_content) tuples.
    """
    code_blocks = []

    # Pattern to match: ### File: path/to/File.cs followed by ```csharp code ```
    pattern = r'###\s+File:\s+([^\n]+)\n+```(?:csharp)?\n(.*?)```'

    matches = re.finditer(pattern, markdown_content, re.DOTALL)

    for match in matches:
        file_path = match.group(1).strip()
        code_content = match.group(2).strip()
        code_blocks.append((file_path, code_content))

    return code_blocks


def save_code_files(
    code_blocks: List[Tuple[str, str]],
    output_base_dir: str,
    namespace_prefix: str = "KstLce.Modernized"
) -> List[str]:
    """
    Saves extracted code blocks as individual .cs files.

    Returns list of created file paths.
    """
    base_path = Path(output_base_dir)
    base_path.mkdir(parents=True, exist_ok=True)

    created_files = []

    for file_path, code_content in code_blocks:
        # Clean up file path and ensure it's relative
        file_path = file_path.replace("\\", "/").strip()

        # Create full path under base directory
        full_path = base_path / file_path

        # Create parent directories
        full_path.parent.mkdir(parents=True, exist_ok=True)

        # Write the code file
        full_path.write_text(code_content, encoding="utf-8")

        created_files.append(str(full_path))
        print(f"  Created: {full_path}")

    return created_files


def extract_and_save_from_report(
    report_path: str,
    output_dir: str
) -> int:
    """
    Reads a code conversion report and extracts all code blocks to individual files.

    Returns the number of files created.
    """
    report_file = Path(report_path)

    if not report_file.exists():
        raise FileNotFoundError(f"Report file not found: {report_path}")

    print(f"Reading report: {report_path}")
    markdown_content = report_file.read_text(encoding="utf-8")

    print("Extracting code blocks...")
    code_blocks = extract_code_blocks(markdown_content)

    if not code_blocks:
        print("WARNING: No code blocks found in report.")
        print("The report may be incomplete or not contain Appendix A.")
        return 0

    print(f"Found {len(code_blocks)} code blocks")
    print(f"Saving to: {output_dir}")

    created_files = save_code_files(code_blocks, output_dir)

    print(f"\nSuccessfully created {len(created_files)} files")
    return len(created_files)


if __name__ == "__main__":
    # Example usage
    report_path = "sandbox/reports/code_conversion_report.md"
    output_dir = "sandbox/generated_code"

    try:
        count = extract_and_save_from_report(report_path, output_dir)
        print(f"\nDone! {count} C# files extracted.")
    except Exception as e:
        print(f"Error: {e}")
        exit(1)
