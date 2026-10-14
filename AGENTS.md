# MODERN ERP Server — ASP.NET Core and C# Instructions

## Scope

These instructions apply to all files under `server/`.

Read the repository root `AGENTS.md` first. Also read `database/AGENTS.md` whenever a backend change creates, calls, or modifies a SQL Server object. Read `frontend/AGENTS.md` whenever a public API contract affects Angular.

## Inspect before editing

Before implementing a backend change, locate and inspect:

- The nearest similar controller.
- Existing request and response DTOs.
- Application interfaces or services.
- Repository interfaces and implementations.
- Connection-factory and transaction patterns.
- Existing validation and authorization behavior.
- Existing exception-handling and API error-response conventions.
- Relevant unit or integration tests.
- Relevant stored procedures, table types, views, functions, or tables under `database/`.

Do not generate a parallel architecture when a working project pattern already exists.

## Controller rules

- Keep controllers thin and focused on HTTP concerns.
- Do not put SQL, connection management, transaction logic, or substantial business logic in controllers.
- Use explicit routes and appropriate HTTP verbs.
- Use request and response DTOs rather than returning database entities directly.
- Validate route values, query values, and request bodies.
- Preserve the project's established response and error format.
- Return appropriate status codes for validation errors, not found, conflicts, unauthorized access, forbidden operations, and successful creation or updates.
- Pass `CancellationToken` through asynchronous operations where the existing architecture supports it.
- Do not catch exceptions only to hide them or return a false success response.

## DTO and API contract rules

- Use clear request DTOs for commands and query DTOs for filters.
- Use response DTOs designed for the client contract.
- Respect nullable reference types.
- Keep property names and JSON serialization behavior compatible with Angular.
- Use decimal types for monetary, quantity, tax, discount, rate, and total values.
- Do not use floating-point types for accounting values.
- Validate string lengths and required fields against SQL Server definitions.
- Define date semantics clearly. Use `DateOnly` only when the project already supports it consistently; otherwise preserve the existing date type and serialization pattern.
- Do not change public route names or response shapes without updating all clients and tests.

## Application and business logic

- Place reusable business rules in the established application or service layer.
- Enforce critical rules on the server even when the frontend also validates them.
- Preserve document statuses and allowed transitions.
- Verify referenced supplier, branch, warehouse, account, customer, item, term, user, and source document records as required by the module.
- Prevent duplicate document numbers and duplicate processing.
- For posting operations, verify that the source document is eligible and not already posted.
- For update operations, preserve concurrency or row-version behavior when present.
- Do not allow client-supplied calculated totals to override authoritative server calculations without verification.

## Repository and data-access rules

- Preserve the established repository and connection-factory pattern.
- Use Dapper and stored procedures where that is the existing module convention.
- Do not introduce Entity Framework Core unless explicitly requested.
- Use parameterized commands only.
- Do not concatenate user input into SQL.
- Open database connections as late as practical and dispose of them correctly.
- Use asynchronous database APIs where supported.
- Keep command timeout behavior consistent with the project; do not increase timeouts merely to mask inefficient queries.
- Map database nulls and legacy column names explicitly.
- Do not return successful results when zero rows were unexpectedly affected.

## Transaction and batch-saving rules

Use one explicit database transaction when a business operation must be atomic, including typical header-detail saves, posting, receiving, liquidation, and accounting-entry creation.

For batch operations:

- Validate the header and all detail rows before committing.
- Reject empty detail collections when details are mandatory.
- Reject duplicate line identifiers or duplicate business keys.
- Ensure the header and details use the same document number.
- Roll back the entire operation when any required step fails.
- Return the generated document number and saved row count when useful to the caller.
- Use a table-valued parameter when it matches the existing high-volume saving pattern.
- Keep TVP column order, SQL types, lengths, precision, scale, and nullability exactly aligned with the C# row structure.

## Error handling and logging

- Follow the project's global exception-handling pattern.
- Log enough context to diagnose failures without logging secrets or excessive personal data.
- Include stable identifiers such as document number, branch, operation, and correlation identifier when available.
- Do not expose stack traces, SQL text, connection strings, or internal exception details to production clients.
- Use conflict responses for duplicate or invalid state-transition scenarios when consistent with the existing API design.
- Preserve the original exception as the inner exception when adding contextual exceptions.

## Authentication and authorization

- Apply the established authentication and authorization mechanisms.
- Enforce permissions on the API, not only in Angular.
- Validate the authenticated user identity instead of trusting a free-form `userId` from the request.
- Enforce branch or entity restrictions when applicable.
- Do not weaken authorization to simplify testing.

## Performance

For listing endpoints and large tables:

- Use server-side paging, filtering, and sorting.
- Return the total count required by the Angular grid when applicable.
- Avoid loading all records into memory before paging.
- Select only required columns.
- Avoid N+1 database calls.
- Review indexes and query plans for new high-volume filters.
- Preserve cancellation support for long-running read operations.
- Do not add broad `DISTINCT`, scalar functions, or wildcard searches as a substitute for correct joins and indexing without reviewing their cost.

## Testing

Add or update focused tests for:

- Controller contract behavior where practical.
- Business validation.
- Status transitions.
- Duplicate prevention.
- Repository parameter mapping.
- Transaction rollback behavior.
- Null, empty, boundary, and decimal-precision cases.
- Authorization-sensitive behavior.

Do not delete or weaken existing tests merely to make a change pass.

## Verification

From the repository root or the appropriate solution directory, discover the actual solution and project names, then run the applicable commands:

```powershell
dotnet restore
dotnet build
dotnet test
```

Also inspect compiler warnings, nullable warnings, analyzer findings, and test failures. Report any check that could not run.

## Completion checklist

Before finishing a server task, confirm:

- Controller route and Angular URL match.
- HTTP verb and payload location match.
- Request and response DTOs match the TypeScript contracts.
- C# parameter names and types match the stored procedure or SQL object.
- Transaction boundaries are correct.
- Authorization and validation remain server-side.
- Error responses follow the project convention.
- Relevant builds and tests pass.
