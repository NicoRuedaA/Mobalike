# Skill Registry

**Delegator use only.** Any agent that launches sub-agents reads this registry to resolve compact rules, then injects them directly into sub-agent prompts. Sub-agents do NOT read this registry or individual SKILL.md files.

## User Skills

| Trigger | Skill | Path |
|---------|-------|------|
| When writing Go tests, using teatest, or adding test coverage | go-testing | C:\Users\Nico\.config\opencode\skills\go-testing\SKILL.md |
| When user asks to create a new skill, add agent instructions, or document patterns for AI | skill-creator | C:\Users\Nico\.config\opencode\skills\skill-creator\SKILL.md |
| When creating a GitHub issue, reporting a bug, or requesting a feature | issue-creation | C:\Users\Nico\.config\opencode\skills\issue-creation\SKILL.md |
| When creating a pull request, opening a PR, or preparing changes for review | branch-pr | C:\Users\Nico\.config\opencode\skills\branch-pr\SKILL.md |
| When saying "judgment day", "dual review", "juzgar", or "que lo juzguen" | judgment-day | C:\Users\Nico\.config\opencode\skills\judgment-day\SKILL.md |
| When working with Unity projects through MCP for Unity | unity-mcp-orchestrator | C:\Users\Nico\.config\opencode\skills\unity-mcp\SKILL.md |
| When user wants to initialize SDD in a project, or says "sdd init", "iniciar sdd" | sdd-init | C:\Users\Nico\.config\opencode\skills\sdd-init\SKILL.md |
| When the orchestrator launches you to write or update specs for a change | sdd-spec | C:\Users\Nico\.config\opencode\skills\sdd-spec\SKILL.md |
| When the orchestrator launches you to create or update the task breakdown for a change | sdd-tasks | C:\Users\Nico\.config\opencode\skills\sdd-tasks\SKILL.md |
| When the orchestrator launches you to write or update the technical design for a change | sdd-design | C:\Users\Nico\.config\opencode\skills\sdd-design\SKILL.md |
| When the orchestrator launches you to implement one or more tasks from a change | sdd-apply | C:\Users\Nico\.config\opencode\skills\sdd-apply\SKILL.md |
| When the orchestrator launches you to validate implementation against specs | sdd-verify | C:\Users\Nico\.config\opencode\skills\sdd-verify\SKILL.md |
| When the orchestrator launches you to sync delta specs to main specs and archive a completed change | sdd-archive | C:\Users\Nico\.config\opencode\skills\sdd-archive\SKILL.md |
| When the orchestrator launches you to think through a feature, investigate the codebase, or clarify requirements | sdd-explore | C:\Users\Nico\.config\opencode\skills\sdd-explore\SKILL.md |
| When the orchestrator launches you to create or update a proposal for a change | sdd-propose | C:\Users\Nico\.config\opencode\skills\sdd-propose\SKILL.md |
| When the orchestrator launches you to onboard a user through the full SDD cycle | sdd-onboard | C:\Users\Nico\.config\opencode\skills\sdd-onboard\SKILL.md |

## Compact Rules

Pre-digested rules per skill. Delegators copy matching blocks into sub-agent prompts as `## Project Standards (auto-resolved)`.

### unity-mcp-orchestrator
- Always read `mcpforunity://project/info` before UI/input operations to detect packages
- Wait for compilation after `create_script` or `script_apply_edits` — they auto-trigger compile
- Use `batch_execute` for 10-100x faster multiple operations (max 25 per batch)
- Use `manage_camera(action="screenshot", include_image=True)` to verify visual results
- Check `mcpforunity://editor/state` before complex operations
- Prefab instantiation: use `manage_gameobject(action="create", prefab_path="...")`, NOT `manage_prefabs`
- UI Toolkit: use `manage_ui`; uGUI: use `batch_execute` with `manage_gameobject` + `manage_components`
- Always use `<ui:Style>` prefix in UXML, not bare `<Style>`

### go-testing
- Table-driven tests: `tests := []struct{ name string; ... }` pattern
- Golden file testing: use `teatest` for TUI tests
- `require` over `assert` for clearer failure messages
- Mock interfaces, not concrete types
- Use `t.Run(name, func(t *testing.T))` for subtests

### skill-creator
- Create skill when: repeated patterns need guidance, project conventions differ, complex workflows
- Don't create skill when: documentation exists, trivial, one-off task
- Structure: `skills/{name}/SKILL.md` required, `assets/` and `references/` optional
- SKILL.md must have: name, description with trigger, license, metadata

### issue-creation
- Follow issue-first enforcement: create issue BEFORE any code
- Use issue creation workflow for bug reports and feature requests
- Include: problem description, reproduction steps, expected vs actual behavior

### branch-pr
- Create branch from issue number: `git checkout -b {type}/{issue-number}-{short-description}`
- Use conventional commits: `feat(scope): description`, `fix(scope): description`
- PR template: description, type of change, testing checklist, screenshots if applicable

### judgment-day
- Launch two independent blind judge sub-agents simultaneously
- Synthesize findings, apply fixes, re-judge until both pass
- Escalate after 2 iterations if issues remain

### sdd-init
- Detect stack, conventions, testing capabilities
- Bootstrap active persistence backend
- Initialize .atl/ directory structure

### sdd-spec
- Write requirements and scenarios (delta specs for changes)
- Include: what, why, where, learned sections
- Update existing specs with same topic_key for evolving decisions

### sdd-tasks
- Break down specs into implementation task checklist
- Each task: specific, actionable, verifiable

### sdd-design
- Create technical design document with architecture decisions
- Include: approach, tradeoffs, alternatives considered

### sdd-apply
- Implement tasks following specs and design
- Write actual code, respect project conventions

### sdd-verify
- Validate implementation matches specs, design, and tasks
- Check: requirements met, edge cases handled, no regressions

### sdd-archive
- Sync delta specs to main specs
- Archive completed change artifacts

### sdd-explore
- Investigate codebase, think through features
- Clarify requirements before committing to change

### sdd-propose
- Create change proposal with intent, scope, and approach
- Based on exploration findings

### sdd-onboard
- Guided end-to-end walkthrough of SDD workflow
- Uses real codebase for learning

## Project Conventions

| File | Path | Notes |
|------|------|-------|
| README.es.md | F:\Proyectos\Unity\Unity Projects\Mobalike\README.es.md | Spanish architecture documentation |
| README.md | F:\Proyectos\Unity\Unity Projects\Mobalike\README.md | English documentation |

## Documentation (Assets/_Project/Documentation/)

| File | Notes |
|------|-------|
| memoria.md | Project state, bugs fixed, learnings |
| roadmap.md | Project roadmap (fases 1-7) |
| rules.md | Concisión + archivos prohibidos |
| HOW_TO_CREATE_ITEMS.md | Item creation guide |
| idea_inicial.md | Original project vision | |

## Project Standards (from rules.md)

- **Concision**: Be concise. Don't repeat unchanged code. Show only relevant fragments for small changes.
- **Spanish first**: This project uses Spanish (voseo) for documentation and conventions.
- **Unity MCP**: Use `mcpforunity://` resources before tools; wait for compilation after script edits.
- **Token conservation**: NEVER read .unity, .prefab, .meta, .mat, .asset, .controller, .anim files directly. Use MCP for Unity tools (manage_gameobject, manage_components) to inspect scenes and prefabs instead. See `Documentation/rules.md` for full file ignore list.
