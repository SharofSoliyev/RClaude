# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.1.0] - 2026-02-25

### Added

- Telegram bot wrapping Claude Code CLI with real-time streaming responses
- Session management with SQLite persistence (create, switch, rename, delete)
- Permission system using Claude Code Hooks (Full Access / Ask Permission modes)
- Cross-platform support: macOS, Linux, Windows
- Auto-installer that sets up .NET SDK, Node.js, and Claude Code CLI
- Multi-language support for bot UI (Uzbek, English, Russian)
- Tool visibility — see which tools Claude uses (Read, Write, Bash, etc.)
- Security: command injection protection via ProcessStartInfo.ArgumentList
- Security: model whitelist validation (sonnet, opus, haiku)
- Security: session ID regex validation
- Telegram commands: /newsession, /setdir, /session, /sessions, /renamesession, /deletesession, /getdir, /clear, /model, /help

### Fixed

- Permission button deadlock and in-place message update issues
- Telegram username required during installation for security
- UTF-8 BOM and @ symbol escaping in install.ps1 for PowerShell compatibility
- install.sh syntax error (missing fi)
