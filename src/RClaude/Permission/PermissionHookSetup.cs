using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RClaude.Permission;

/// <summary>
/// Creates the hook script and configures Claude CLI to use it.
/// Platform-aware: bash on Unix, PowerShell on Windows.
/// </summary>
public static class PermissionHookSetup
{
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    private static readonly string HooksDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rclaude", "hooks");

    private static readonly string HookScriptFileName = IsWindows ? "permission-hook.ps1" : "permission-hook.sh";

    private static readonly string HookScriptPath = Path.Combine(HooksDir, HookScriptFileName);

    private static readonly string ClaudeSettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude", "settings.json");

    /// <summary>
    /// Returns the command string for Claude CLI settings.json.
    /// Unix: direct path to .sh file.
    /// Windows: powershell invocation wrapping the .ps1 file.
    /// </summary>
    private static string GetHookCommand()
    {
        if (IsWindows)
            return $"powershell -NoProfile -ExecutionPolicy Bypass -File \"{HookScriptPath}\"";
        return HookScriptPath;
    }

    private const string UnixHookScript = """
        #!/bin/bash
        # RClaude Permission Hook
        # This hook intercepts tool calls and asks for permission via Telegram.
        # When not running via RClaude (no RCLAUDE_PERMISSION_PORT), it passes through.

        if [ -z "$RCLAUDE_PERMISSION_PORT" ]; then
          exit 0
        fi

        # Full access mode — barcha toollar avtomatik ruxsat
        if [ "$RCLAUDE_PERMISSION_MODE" = "full" ]; then
          echo '{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"allow","permissionDecisionReason":"Full access mode"}}'
          exit 0
        fi

        PAYLOAD=$(cat)
        TOOL_NAME=$(echo "$PAYLOAD" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_name',''))" 2>/dev/null)

        # Safe tools — auto-allow
        case "$TOOL_NAME" in
          Read|read_file|Glob|glob|Grep|grep|ListDirectory|list_directory|WebSearch|WebFetch|TodoWrite|Task|AskUserQuestion)
            echo '{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"allow","permissionDecisionReason":"Safe tool"}}'
            exit 0
            ;;
        esac

        # Dangerous tools — ask bot for permission
        REQUEST_ID=$(uuidgen 2>/dev/null || python3 -c "import uuid; print(uuid.uuid4())")

        RESPONSE=$(curl -s --max-time 125 -X POST "http://localhost:$RCLAUDE_PERMISSION_PORT/permission" \
          -H "Content-Type: application/json" \
          -d "{\"userId\": $RCLAUDE_USER_ID, \"chatId\": $RCLAUDE_CHAT_ID, \"toolName\": \"$TOOL_NAME\", \"toolInput\": $PAYLOAD, \"requestId\": \"$REQUEST_ID\"}")

        DECISION=$(echo "$RESPONSE" | python3 -c "import sys,json; print(json.load(sys.stdin).get('decision','deny'))" 2>/dev/null)

        if [ "$DECISION" = "allow" ]; then
          echo '{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"allow"}}'
        else
          echo '{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"deny","permissionDecisionReason":"User denied via Telegram"}}'
        fi
        """;

    private const string WindowsHookScript = """
        # RClaude Permission Hook (Windows)
        # This hook intercepts tool calls and asks for permission via Telegram.
        # When not running via RClaude (no RCLAUDE_PERMISSION_PORT), it passes through.

        if (-not $env:RCLAUDE_PERMISSION_PORT) {
            exit 0
        }

        # Full access mode
        if ($env:RCLAUDE_PERMISSION_MODE -eq "full") {
            Write-Output '{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"allow","permissionDecisionReason":"Full access mode"}}'
            exit 0
        }

        # Read JSON payload from stdin
        $payload = [Console]::In.ReadToEnd()

        # Parse tool_name
        try {
            $json = $payload | ConvertFrom-Json
            $toolName = $json.tool_name
        } catch {
            $toolName = ""
        }

        # Safe tools — auto-allow
        $safeTools = @(
            "Read", "read_file", "Glob", "glob", "Grep", "grep",
            "ListDirectory", "list_directory", "WebSearch", "WebFetch",
            "TodoWrite", "Task", "AskUserQuestion"
        )
        if ($toolName -in $safeTools) {
            Write-Output '{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"allow","permissionDecisionReason":"Safe tool"}}'
            exit 0
        }

        # Generate request ID
        $requestId = [guid]::NewGuid().ToString()

        # Build JSON body
        $body = @{
            userId    = [long]$env:RCLAUDE_USER_ID
            chatId    = [long]$env:RCLAUDE_CHAT_ID
            toolName  = $toolName
            toolInput = ($payload | ConvertFrom-Json)
            requestId = $requestId
        } | ConvertTo-Json -Depth 10 -Compress

        # POST to local permission service
        try {
            $response = Invoke-RestMethod -Uri "http://localhost:$($env:RCLAUDE_PERMISSION_PORT)/permission" `
                -Method Post -ContentType "application/json" -Body $body -TimeoutSec 125
            $decision = $response.decision
        } catch {
            $decision = "deny"
        }

        if ($decision -eq "allow") {
            Write-Output '{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"allow"}}'
        } else {
            Write-Output '{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"deny","permissionDecisionReason":"User denied via Telegram"}}'
        }
        """;

    /// <summary>
    /// Creates the hook script file and adds it to Claude CLI settings.
    /// </summary>
    public static void EnsureHookScript()
    {
        Directory.CreateDirectory(HooksDir);

        var scriptContent = IsWindows ? WindowsHookScript : UnixHookScript;
        File.WriteAllText(HookScriptPath, scriptContent);

        // Make executable on Unix only
        if (!IsWindows)
        {
            try
            {
                System.Diagnostics.Process.Start("chmod", $"+x \"{HookScriptPath}\"")?.WaitForExit(5000);
            }
            catch { }
        }

        ConfigureClaudeSettings();
    }

    private static void ConfigureClaudeSettings()
    {
        var settingsDir = Path.GetDirectoryName(ClaudeSettingsPath)!;
        Directory.CreateDirectory(settingsDir);

        JsonNode? root = null;

        if (File.Exists(ClaudeSettingsPath))
        {
            try
            {
                var existing = File.ReadAllText(ClaudeSettingsPath);
                root = JsonNode.Parse(existing);
            }
            catch { }
        }

        root ??= new JsonObject();

        var hooks = root["hooks"]?.AsObject();
        if (hooks == null)
        {
            hooks = new JsonObject();
            root["hooks"] = hooks;
        }

        var preToolUse = hooks["PreToolUse"]?.AsArray();
        if (preToolUse == null)
        {
            preToolUse = new JsonArray();
            hooks["PreToolUse"] = preToolUse;
        }

        // Check if RClaude hook already exists (either .sh or .ps1)
        var alreadyConfigured = false;
        foreach (var item in preToolUse)
        {
            var hooksList = item?["hooks"]?.AsArray();
            if (hooksList == null) continue;

            foreach (var hook in hooksList)
            {
                var cmd = hook?["command"]?.GetValue<string>();
                if (cmd != null && (cmd.Contains("permission-hook.sh") || cmd.Contains("permission-hook.ps1")))
                {
                    hook!["command"] = GetHookCommand();
                    alreadyConfigured = true;
                }
            }
        }

        if (!alreadyConfigured)
        {
            var hookEntry = new JsonObject
            {
                ["matcher"] = "",
                ["hooks"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["type"] = "command",
                        ["command"] = GetHookCommand()
                    }
                }
            };
            preToolUse.Add(hookEntry);
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(ClaudeSettingsPath, root.ToJsonString(options));
    }
}
