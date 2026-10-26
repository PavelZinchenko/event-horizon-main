# GitHub Pull Request Handling

This folder stores active pull requests from the main repository and from the submodule repositories listed in [Repositories](#repositories).
Active means open and not merged. Include draft pull requests.
Track active pull requests only. When a pull request is merged or closed, delete all of its local files.

## Repositories

| Repository | Submodule path | Local folder |
| --- | --- | --- |
| `PavelZinchenko/event-horizon-main` | none (main repository) | `.pull_requests/` |
| `PavelZinchenko/event-horizon-modules` | `Assets/ModulesShared` | `.pull_requests/event-horizon-modules/` |

Pull request numbers are unique only inside one repository. The main repository and a submodule can both have a #12.
Store each submodule's files in its own folder, named after the GitHub repository. Never put submodule files in the root folder.
Identify a pull request by repository and number, never by number alone.
In local text, write `#616` for the main repository and `event-horizon-modules#12` for a submodule.
To track another submodule, add a row to this table. Synchronization creates the folder.

## Index

[INDEX.md](INDEX.md) lists every active pull request, one row each, with the last synchronization time.
Use one table per repository, under a heading with the repository name. Put the main repository first.
Link submodule files by their folder path, for example `[event-horizon-modules/12.md](event-horizon-modules/12.md)`.
Each row has a status, links to the snapshot and review, the confirmed-issue count, and a short result.
Add a row when synchronization creates a snapshot. Delete the row when the pull request is merged or closed.

Use one of these statuses:

- `Needs review`: `needs_review` is `true` in the snapshot.
- `Changes required`: the latest review found confirmed issues at the current head.
- `Ready to merge`: the latest review passed at the current head; the merge waits for the owner's confirmation.

Update the status after each synchronization, review, and merge.

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

Store one pull request per `<folder>/<number>.md` file. `<folder>` is the repository's local folder from [Repositories](#repositories).
Use the pull request number as the filename, for example `547.md` or `event-horizon-modules/12.md`.
Keep the snapshot, review, and comment draft of one pull request in the same folder.

Each file must contain:

- Repository, GitHub URL, title, state, draft status, and author.
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
repository: "PavelZinchenko/event-horizon-main"
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

For a submodule pull request, use the submodule repository in `repository` and `url`.
Write the heading as `# event-horizon-modules#12: Title`.
List changed paths as GitHub reports them, relative to the submodule repository root, not to the main repository.
Add the `repository` field to an older snapshot that lacks it at its next synchronization.

## Local reviews

In the review step, review each pull request whose snapshot has `needs_review: true`, unless the user names other pull requests.
Store the review in `<number>.review.md` beside the synchronized `<number>.md` snapshot.
For example, pull request 547 uses `547.review.md`, and `event-horizon-modules#12` uses `event-horizon-modules/12.review.md`.
Code review is a separate operation from synchronization.

For a submodule pull request:

- Pin source links to commits in the submodule repository.
- Also trace consumers in the main repository. The main repository uses the submodule at a pinned commit, so read its current `main`.
- Record a main repository consumer that would break as an issue of the submodule pull request.
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
8. Read back the review, set `needs_review: false` in the snapshot, and update its row in [INDEX.md](INDEX.md).

If a patch is omitted, inspect the file at the recorded head and state any remaining comparison limitation.
For large pull requests, record the actual coverage and all retrieval limits.
A complete path inventory does not establish complete code-review coverage.
Label a targeted review explicitly and do not treat it as merge approval.

Reviews remain local unless the user explicitly approves publishing the exact review content to GitHub.
Do not implement fixes, stage files, or create commits as part of a review request.

### Local command fallback

If Codex's command helper fails before launching a process, the available Node runtime can provide local file access and child processes.
Use `node:fs/promises` for files and `node:child_process.execFile` for commands, with explicit argument arrays and the repository working directory.
Keep the same filesystem and network permissions. Continue to use the GitHub integration for remote GitHub operations.

## Merging

Merge only after the review passes and the owner confirms the merge.
Before merging, recheck that the head commit still equals the reviewed head and that the pull request merges cleanly.
Choose the merge method unless the owner specifies one.
After the merge, delete the pull request's local files and its index row.
A merged submodule pull request does not change the main repository. The main repository changes only when its submodule pointer moves.
Report that the pointer still needs an update, but do not move it without the owner's request.

## Comment drafts

Store a draft in `<number>.comment-draft.md`, in the pull request's folder. Write it for the pull request author, not for the owner.
Keep it short. The author must be able to apply the fix without reading the local review.

Each draft must contain:

- An opening of one to three sentences: what blocks the merge, and owner decisions already made.
  Add requested description changes or reverts here, in one line.
- The reviewed head commit.
- One section per issue:
  - A commit-pinned link to the affected lines.
  - The trigger with concrete values, and the impact. Use two or three sentences.
  - The fix, shown only as the change: a short diff or the changed lines, with file and line.
    Do not paste whole methods or classes. When the fix spans many call sites, list them instead of patching each.
  - Optionally, one `Check:` line for a case the fix could easily break.

Leave out findings that cannot trigger, generic test steps, summary sections that repeat the issues,
and internal notes meant only for the owner.
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
2. Enumerate all active pull requests in each repository from [Repositories](#repositories). Follow pagination until each result set is complete.
3. Fetch each active pull request's metadata, original description, and changed-file list.
4. Verify that the file list belongs to the recorded head commit.
5. Check that the list contains unique paths and matches GitHub's changed-file count.
6. Create missing files and refresh GitHub-derived content in existing files.
7. Set `needs_review` in each snapshot: `true` for a new pull request or a moved head commit, otherwise unchanged.
   Add or update each row in [INDEX.md](INDEX.md) and set its status.
8. For each local pull request missing from the active results, fetch its state directly from its own repository. If it is merged or closed, delete its local files and its index row.
9. Preserve all local analysis, owner notes, and response drafts of active pull requests.
10. Read back the written files and verify their descriptions, metadata, and file counts.
11. Report created, updated, unchanged, deleted, and incomplete files per repository, and list the pull requests that need a review.

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
