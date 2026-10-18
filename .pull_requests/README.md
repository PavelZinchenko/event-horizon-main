# GitHub Pull Request Handling

This folder stores active pull requests from `PavelZinchenko/event-horizon-main`.
Active means open and not merged. Include draft pull requests.
Track active pull requests only. When a pull request is merged or closed, delete all of its local files.

## Workflow

1. **Update.** Synchronize with GitHub (see [Synchronization](#synchronization)). Create files for new pull requests. Mark each pull request that changed and needs a review.
2. **Review.** Write or refresh a review file for each pull request marked for review (see [Local reviews](#local-reviews)).
3. **Merge.** When a review passes, merge the pull request and delete its local files. Do this only with the owner's confirmation.
4. **Draft comments.** Write a comment draft for each reviewed pull request that needs one (see [Comment drafts](#comment-drafts)).
5. **Post comments.** Post the drafts to GitHub. Do this only with the owner's confirmation.

Stop after each step and report. Run several steps in one go only when the owner asks for them together, for example "update and review".

## Scope and authority

- Use the GitHub integration plugin for remote GitHub reads.
- Query, fetch, and refresh active pull requests only, unless the user requests another scope.
- Skip results already known to be closed or merged.
- Synchronization updates local Markdown files only.
- Require explicit user approval for GitHub writes, including comments, reviews, labels, closing, and merging.
- Show the exact proposed content and action before requesting approval for a GitHub write.
- Do not implement changes or perform a code review during synchronization.
- Preserve local analysis, owner notes, response drafts, and unrelated changes while a pull request is active.
- Delete a pull request's local files (snapshot, review, comment drafts) and its index row once it is merged or closed.

## Local files

Store one pull request per `.pull_requests/<number>.md` file.
Use the pull request number as the filename, for example `547.md`.

Each file must contain:

- GitHub URL, title, state, draft status, and author.
- GitHub creation and update timestamps, plus the time of the local check.
- Base and head branches and commit hashes.
- GitHub's changed-file count.
- The original description under `## Description`.
- The complete list of changed repository paths under `## Changed files`.
- `needs_review`: `true` when no review exists or the head commit differs from the reviewed head; otherwise `false`.

Preserve the original description, including Markdown and attachment URLs.
Use `No description provided.` when the description is empty.
Use `null` for unavailable metadata. Do not invent values.
Keep binary attachments on GitHub unless the user requests a download.

Use this structure:

```markdown
---
pull_request: 547
url: "https://github.com/PavelZinchenko/event-horizon-main/pull/547"
title: "Example pull request"
state: "open"
draft: false
author: "example-author"
github_created_at: "2026-01-01T00:00:00Z"
github_updated_at: "2026-01-02T00:00:00Z"
last_checked_at: "2026-01-03T00:00:00Z"
base_branch: "main"
base_sha: "<base commit hash>"
head_branch: "example-change"
head_sha: "<head commit hash>"
changed_files: 1
needs_review: true
---

# #547: Example pull request

## Description

Original GitHub description.

## Changed files

Total: 1 file.

- `Assets/Scripts/Example.cs`
```

## Local reviews

In the review step, review each pull request whose snapshot has `needs_review: true`, unless the user names other pull requests.
Store the review in `<number>.review.md` beside the synchronized `<number>.md` snapshot.
For example, pull request 547 uses `547.review.md`.
Code review is a separate operation from synchronization.
Keep GitHub-derived metadata in the snapshot and local findings in the review.

Each review must contain:

- The pull request URL, title, review time, base commit, merge base, and reviewed head commit.
- A result that states whether changes are required or only potential risks remain.
- A numbered issues list, ordered by severity.
- For each issue: priority, a commit-pinned source link, triggering conditions, impact, and a concrete correction.
- A separate potential risks list. Distinguish verified defects from assumptions, intended behavior, and existing problems.
- Coverage and verification, including inspected areas, checks performed, and checks not performed.

Use P1 for high-impact defects, P2 for normal-impact defects, and P3 for low-impact defects.
Do not invent an issue to populate a review.
State explicitly when no confirmed issue was found.
Record an existing defect as a remaining gap, not as a new regression.

Before writing a review:

1. Confirm that the pull request is active and read its local snapshot.
2. Fetch the current metadata, changed paths, patches, and necessary source files through the GitHub integration.
3. Read relevant project instructions and trace affected callers and dependencies.
4. Review the recorded head commit. Do not substitute the working tree for the pull request source.
5. Verify the issue against surrounding code and identify its trigger and impact.
6. Recheck the head commit after fetching changes. Refresh the review if it moved.
7. Write or update the review and preserve unrelated local notes.
8. Read back the review, set `needs_review: false` in the snapshot, and update the index below.

If a patch is omitted, inspect the file at the recorded head and state any remaining comparison limitation.
For large pull requests, record the actual coverage and all retrieval limits.
A complete path inventory does not establish complete code-review coverage.
Label a targeted review explicitly and do not treat it as merge approval.

Reviews remain local unless the user explicitly approves publishing the exact review content to GitHub.
Do not implement fixes, stage files, or create commits as part of a review request.

### Review index

Latest reviews: #594 and #586 on 2026-10-15; #547 on 2026-10-16; #572 rechecked on 2026-10-18 at 06:57 UTC; #571 on 2026-10-18 at 07:45 UTC. Counts include confirmed issues only; each file also lists potential risks.

| Pull request | Snapshot | Review | Confirmed issues | Result and coverage |
| --- | --- | --- | ---: | --- |
| #547 — LootItem requirements | [547.md](547.md) | [547.review.md](547.review.md) | 2 | Changes required; re-reviewed at 83201454: two earlier issues fixed, requirement-budget fix incomplete; seeded loot change needs owner approval. |
| #571 — Negative fire rate | [571.md](571.md) | [571.review.md](571.review.md) | 1 | Changes required; re-reviewed at 30724329: ChargeableCannon fixed; P1 regression — fire rate 0 now blocks all 17 built-in beam weapons (missed by earlier reviews). |
| #572 — Beam damage | [572.md](572.md) | [572.review.md](572.review.md) | 1 | P3 cleanup only; re-reviewed at 1dffc706, rechecked against main a177bdd5: runtime and calculator agree; beam damage doubling approved by owner 2026-10-18. |
| #586 — Mixed slot types | [586.md](586.md) | [586.review.md](586.review.md) | 4 | Changes required; seven paths, schema, prefab bindings, and dependency checked. |
| #594 — Beta5.1 | [594.md](594.md) | [594.review.md](594.review.md) | 5 | Changes required; targeted review of 37 head files within 7,180 changed paths. |

### Local command fallback

If Codex's command helper fails before launching a process, the available Node runtime can provide local file access and child processes.
Use `node:fs/promises` for files and `node:child_process.execFile` for commands, with explicit argument arrays and the repository working directory.
Keep the same filesystem and network permissions. Continue to use the GitHub integration for remote GitHub operations.

## Merging

Merge only after the review passes and the owner confirms the merge.
Before merging, recheck that the head commit still equals the reviewed head and that the pull request merges cleanly.
Ask the owner for the merge method (merge commit or squash) unless they named one.
After the merge, delete the pull request's local files and its index row.

## Comment drafts

Store a draft in `<number>.comment-draft.md`. Write it for the pull request author, not for the owner.
Make each draft detailed. Write it so the author can apply the fix without reading the local review.

Each draft must contain:

- A short opening: what works, what blocks the merge, and owner decisions already made.
- The reviewed head commit.
- One section per issue, with commit-pinned links to the affected lines.
- For each issue: the code at fault, the trigger, the step-by-step failure path, and the impact.
- A suggested fix as complete code blocks that the author can paste, with any edge cases it covers.
- A way to reproduce or test the fix, before and after.
- A list of requested reverts or description changes, when there are any.

Leave out findings that cannot trigger, and internal notes meant only for the owner.
Show the draft to the owner before posting. Post it only after the owner confirms.
End every draft and every posted comment with an AI attribution line, after a `---` rule. Use the name of the assistant that wrote the comment, not the name from earlier comments:

```markdown
---
_This comment was generated by AI assistant <assistant name> and posted by the maintainer._
```

For example: `Claude (Claude Code)` or `Codex`.
After posting, record the comment URL in the review and delete the draft file.

## Synchronization

1. Read this README and existing local pull request files.
2. Enumerate all active pull requests and follow pagination until the result set is complete.
3. Fetch each active pull request's metadata, original description, and changed-file list.
4. Verify that the file list belongs to the recorded head commit.
5. Check that the list contains unique paths and matches GitHub's changed-file count.
6. Create missing files and refresh GitHub-derived content in existing files.
7. Set `needs_review` in each snapshot: `true` for a new pull request or a moved head commit, otherwise unchanged.
8. For each local pull request missing from the active results, fetch its state directly. If it is merged or closed, delete its local files and its index row.
9. Preserve all local analysis, owner notes, and response drafts of active pull requests.
10. Read back the written files and verify their descriptions, metadata, and file counts.
11. Report created, updated, unchanged, deleted, and incomplete files, and list the pull requests that need a review.

Do not infer closure from a pull request's absence in active search results. Confirm the state directly before deleting.
Do not use filesystem modification times as GitHub update times.
No separate synchronization state file is currently required.

## Large pull requests

The GitHub pull request file endpoint can return at most 3,000 files.
Pagination alone cannot produce a complete list beyond that limit.

Compare complete Git trees at the merge base and head commits when necessary.
A Git tree records repository paths, file content hashes, and file modes at a commit.
Include added, modified, deleted, and renamed paths, and account for file mode changes and submodules.
Represent renames by their destination paths when matching GitHub's changed-file count.
Check that recursive tree responses are complete; retrieve missing subtrees if GitHub truncates them.
Record the file-list source, compared commits, and any inferred rename mappings in the pull request file.
Do not claim completeness from a matching count alone.
Verify all available endpoint paths against the reconstructed list.
If a complete list cannot be verified, mark it incomplete and report the limitation.
