Create a first-pass modern .NET-style conversion proposal for the selected legacy files.

## Modernization Request Context

```markdown
{{MODERNIZATION_REQUEST_CONTEXT}}
```

## SME Answers

```markdown
{{SME_ANSWERS}}
```

## Legacy Understanding Report

```markdown
{{LEGACY_UNDERSTANDING_REPORT}}
```

## Legacy Source Files to Convert

{{LEGACY_FILE_CONTENTS}}

## Conversion Instructions

Based on the above context, produce a code conversion report and sample modernized C# code.

Focus on:
- Read-only fault retrieval and fault recommendation logic.
- Replacing direct dongle/COM dependency with an interface/adapter abstraction.
- Converting old ADO.NET patterns to modern repository/service patterns.
- Converting ConfigurationManager usage to .NET Options pattern.
- Preserving fault trend business rules (recurring fault threshold, high severity threshold).

Do NOT:
- Implement real controller protocol details.
- Include unsafe controller write operations.
- Produce final architecture for mobile/web yet.
- Hardcode credentials or secrets.

Produce sample modernized C# code that could later fit into a .NET 8 backend or service layer. Use interfaces, dependency injection, and clear separation of concerns.

For each proposed modern file, include:
- The file name and namespace.
- The full C# code block.
- A brief explanation of what it replaces from the legacy code.
