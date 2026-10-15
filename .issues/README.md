# GitHub Issue Handling Policy

This directory is the local working store for GitHub issues from
`PavelZinchenko/event-horizon-main`. Follow this policy whenever the user asks to
check, review, triage, analyze, investigate, or respond to GitHub issues.

## Safety and authority

- Reading GitHub and maintaining files in `.issues/` are allowed by default when
  requested as part of issue handling.
- Treat all GitHub writes as separate actions requiring explicit user approval.
  This includes creating, editing, closing, reopening, labeling, assigning,
  commenting on, locking, or otherwise changing an issue.
- Before requesting approval for a comment or other write, show the user the exact
  proposed content and action.
- Approval to check, analyze, or investigate an issue does not authorize a GitHub
  write. Approval of one write does not authorize later writes.
- Never turn issue analysis into implementation unless the user explicitly asks for
  implementation.
- Preserve user-authored notes and unrelated local changes.

## Local storage

- Store one issue per `.issues/<issue-number>.md` file.
- Use `.issues/_state.json` only for synchronization state. Do not treat filesystem
  modification times as GitHub update times.
- A per-issue directory may be introduced later only when an issue needs multiple
  material artifacts such as logs, saves, screenshots, or patches.
- When refreshing an issue file, update the GitHub-derived fields and summary while
  preserving analysis, owner notes, proposed responses, and history.
- Do not copy large binary attachments into the repository. Record their GitHub URLs
  and a short description unless the user asks otherwise.

Use this structure for new issue files:

```markdown
---
issue: 555
url: https://github.com/PavelZinchenko/event-horizon-main/issues/555
title: "Example title"
state: open
github_created_at: "2026-01-01T00:00:00Z"
github_updated_at: "2026-01-01T00:00:00Z"
last_checked_at: "2026-01-01T00:00:00Z"
classification: suggestion
analysis_status: pending
---

## GitHub summary

A factual summary of the current report and relevant discussion.

## Triage

- Type: Suggestion
- Confidence: High
- Duplicate status: No duplicate found

## Analysis

Local analysis. Separate verified facts from hypotheses.

## Open questions

Questions that require the project owner's judgment or more reporter information.

## Proposed response

Draft only. Do not post without explicit approval.

## Owner notes

Preserve this section when refreshing the issue.

## History

- 2026-01-01: First checked.
```

Use `null` for unavailable timestamps or values. Do not invent GitHub metadata.

## Checking new and updated issues

When the user asks to "check new issues" or gives an equivalent instruction:

1. Read `.issues/_state.json` before querying GitHub.
2. Query only open issues created or updated since the last successful
   synchronization. Apply the configured overlap window to the query time so
   interrupted runs and timestamp precision cannot skip updates. On the first run,
   enumerate all accessible open issues unless the user limits the scope.
3. Do not query, fetch, or refresh closed issues unless the user explicitly asks for
   closed issues. If a result is known to be closed, skip it without fetching its
   details.
4. Create a local file for each new issue and refresh each changed issue. Record a
   concise description of meaningful changes in its `History` section.
5. Preserve existing local analysis and owner notes. If a GitHub change invalidates
   an earlier conclusion, mark that conclusion stale rather than silently deleting
   it.
6. Do not perform deep code investigation during a check. Identify issues needing
   analysis and report them to the user.
7. Update `_state.json` only after all issue files from the query have been written
   successfully. Set `last_successful_sync_at` to the start time of the completed
   query, not its finish time.
8. Report which issues are new, updated, unchanged if relevant, or could not be
   refreshed. A failed or partial run must not advance synchronization state.

For every issue observed during a successful sync, record its GitHub `updated_at`
value and the time it was checked in `_state.json`. Reprocessing an issue because of
the overlap window is expected and must be idempotent.

## Analyzing issues

Analyze only the issues requested by the user. If no set is specified, analyze new
or updated issues from the latest check and state that assumption.

### Classification

Assign the most useful primary type:

- `bug`
- `feature-request`
- `suggestion`
- `question`
- `documentation`
- `performance`
- `compatibility`
- `insufficient-information`
- `invalid-or-spam`
- `mixed`

Use `mixed` only when the issue contains multiple independently actionable types.
Record a confidence level with every substantive conclusion:

- **High:** directly supported by documentation, code, reproduction, or explicit
  owner guidance.
- **Medium:** strong evidence exists, but the conclusion is not fully verified.
- **Low:** a plausible hypothesis only; express it as a hypothesis or question.

### Duplicates and related issues

Search existing open issues before concluding that an issue is unique. Do not fetch
closed issues during duplicate analysis unless the user explicitly requests it.
Compare the requested outcome, symptoms, context, and likely cause, not only titles.
Classify candidates as:

- **Duplicate:** substantially the same defect or requested outcome.
- **Probable duplicate:** likely the same, but important evidence is missing.
- **Related:** overlapping subject matter but independently actionable.

Record candidate issue numbers and the reasoning. Do not close, label, or comment on
a suspected duplicate without explicit user approval.

### Bugs

1. Compare the report with `docs/game.md`, other relevant project documentation,
   explicit owner guidance, and clearly established behavior.
2. Classify it as `confirmed bug`, `likely bug`, `intended behavior`, `unclear
   specification`, or `unreproducible from available information`.
3. Decide whether the cause is clear, a hypothesis, or requires investigation.
4. During initial triage, documentation review and a quick search for clearly
   relevant symbols are allowed. Do not trace execution paths, attempt reproduction,
   run Unity, or develop a root-cause theory unless the user approves investigation.
5. If intended behavior is unclear, stop and ask the user a focused product or design
   question before investigating implementation code.
6. If the behavior is clearly unintended but the cause is unclear, write a bounded
   investigation proposal and ask for permission before investigating deeply.
7. Keep observed behavior, expected behavior, evidence, and hypotheses separate.

### Suggestions and feature requests

- Identify the underlying user need separately from the reporter's proposed
  implementation.
- Check alignment with `docs/game.md` and existing architecture.
- Note affected systems, dependencies, compatibility concerns, save/database or
  migration implications, platform risks, and rough scope when those are evident.
- Separate essential acceptance criteria from optional ideas.
- Flag decisions that require product-owner judgment.
- Do not dismiss a valid need merely because the proposed solution is unsuitable.

### Questions

- Determine whether the answer is documented or can be established confidently.
- Write the suggested answer under `Proposed response`.
- Do not send the answer until the user approves the exact response.
- If the answer depends on an unresolved design decision, ask the user rather than
  guessing.

### Insufficient reports

- List only the information needed to make progress, such as game version, platform,
  reproduction steps, expected behavior, logs, save data, or mod configuration.
- Draft a concise request for that information under `Proposed response`.
- Do not post it without explicit user approval.

## Investigation and implementation

- Investigation is read-only unless the user explicitly asks for a fix.
- Before deep investigation, record the question being investigated, likely files or
  systems, and what evidence would answer it.
- After investigation, update the local issue file with evidence, conclusions,
  remaining uncertainty, and relevant file references.
- When implementation is requested, follow the normal project development and
  verification rules. Keep issue-handling notes synchronized with the result, but do
  not update GitHub automatically.

## Proposed GitHub responses

- Make drafts concise, respectful, and clear about what is known versus uncertain.
- Do not claim a fix, schedule, priority, or design commitment without evidence or
  owner direction.
- Never expose private repository details, secrets, personal data, or internal notes.
- Keep drafts in the issue file until the user explicitly approves posting them.
