# Agent Instructions

- Review this file and any nested `AGENTS.md` files before starting work.
- Before beginning any task, review `Codex/CollaborationGuidelines.txt`, `Codex/docs/CHANGELOG.md`, `Codex/docs/CollaborationAndDebugTips.txt`, and any relevant `AGENTS.md` files to gather context and instructions.
- `Codex/CollaborationGuidelines.txt` provides collaboration practices, while `Codex/docs/CollaborationAndDebugTips.txt` is a running log of what worked, what broke, and why.
- Update those documents when your changes add context or new guidance.
- After modifying code, request that a collaborator run `dotnet test` with `tests.runsettings`, share the results, and document any environment limitations (for example, missing Windows desktop support) so the team can rely on CI when needed.

## Architecture & Coding Standards
Summarized from the Codex Environment & Collaboration Guidelines:

- **Dependency Injection:** Register services, ViewModels, and factories in `AppHost/ServiceCollection` via `IServiceCollection` extensions.
- **Unit Tests:** Add xUnit tests for each new public method and edge cases; coordinate with collaborators to run `dotnet test` using `tests.runsettings` and share the results.
- **Logging:** Use `Microsoft.Extensions.Logging` with structured logs, logging start/end and errors per Codex logging policy.
- **Async/await:** Prefer async/await; avoid blocking and use `ConfigureAwait(false)` in libraries.
- **MVVM:** Keep Views free of logic; ViewModels expose `ICommand` and observable state.
- **Naming & Safety:** Use intention-revealing names, validate indexes/keys, guard against `null`, and reuse existing services.

Refer to `Codex/CollaborationGuidelines.txt` and the Codex Environment & Collaboration Guidelines for complete details.

Follow system/user environment instructions. When they conflict (e.g., read-only mode), note the limitation and skip the step.
