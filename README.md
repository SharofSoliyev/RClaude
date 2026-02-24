<p align="center">
  <img src="logo.png" alt="RClaude Logo" width="500">
</p>

<h1 align="center">RClaude</h1>

<p align="center"><strong>Claude Code AI Agent via Telegram</strong></p>

Use your Claude Code subscription through Telegram — just like the VS Code extension, but from anywhere.

RClaude runs the Claude Code CLI as a subprocess, streams responses in real-time to your Telegram chat, and manages multiple sessions with persistent storage.

---

## Features

- **Real-time streaming** — responses appear live in Telegram with edit updates
- **Session management** — multiple named sessions, each with its own folder and context
- **Tool visibility** — see which tools Claude uses (Read, Write, Bash, etc.)
- **Database persistence** — sessions and settings survive restarts (SQLite)
- **Easy installer** — one command setup with auto .NET installation
- **Multi-language** — installer supports Uzbek, English, Russian

## Requirements

- macOS or Linux
- [Claude Code CLI](https://docs.anthropic.com/en/docs/claude-code) (with active subscription)
- Telegram bot token from [@BotFather](https://t.me/BotFather)
- .NET SDK 8+ (installed automatically if not present)

## Installation

```bash
git clone https://github.com/SharofSoliyev/rclaude.git
cd rclaude
chmod +x install.sh
./install.sh
```

The installer will:
1. Check/install prerequisites (.NET SDK, Claude CLI)
2. Ask for your Telegram bot token
3. Build and deploy to `~/.rclaude`
4. Add `rclaude` command to your PATH

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

## How It Works

1. You send a message in Telegram
2. RClaude passes it to the Claude Code CLI (`claude -p "message" --output-format stream-json`)
3. Claude reads/writes files, runs commands in your project folder
4. Responses stream back to Telegram in real-time

## Project Structure

```
src/RClaude/
├── Claude/
│   ├── ClaudeCliService.cs     # Runs Claude CLI subprocess, streams output
│   ├── StreamJsonParser.cs     # Parses stream-json events from Claude
│   ├── StreamEvent.cs          # Stream event models
│   └── ClaudeResult.cs         # Final result model
├── Telegram/
│   ├── TelegramHostedService.cs  # Background service, bot lifecycle
│   ├── UpdateHandler.cs          # Handles incoming Telegram updates
│   ├── CommandHandler.cs         # Handles slash commands
│   └── MessageFormatter.cs       # Formats messages for Telegram
├── Session/
│   ├── SessionStore.cs           # SQLite-based session persistence
│   └── UserSession.cs            # Session model
├── Configuration/                # App settings and config models
├── Data/                         # Database context
├── Program.cs                    # Entry point, DI setup
└── appsettings.json
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 8 |
| Bot framework | Telegram.Bot |
| Database | SQLite + EF Core |
| AI backend | Claude Code CLI |
| Installer | Bash |

## Update

Run `./install.sh` again — select "Update" to rebuild without losing settings.

## Uninstall

Run `./install.sh` — select "Uninstall", or manually:

```bash
rm -rf ~/.rclaude
# Remove PATH line from ~/.zshrc or ~/.bashrc
```

---

<p align="center">
  <img src="logo.png" alt="RClaude Logo" width="500">
</p>

<h1 align="center">RClaude</h1>

<p align="center"><strong>Telegram orqali Claude Code AI Agent</strong></p>

Claude Code subscriptioningizni Telegram orqali ishlating — xuddi VS Code extensiondek, lekin istalgan joydan.

RClaude Claude Code CLI ni subprocess sifatida ishga tushiradi, javoblarni real-time Telegram chatga uzatadi, va bir nechta sessiyalarni doimiy saqlash bilan boshqaradi.

---

## Xususiyatlari

- **Real-time streaming** — javoblar Telegramda jonli ko'rinadi
- **Sessiya boshqaruvi** — har biri o'z folder va kontekstiga ega nomli sessiyalar
- **Tool ko'rinishi** — Claude qaysi asboblarni ishlatayotganini ko'ring (Read, Write, Bash, va h.k.)
- **Database saqlash** — sessiyalar va sozlamalar restart dan keyin saqlanadi (SQLite)
- **Oson installer** — bitta buyruq bilan o'rnatish, .NET avtomatik o'rnatiladi
- **Ko'p tilli** — installer O'zbek, Ingliz, Rus tillarini qo'llab-quvvatlaydi

## Talablar

- macOS yoki Linux
- [Claude Code CLI](https://docs.anthropic.com/en/docs/claude-code) (faol subscription bilan)
- [@BotFather](https://t.me/BotFather) dan Telegram bot token
- .NET SDK 8+ (yo'q bo'lsa avtomatik o'rnatiladi)

## O'rnatish

```bash
git clone https://github.com/SharofSoliyev/rclaude.git
cd rclaude
chmod +x install.sh
./install.sh
```

## Foydalanish

```bash
rclaude start     # Background da ishga tushirish
rclaude stop      # To'xtatish
rclaude restart   # Qayta ishga tushirish
rclaude status    # Holat tekshirish
rclaude logs      # Loglarni ko'rish (real-time)
rclaude run       # Foreground da ishga tushirish
rclaude config    # Sozlamalarni ko'rish
```

## Telegram Buyruqlari

| Buyruq | Tavsif |
|--------|--------|
| `/newsession <nom>` | Yangi sessiya yaratish |
| `/setdir <path>` | Ishchi papka belgilash |
| `/session` | Sessiya tanlash (inline keyboard) |
| `/sessions` | Barcha sessiyalarni ko'rish |
| `/renamesession <nom>` | Joriy sessiyani qayta nomlash |
| `/deletesession` | Sessiyani o'chirish |
| `/getdir` | Sessiya ma'lumotlarini ko'rish |
| `/clear` | Kontekstni tozalash |
| `/model <nom>` | Model o'zgartirish (sonnet/opus/haiku) |
| `/help` | Barcha buyruqlar |

## Loyiha Tuzilmasi

```
src/RClaude/
├── Claude/
│   ├── ClaudeCliService.cs     # Claude CLI ni subprocess sifatida ishlatadi
│   ├── StreamJsonParser.cs     # stream-json eventlarini parse qiladi
│   ├── StreamEvent.cs          # Stream event modellari
│   └── ClaudeResult.cs         # Natija modeli
├── Telegram/
│   ├── TelegramHostedService.cs  # Bot lifecycle boshqaruvi
│   ├── UpdateHandler.cs          # Telegram updatelarini qayta ishlash
│   ├── CommandHandler.cs         # Slash commandlarni qayta ishlash
│   └── MessageFormatter.cs       # Xabarlarni formatlash
├── Session/
│   ├── SessionStore.cs           # SQLite asosida sessiya saqlash
│   └── UserSession.cs            # Sessiya modeli
├── Configuration/                # Sozlamalar
├── Data/                         # Database context
├── Program.cs                    # Entry point, DI setup
└── appsettings.json
```

## Texnologiyalar

| Qatlam | Texnologiya |
|--------|-------------|
| Runtime | .NET 8 |
| Bot framework | Telegram.Bot |
| Database | SQLite + EF Core |
| AI backend | Claude Code CLI |
| Installer | Bash |
