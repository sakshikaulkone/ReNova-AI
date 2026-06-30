Analyze the following legacy source files in the context of the modernization request below.

Your goal is to build a structured understanding of what the legacy code does, how the components interact, and what must be preserved or reviewed before modernization can proceed.

## Modernization Context

```markdown
{{MODERNIZATION_REQUEST_REPORT}}
```

## Legacy Source Files

{{LEGACY_FILE_CONTENTS}}

## Your Analysis Should Cover

1. What each file appears to do (purpose and responsibility).
2. How the dongle validation and controller connection flow works.
3. How faults are read from the controller hardware.
4. How fault trends and recommendations are calculated (business rules).
5. How fault data is persisted (data access patterns).
6. What configuration values the code depends on.
7. What modernization risks exist (things that could break).
8. What logic must be preserved in any future rewrite.
9. What unknowns remain and what questions a human SME should answer.
10. What the next agent in the pipeline should do.

Be precise. Reference specific file names and function/method names. Separate facts from assumptions.
