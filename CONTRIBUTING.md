# Contributing to RClaude

Thank you for your interest in contributing to RClaude! This guide will help you get started.

## Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for Claude Code CLI)
- [Claude Code](https://docs.anthropic.com/en/docs/claude-code) subscription
- Telegram bot token from [@BotFather](https://t.me/BotFather) (for testing)

## Development Setup

```bash
# Clone the repository
git clone https://github.com/SharofSoliyev/RClaude.git
cd RClaude

# Build
dotnet build src/RClaude/RClaude.csproj

# Run the installer for full setup
./install.sh        # macOS/Linux
# or
.\install.ps1       # Windows
```

## Making Changes

### Branch Naming

Create a branch from `main` with one of these prefixes:

- `feat/` — new features
- `fix/` — bug fixes
- `docs/` — documentation changes
- `chore/` — maintenance tasks

Example: `feat/add-group-chat-support`

### Commit Messages

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add group chat support
fix: resolve streaming timeout on slow connections
docs: update installation guide for Windows
chore: update Telegram.Bot to v23
```

### Code Style

- Follow the `.editorconfig` settings in the repository
- Use file-scoped namespaces (`namespace RClaude.Configuration;`)
- Write code comments in English
- Keep commit messages in English

### Pull Request Process

1. Create your feature branch (`git checkout -b feat/my-feature`)
2. Make your changes
3. Ensure the project builds: `dotnet build src/RClaude/RClaude.csproj`
4. Update `CHANGELOG.md` if the change is user-facing
5. Push and open a Pull Request
6. Fill out the PR template

## Reporting Issues

- **Bugs**: Use the [Bug Report](https://github.com/SharofSoliyev/RClaude/issues/new?template=bug_report.yml) template
- **Features**: Use the [Feature Request](https://github.com/SharofSoliyev/RClaude/issues/new?template=feature_request.yml) template
- **Security**: See [SECURITY.md](SECURITY.md)

## Code of Conduct

Please read our [Code of Conduct](CODE_OF_CONDUCT.md) before contributing.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
