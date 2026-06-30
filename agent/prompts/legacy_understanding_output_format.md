Format your response as a Markdown report with exactly these sections:

# Legacy Understanding Report

## 1. Executive Summary

A 2-3 sentence overview of what the legacy codebase does and its modernization readiness.

## 2. Files Analyzed

List each file with a one-line description of its purpose.

## 3. Dongle and Controller Connection Logic

Explain how dongle validation works and how the controller connection is established.

## 4. Fault Reading Logic

Explain how faults are read from the controller and how the raw buffer is parsed.

## 5. Fault Trend and Recommendation Rules

Describe the business rules for fault analysis and what recommendations are generated.

## 6. Data Access and Persistence Logic

Explain how fault data is stored and retrieved from the database.

## 7. Configuration Dependencies

List all configuration keys the code depends on and their purpose.

## 8. Modernization Risks

Identify what could break during modernization and what requires careful handling.

## 9. Logic That Must Be Preserved

Identify business rules and behaviors that must survive any rewrite.

## 10. Unknowns and Human Review Questions

List 5-8 questions that a Subject Matter Expert must answer before modernization proceeds.

## 11. Recommended Next Step

One clear recommendation for what the modernization pipeline should do next.
