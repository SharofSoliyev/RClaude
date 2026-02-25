<p align="center">
  <img src="logo.png" alt="RClaude Logo" width="500">
</p>

<h1 align="center">RClaude</h1>

<p align="center"><strong>Claude Code AI Agent via Telegram</strong></p>

<p align="center">
  <a href="#installation">Install</a> &bull;
  <a href="#usage">Usage</a> &bull;
  <a href="#telegram-commands">Commands</a> &bull;
  <a href="#security">Security</a> &bull;
  <a href="#ornatish">O'zbekcha</a>
</p>

<p align="center">
  <a href="https://github.com/SharofSoliyev/RClaude/actions/workflows/build.yml"><img src="https://github.com/SharofSoliyev/RClaude/actions/workflows/build.yml/badge.svg" alt="Build"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License: MIT"></a>
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-8.0-purple.svg" alt=".NET"></a>
  <a href="https://core.telegram.org/bots"><img src="https://img.shields.io/badge/Telegram-Bot-blue.svg?logo=telegram" alt="Telegram Bot"></a>
</p>

Use your Claude Code subscription through Telegram — just like the VS Code extension, but from anywhere.

RClaude runs the Claude Code CLI as a subprocess, streams responses in real-time to your Telegram chat, and manages multiple sessions with persistent storage.

---

## Features

- **Real-time streaming** — responses appear live in Telegram with edit updates
- **Session management** — multiple named sessions, each with its own folder and context
- **Permission system** — dangerous tools (Bash, Write, Edit) require approval via Telegram buttons, just like VS Code
- **Cross-platform** — works on macOS, Linux, and Windows
- **Auto-install** — installer automatically sets up .NET SDK, Node.js, Claude Code CLI, and runs `claude login`
- **Tool visibility** — see which tools Claude uses (Read, Write, Bash, etc.)
- **Security hardened** — no shell injection, model whitelist, session ID validation
- **Database persistence** — sessions and settings survive restarts (SQLite)
- **Multi-language** — installer supports Uzbek, English, Russian

## Architecture

```
┌──────────────┐     ┌──────────────────┐     ┌──────────────────┐
│              │     │                  │     │                  │
│  Telegram    │────▶│  RClaude Bot     │────▶│  Claude Code CLI │
│  User        │     │  (.NET 8 Worker) │     │  (subprocess)    │
│              │◀────│                  │◀────│                  │
│              │     │  • UpdateHandler │     │  • stream-json   │
└──────────────┘     │  • SessionStore  │     │  • file ops      │
   Real-time         │  • PermissionSvc │     │  • shell cmds    │
   streaming         └────────┬─────────┘     └──────────────────┘
                              │
                     ┌────────▼─────────┐
                     │  SQLite Database  │
                     │  (sessions, cfg)  │
                     └──────────────────┘
```

## Requirements

