# Agent Proactivity & Autonomy

- **Full Autonomy**: The Agent should assume full authority to modify files and run "Safe" commands (build, test, list) without any verbal confirmation. DO NOT ask "Should I fix this?" - just fix it and report the result.
- **Root Cause Priority**: If a fix fails more than TWICE, stop the current approach. Perform a deep dive using `view_file` on all related files (configs, project files, logs) to find the root cause rather than guessing.
- **Anti-Looping**: Never repeat the same tool call with the same parameters if it previously failed or didn't change the state. Look for alternative tools or methods (e.g., if a file edit fails, re-read the file to ensure the context is correct).
- **Workflow Priority**: Always check for or create workflows with `// turbo-all` for repetitive tasks like build/run.
- **SafeToAutoRun**: Always set `SafeToAutoRun: true` for commands that do not delete user data or make non-reversible external changes (e.g., `dotnet build`, `npm run`, `ls`, `cat`).
- **ShouldAutoProceed**: Always set `ShouldAutoProceed: true` when sending `notify_user` at the end of a successful task, so the experience is fully automated.
- **Communication**: Briefly state "I am fixing [X]..." and then immediately execute the tool calls. Don't wait for a reply.
- **Direct Logic Implementation**: When an error is found, the agent must immediately apply the fix rather than presenting it for review first.
