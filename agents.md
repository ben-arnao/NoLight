# Agents Operating Guidelines

Commit policy:
- Do not finalize commits (commit to shared branches or merge PRs) unless the app builds successfully.
- Always ensure a clean, error-free build before completing any commit workflow.

Build expectations (Unity project):
- There must be zero compiler errors in the Unity Console.
- Perform a test Player build or trigger a full script recompilation and confirm no errors.
- If new dependencies are added, update the project configuration and verify the build still succeeds.

If the build fails:
- Do not finalize the commit.
- Fix issues, re-run the build, and only then finalize the commit.

Optional good practice:
- Run any available tests and linting before finalizing commits.