- macOS, Linux, or Windows 10+
- [Claude Code](https://docs.anthropic.com/en/docs/claude-code) subscription (CLI installed automatically if missing)
- Telegram bot token from [@BotFather](https://t.me/BotFather)
- .NET SDK 8+ (installed automatically if not present)

## Installation

### macOS / Linux

```bash
git clone https://github.com/SharofSoliyev/RClaude.git
cd RClaude
chmod +x install.sh
./install.sh
```

### Windows (PowerShell)

```powershell
git clone https://github.com/SharofSoliyev/RClaude.git
cd RClaude
.\install.ps1
```

### What the installer does

1. Checks/installs prerequisites (.NET SDK, Node.js, Claude Code CLI)
2. If Claude Code is not installed — installs via `npm` and runs `claude login`
3. Asks for your Telegram bot token and username
4. Asks for permission mode (Full Access or Ask Permission)
5. Builds and deploys to `~/.rclaude`
6. Adds `rclaude` command to your PATH

## Usage

```bash
rclaude start     # Start in background
rclaude stop      # Stop
rclaude restart   # Restart
rclaude status    # Check status
rclaude logs      # View logs (real-time)
rclaude run       # Start in foreground
rclaude config    # View settings
```

On Windows, the same commands work via `rclaude.cmd`.

## Telegram Commands

| Command | Description |
|---------|-------------|
| `/newsession <name>` | Create a new session |
| `/setdir <path>` | Set working directory |
| `/session` | Switch session (inline keyboard) |
| `/sessions` | List all sessions |
| `/renamesession <name>` | Rename current session |
| `/deletesession` | Delete current session |
| `/getdir` | Show current session info |
| `/clear` | Clear conversation context |
| `/model <name>` | Change model (sonnet/opus/haiku) |
| `/help` | Show all commands |

## Security

### Permission System

RClaude has a built-in permission system using [Claude Code Hooks](https://docs.anthropic.com/en/docs/claude-code/hooks). During installation you choose one of two modes:

| Mode | Behavior |
|------|----------|
| **Full Access** | All tools auto-approved (fastest) |
| **Ask Permission** | Dangerous tools require approval via Telegram buttons |

In “Ask Permission” mode, safe tools (Read, Glob, Grep, WebSearch) are auto-allowed. Dangerous tools (Bash, Write, Edit) show a Telegram message with the command/file and Allow/Deny buttons:

```
💻 Bash ishlatmoqchi:
npm install

[✅ Ruxsat berish] [❌ Rad etish]
```

### Command Injection Protection

- Uses `ProcessStartInfo.ArgumentList` — each argument is passed separately, no shell parsing
- Model whitelist validation — only allowed model names accepted
- Session ID regex validation — only alphanumeric, dash, underscore

For more details, see [SECURITY.md](SECURITY.md).

## Project Structure

```
src/RClaude/
├── Claude/
│   ├── ClaudeCliService.cs       # Runs Claude CLI subprocess, streams output
│   ├── StreamJsonParser.cs       # Parses stream-json events
│   ├── StreamEvent.cs            # Stream event models
│   └── ClaudeResult.cs           # Final result model
├── Telegram/
│   ├── TelegramHostedService.cs  # Background service, bot lifecycle
│   ├── UpdateHandler.cs          # Handles Telegram updates + permission buttons
│   ├── CommandHandler.cs         # Slash commands + callback handling
│   └── MessageFormatter.cs       # Formats messages for Telegram
├── Permission/
│   ├── PermissionService.cs      # HTTP server for hook communication
│   ├── PermissionRequest.cs      # Request/response models
│   └── PermissionHookSetup.cs    # Creates hook scripts (bash/PowerShell)
├── Session/
│   ├── SessionStore.cs           # Session persistence
│   └── UserSession.cs            # Session model
├── Configuration/                # App settings models
├── Data/                         # EF Core database context
└── Program.cs                    # Entry point, DI setup, --init-db mode
```

## Tech Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 8 |
| Bot framework | Telegram.Bot v22 |
| Database | SQLite + EF Core |
| AI backend | Claude Code CLI |
| Permission hooks | Bash (Unix) / PowerShell (Windows) |
| Installer | Bash + PowerShell |

## Update

Run `./install.sh` (or `.\install.ps1` on Windows) again — select **Update** to rebuild without losing settings.

## Uninstall

Run the installer and select **Uninstall**, or manually:

```bash
# macOS/Linux
rm -rf ~/.rclaude

# Windows (PowerShell)
Remove-Item -Recurse -Force "$env:USERPROFILE\.rclaude"
```

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

<h2 id="ornatish">O'zbekcha</h2>

<p align="center">
  <img src="logo.png" alt="RClaude Logo" width="500">
</p>

<h3 align="center">Telegram orqali Claude Code AI Agent</h3>

Claude Code subscriptioningizni Telegram orqali ishlating — xuddi VS Code extensiondek, lekin istalgan joydan.

### Xususiyatlari

- **Real-time streaming** — javoblar Telegramda jonli ko'rinadi
- **Sessiya boshqaruvi** — har biri o'z folder va kontekstiga ega
- **Ruxsat tizimi** — xavfli toollar (Bash, Write, Edit) uchun Telegram button orqali ruxsat so'raydi
- **Kross-platform** — macOS, Linux va Windows da ishlaydi
- **Avtomatik o'rnatish** — .NET, Node.js, Claude Code CLI avtomatik o'rnatiladi va `claude login` so'raladi
- **Xavfsizlik** — command injection himoyasi, model whitelist, session ID validatsiyasi

### O'rnatish

**macOS / Linux:**
```bash
git clone https://github.com/SharofSoliyev/RClaude.git
cd RClaude
chmod +x install.sh
./install.sh
```

**Windows (PowerShell):**
```powershell
git clone https://github.com/SharofSoliyev/RClaude.git
cd RClaude
.\install.ps1
```

Installer quyidagilarni bajaradi:
1. .NET SDK, Node.js, Claude Code CLI ni tekshiradi — yo'q bo'lsa o'rnatadi
2. `claude login` ni ishga tushiradi
3. Bot token va username so'raydi
4. Ruxsat rejimini tanlashni so'raydi (To'liq kirish yoki So'rash)
5. `~/.rclaude` ga build qiladi
6. `rclaude` buyrug'ini PATH ga qo'shadi

### Foydalanish

```bash
rclaude start     # Background da ishga tushirish
rclaude stop      # To'xtatish
rclaude restart   # Qayta ishga tushirish
rclaude status    # Holat tekshirish
rclaude logs      # Loglarni ko'rish
rclaude run       # Foreground da ishga tushirish
rclaude config    # Sozlamalarni ko'rish
```

### Telegram Buyruqlari

| Buyruq | Tavsif |
|--------|--------|
| `/newsession <nom>` | Yangi sessiya yaratish |
| `/setdir <path>` | Ishchi papka belgilash |
| `/session` | Sessiya tanlash |
| `/sessions` | Barcha sessiyalar |
| `/renamesession <nom>` | Sessiyani qayta nomlash |
| `/deletesession` | Sessiyani o'chirish |
| `/getdir` | Sessiya ma'lumotlari |
| `/clear` | Kontekstni tozalash |
| `/model <nom>` | Model o'zgartirish (sonnet/opus/haiku) |
| `/help` | Barcha buyruqlar |

### Ruxsat Tizimi

O'rnatish vaqtida ikki rejimdan birini tanlaysiz:

| Rejim | Harakat |
|-------|---------|
| **To'liq kirish** | Barcha toollar avtomatik ruxsat |
| **So'rash** | Xavfli toollar (Bash, Write, Edit) uchun Telegram da button chiqadi |

"So'rash" rejimida xavfsiz toollar (Read, Glob, Grep) avtomatik ruxsat oladi. Xavfli toollar uchun button chiqadi:

```
💻 Bash ishlatmoqchi:
npm install

[✅ Ruxsat berish] [❌ Rad etish]
```
