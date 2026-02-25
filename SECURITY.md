# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| Latest (main) | Yes |
| Older versions | No |

## Reporting a Vulnerability

We take security seriously. If you discover a vulnerability, please report it responsibly.

**Do NOT open a public issue for security vulnerabilities.**

### How to Report

1. Go to the [Security Advisories](https://github.com/SharofSoliyev/RClaude/security/advisories) page
2. Click "Report a vulnerability"
3. Provide a detailed description of the issue

### What to Expect

- **48 hours** — initial acknowledgment
- **7 days** — assessment and action plan
- **30 days** — fix released (for confirmed vulnerabilities)

## Security Architecture

RClaude implements several layers of protection:

### Command Injection Prevention
Claude Code CLI is invoked using `ProcessStartInfo.ArgumentList`, which passes each argument separately without shell parsing. This prevents shell injection attacks.

### Model Whitelist
Only approved model names (sonnet, opus, haiku) are accepted. All model inputs are validated against a strict whitelist.

### Session ID Validation
Session identifiers are validated with a regex pattern allowing only alphanumeric characters, dashes, and underscores.

### Access Control
- Telegram username-based authentication restricts bot access to authorized users only
- Permission system using Claude Code Hooks requires explicit approval for dangerous tools (Bash, Write, Edit)
