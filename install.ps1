# ═══════════════════════════════════════════════════
#  RClaude Installer — Windows (PowerShell)
#  Claude Code AI Agent via Telegram
# ═══════════════════════════════════════════════════

$ErrorActionPreference = "Stop"

$INSTALL_DIR = "$env:USERPROFILE\.rclaude"
$BINARY_NAME = "rclaude"
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$SRC_DIR = Join-Path $SCRIPT_DIR "src\RClaude"

# ─── Language Selection ──────────────────────────

Write-Host ""
Write-Host "  ===================================================" -ForegroundColor Cyan
Write-Host "    RClaude — Claude Code Agent via Telegram" -ForegroundColor Cyan
Write-Host "  ===================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1) O'zbek tili" -ForegroundColor Green
Write-Host "  2) English" -ForegroundColor Green
Write-Host "  3) Русский" -ForegroundColor Green
Write-Host ""
$LANG_CHOICE = Read-Host "  Til / Language / Язык [1]"
if (-not $LANG_CHOICE) { $LANG_CHOICE = "1" }

# ─── i18n Messages ───────────────────────────────

$msg = @{}

switch ($LANG_CHOICE) {
    "2" {
        $msg.CHECKING = "Checking prerequisites..."
        $msg.DOTNET_NOT_FOUND = ".NET SDK not found. Installing automatically..."
        $msg.DOTNET_INSTALLING = "Installing .NET SDK..."
        $msg.DOTNET_INSTALL_FAILED = "ERROR: .NET SDK installation failed!"
        $msg.DOTNET_INSTALL_MANUAL = "Install manually: https://dotnet.microsoft.com/download"
        $msg.DOTNET_OLD = "ERROR: .NET 8+ required (current:"
        $msg.CLAUDE_NOT_FOUND = "ERROR: Claude Code CLI not found!"
        $msg.CLAUDE_INSTALLING = "Claude Code CLI not found. Installing via npm..."
        $msg.NPM_NOT_FOUND = "npm not found! Install Node.js: https://nodejs.org"
        $msg.NODEJS_INSTALLING = "Installing Node.js..."
        $msg.CLAUDE_INSTALLED = "Claude Code installed"
        $msg.CLAUDE_LOGIN = "Log in to your Claude account:"
        $msg.CLAUDE_INSTALL_FAILED = "Claude Code installation failed!"
        $msg.CLAUDE_INSTALL_MANUAL = "Run manually: npm install -g @anthropic-ai/claude-code"
        $msg.ALREADY_INSTALLED = "RClaude is already installed: $INSTALL_DIR"
        $msg.OPT_UPDATE = "Update (settings preserved)"
        $msg.OPT_REINSTALL = "Reinstall (settings cleared)"
        $msg.OPT_UNINSTALL = "Uninstall (everything removed)"
        $msg.OPT_EXIT = "Exit"
        $msg.CHOICE = "Choice"
        $msg.WRONG_CHOICE = "Invalid choice"
        $msg.UNINSTALLING = "Uninstalling..."
        $msg.BOT_STOPPED = "Bot stopped"
        $msg.DIR_REMOVED = "$INSTALL_DIR removed"
        $msg.PATH_REMOVED = "Removed from PATH"
        $msg.UNINSTALL_DONE = "RClaude successfully uninstalled!"
        $msg.TG_SETTINGS = "Telegram Bot settings"
        $msg.TG_TOKEN_HINT = "Enter bot token from @BotFather."
        $msg.TG_TOKEN_EXAMPLE = "(Example: 123456789:AABBccddEEff...)"
        $msg.TG_TOKEN_PROMPT = "Bot Token"
        $msg.TG_TOKEN_EMPTY = "ERROR: Token not provided"
        $msg.TG_USER_HINT = "Enter your Telegram username (required for security)."
        $msg.TG_USER_EMPTY = "ERROR: Username is required"
        $msg.TG_USER_PROMPT = "Telegram username (without @)"
        $msg.SETTINGS_KEPT = "Existing settings preserved"
        $msg.BUILDING = "Building..."
        $msg.CSPROJ_NOT_FOUND = "ERROR: RClaude.csproj not found!"
        $msg.RUN_FROM_ROOT = "Run install.ps1 from the project root folder."
        $msg.RUNNING_STOPPED = "Running instance stopped"
        $msg.BUILD_FAILED = "ERROR: Build failed!"
        $msg.BUILD_OK = "Build successful"
        $msg.CONFIGURING = "Configuring..."
        $msg.CONFIG_CREATED = "Config created"
        $msg.DB_CREATING = "Creating database..."
        $msg.SETTINGS_SAVED = "Settings saved to DB"
        $msg.CLAUDE_PATH_UPDATED = "Claude CLI path updated"
        $msg.LAUNCHER_CREATING = "Creating launcher..."
        $msg.PATH_ADDED = "Added to PATH"
        $msg.LAUNCHER_CREATED = "Launcher created"
        $msg.INSTALL_DONE = "RClaude successfully installed!"
        $msg.UPDATE_DONE = "RClaude successfully updated!"
        $msg.SETTINGS_PRESERVED = "Settings and sessions preserved."
        $msg.INSTALL_LOC = "Install location"
        $msg.DATABASE = "Database"
        $msg.START = "Start"
        $msg.START_BG = "Background"
        $msg.START_FG = "Foreground"
        $msg.MANAGE = "Manage"
        $msg.STOP = "Stop"
        $msg.RESTART = "Restart"
        $msg.STATUS = "Status"
        $msg.LOGS = "Logs"
        $msg.CONFIG = "Settings"
        $msg.TG_COMMANDS = "Telegram commands"
        $msg.PERM_SELECT = "Permission mode"
        $msg.PERM_FULL = "Full Access (all tools auto-approved)"
        $msg.PERM_ASK = "Ask Permission (Bash/Write/Edit require approval via Telegram)"
    }
    "3" {
        $msg.CHECKING = "Проверка..."
        $msg.DOTNET_NOT_FOUND = ".NET SDK не найден. Устанавливаем автоматически..."
        $msg.DOTNET_INSTALLING = "Установка .NET SDK..."
        $msg.DOTNET_INSTALL_FAILED = "ОШИБКА: Установка .NET SDK не удалась!"
        $msg.DOTNET_INSTALL_MANUAL = "Установите вручную: https://dotnet.microsoft.com/download"
        $msg.DOTNET_OLD = "ОШИБКА: Нужен .NET 8+ (текущий:"
        $msg.CLAUDE_NOT_FOUND = "ОШИБКА: Claude Code CLI не найден!"
        $msg.CLAUDE_INSTALLING = "Claude Code CLI не найден. Установка через npm..."
        $msg.NPM_NOT_FOUND = "npm не найден! Установите Node.js: https://nodejs.org"
        $msg.NODEJS_INSTALLING = "Устанавливаем Node.js..."
        $msg.CLAUDE_INSTALLED = "Claude Code установлен"
        $msg.CLAUDE_LOGIN = "Войдите в аккаунт Claude:"
        $msg.CLAUDE_INSTALL_FAILED = "Не удалось установить Claude Code!"
        $msg.CLAUDE_INSTALL_MANUAL = "Установите вручную: npm install -g @anthropic-ai/claude-code"
        $msg.ALREADY_INSTALLED = "RClaude уже установлен: $INSTALL_DIR"
        $msg.OPT_UPDATE = "Обновить (настройки сохранятся)"
        $msg.OPT_REINSTALL = "Переустановить (настройки очистятся)"
        $msg.OPT_UNINSTALL = "Удалить (всё удалится)"
        $msg.OPT_EXIT = "Выход"
        $msg.CHOICE = "Выбор"
        $msg.WRONG_CHOICE = "Неверный выбор"
        $msg.UNINSTALLING = "Удаление..."
        $msg.BOT_STOPPED = "Бот остановлен"
        $msg.DIR_REMOVED = "$INSTALL_DIR удалён"
        $msg.PATH_REMOVED = "Удалено из PATH"
        $msg.UNINSTALL_DONE = "RClaude успешно удалён!"
        $msg.TG_SETTINGS = "Настройки Telegram бота"
        $msg.TG_TOKEN_HINT = "Введите токен бота от @BotFather."
        $msg.TG_TOKEN_EXAMPLE = "(Пример: 123456789:AABBccddEEff...)"
        $msg.TG_TOKEN_PROMPT = "Токен бота"
        $msg.TG_TOKEN_EMPTY = "ОШИБКА: Токен не введён"
        $msg.TG_USER_HINT = "Введите ваш Telegram username (обязательно для безопасности)."
        $msg.TG_USER_EMPTY = "ОШИБКА: Username обязателен"
        $msg.TG_USER_PROMPT = "Telegram username (без @)"
        $msg.SETTINGS_KEPT = "Существующие настройки сохранены"
        $msg.BUILDING = "Сборка..."
        $msg.CSPROJ_NOT_FOUND = "ОШИБКА: RClaude.csproj не найден!"
        $msg.RUN_FROM_ROOT = "Запустите install.ps1 из корневой папки проекта."
        $msg.RUNNING_STOPPED = "Запущенный экземпляр остановлен"
        $msg.BUILD_FAILED = "ОШИБКА: Сборка не удалась!"
        $msg.BUILD_OK = "Сборка успешна"
        $msg.CONFIGURING = "Настройка..."
        $msg.CONFIG_CREATED = "Конфиг создан"
        $msg.DB_CREATING = "Создание базы данных..."
        $msg.SETTINGS_SAVED = "Настройки сохранены в БД"
        $msg.CLAUDE_PATH_UPDATED = "Путь Claude CLI обновлён"
        $msg.LAUNCHER_CREATING = "Создание лаунчера..."
        $msg.PATH_ADDED = "Добавлено в PATH"
        $msg.LAUNCHER_CREATED = "Лаунчер создан"
        $msg.INSTALL_DONE = "RClaude успешно установлен!"
        $msg.UPDATE_DONE = "RClaude успешно обновлён!"
        $msg.SETTINGS_PRESERVED = "Настройки и сессии сохранены."
        $msg.INSTALL_LOC = "Место установки"
        $msg.DATABASE = "База данных"
        $msg.START = "Запуск"
        $msg.START_BG = "Фоновый"
        $msg.START_FG = "В терминале"
        $msg.MANAGE = "Управление"
        $msg.STOP = "Остановить"
        $msg.RESTART = "Перезапустить"
        $msg.STATUS = "Статус"
        $msg.LOGS = "Логи"
        $msg.CONFIG = "Настройки"
        $msg.TG_COMMANDS = "Команды Telegram"
        $msg.PERM_SELECT = "Режим разрешений"
        $msg.PERM_FULL = "Полный доступ (все инструменты авто-одобрены)"
        $msg.PERM_ASK = "Запрашивать (Bash/Write/Edit требуют одобрения через Telegram)"
    }
    default {
        $msg.CHECKING = "Tekshirilmoqda..."
        $msg.DOTNET_NOT_FOUND = ".NET SDK topilmadi. Avtomatik o'rnatilmoqda..."
        $msg.DOTNET_INSTALLING = ".NET SDK o'rnatilmoqda..."
        $msg.DOTNET_INSTALL_FAILED = "XATO: .NET SDK o'rnatib bo'lmadi!"
        $msg.DOTNET_INSTALL_MANUAL = "Qo'lda o'rnating: https://dotnet.microsoft.com/download"
        $msg.DOTNET_OLD = "XATO: .NET 8+ kerak (hozirgi:"
        $msg.CLAUDE_NOT_FOUND = "XATO: Claude Code CLI topilmadi!"
        $msg.CLAUDE_INSTALLING = "Claude Code CLI topilmadi. npm orqali o'rnatilmoqda..."
        $msg.NPM_NOT_FOUND = "npm topilmadi! Node.js ni o'rnating: https://nodejs.org"
        $msg.NODEJS_INSTALLING = "Node.js o'rnatilmoqda..."
        $msg.CLAUDE_INSTALLED = "Claude Code o'rnatildi"
        $msg.CLAUDE_LOGIN = "Claude hisobingizga kiring:"
        $msg.CLAUDE_INSTALL_FAILED = "Claude Code o'rnatib bo'lmadi!"
        $msg.CLAUDE_INSTALL_MANUAL = "Qo'lda o'rnating: npm install -g @anthropic-ai/claude-code"
        $msg.ALREADY_INSTALLED = "RClaude allaqachon o'rnatilgan: $INSTALL_DIR"
        $msg.OPT_UPDATE = "Yangilash (sozlamalar saqlanadi)"
        $msg.OPT_REINSTALL = "Qayta o'rnatish (sozlamalar tozalanadi)"
        $msg.OPT_UNINSTALL = "O'chirish (hamma narsa o'chiriladi)"
        $msg.OPT_EXIT = "Chiqish"
        $msg.CHOICE = "Tanlov"
        $msg.WRONG_CHOICE = "Noto'g'ri tanlov"
        $msg.UNINSTALLING = "O'chirish..."
        $msg.BOT_STOPPED = "Bot to'xtatildi"
        $msg.DIR_REMOVED = "$INSTALL_DIR o'chirildi"
        $msg.PATH_REMOVED = "PATH dan olib tashlandi"
        $msg.UNINSTALL_DONE = "RClaude muvaffaqiyatli o'chirildi!"
        $msg.TG_SETTINGS = "Telegram Bot sozlamalari"
        $msg.TG_TOKEN_HINT = "@BotFather dan olingan bot token ni kiriting."
        $msg.TG_TOKEN_EXAMPLE = "(Masalan: 123456789:AABBccddEEff...)"
        $msg.TG_TOKEN_PROMPT = "Bot Token"
        $msg.TG_TOKEN_EMPTY = "XATO: Token kiritilmadi"
        $msg.TG_USER_HINT = "Telegram username ni kiriting (xavfsizlik uchun majburiy)."
        $msg.TG_USER_EMPTY = "XATO: Username kiritilmadi"
        $msg.TG_USER_PROMPT = "Telegram username (@siz)"
        $msg.SETTINGS_KEPT = "Mavjud sozlamalar saqlanadi"
        $msg.BUILDING = "Qurilmoqda..."
        $msg.CSPROJ_NOT_FOUND = "XATO: RClaude.csproj topilmadi!"
        $msg.RUN_FROM_ROOT = "install.ps1 ni loyiha root papkasidan ishga tushiring."
        $msg.RUNNING_STOPPED = "Ishlaydigan bot to'xtatildi"
        $msg.BUILD_FAILED = "XATO: Build muvaffaqiyatsiz!"
        $msg.BUILD_OK = "Build muvaffaqiyatli"
        $msg.CONFIGURING = "Sozlanmoqda..."
        $msg.CONFIG_CREATED = "Config yaratildi"
        $msg.DB_CREATING = "Database yaratilmoqda..."
        $msg.SETTINGS_SAVED = "Sozlamalar DB ga yozildi"
        $msg.CLAUDE_PATH_UPDATED = "Claude CLI yo'li yangilandi"
        $msg.LAUNCHER_CREATING = "Launcher yaratilmoqda..."
        $msg.PATH_ADDED = "PATH ga qo'shildi"
        $msg.LAUNCHER_CREATED = "Launcher yaratildi"
        $msg.INSTALL_DONE = "RClaude muvaffaqiyatli o'rnatildi!"
        $msg.UPDATE_DONE = "RClaude muvaffaqiyatli yangilandi!"
        $msg.SETTINGS_PRESERVED = "Sozlamalar va sessiyalar saqlanib qoldi."
        $msg.INSTALL_LOC = "O'rnatilgan joy"
        $msg.DATABASE = "Database"
        $msg.START = "Ishga tushirish"
        $msg.START_BG = "Background da"
        $msg.START_FG = "Foreground da"
        $msg.MANAGE = "Boshqarish"
        $msg.STOP = "To'xtatish"
        $msg.RESTART = "Qayta tushirish"
        $msg.STATUS = "Holat"
        $msg.LOGS = "Loglar"
        $msg.CONFIG = "Sozlamalar"
        $msg.TG_COMMANDS = "Telegram buyruqlari"
        $msg.PERM_SELECT = "Ruxsat rejimi"
        $msg.PERM_FULL = "To'liq kirish (barcha toollar avtomatik ruxsat)"
        $msg.PERM_ASK = "So'rash (Bash/Write/Edit uchun Telegram orqali ruxsat so'raydi)"
    }
}

# ─── Detect existing installation ────────────────
$MODE = "install"

if (Test-Path "$INSTALL_DIR\app") {
    Write-Host "  $($msg.ALREADY_INSTALLED)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  1) $($msg.OPT_UPDATE)" -ForegroundColor Green
    Write-Host "  2) $($msg.OPT_REINSTALL)" -ForegroundColor Green
    Write-Host "  3) $($msg.OPT_UNINSTALL)" -ForegroundColor Green
    Write-Host "  4) $($msg.OPT_EXIT)" -ForegroundColor Green
    Write-Host ""
    $INSTALL_CHOICE = Read-Host "  $($msg.CHOICE) [1]"
    if (-not $INSTALL_CHOICE) { $INSTALL_CHOICE = "1" }

    switch ($INSTALL_CHOICE) {
        "1" { $MODE = "upgrade" }
        "2" { $MODE = "reinstall" }
        "3" { $MODE = "uninstall" }
        "4" { exit 0 }
        default { Write-Host $msg.WRONG_CHOICE -ForegroundColor Red; exit 1 }
    }
}

# ─── Uninstall ───────────────────────────────────
if ($MODE -eq "uninstall") {
    Write-Host ""
    Write-Host "  $($msg.UNINSTALLING)" -ForegroundColor Yellow

    # Stop running instance
    $pidFile = "$INSTALL_DIR\rclaude.pid"
    if (Test-Path $pidFile) {
        $pid = Get-Content $pidFile -ErrorAction SilentlyContinue
        if ($pid) {
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
            Write-Host "  [OK] $($msg.BOT_STOPPED)" -ForegroundColor Green
        }
    }

    Remove-Item -Path $INSTALL_DIR -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  [OK] $($msg.DIR_REMOVED)" -ForegroundColor Green

    # Remove from user PATH
    $userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
    if ($userPath -and $userPath.Contains($INSTALL_DIR)) {
        $newPath = ($userPath.Split(';') | Where-Object { $_ -ne $INSTALL_DIR }) -join ';'
        [Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
        Write-Host "  [OK] $($msg.PATH_REMOVED)" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "  $($msg.UNINSTALL_DONE)" -ForegroundColor Green
    Write-Host ""
    exit 0
}

# ─── Check prerequisites ─────────────────────────

Write-Host ""
Write-Host "  [1/5] $($msg.CHECKING)" -ForegroundColor Blue

# --- .NET SDK 8+ ---
$NEED_DOTNET = $false
$dotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnetCmd) {
    $NEED_DOTNET = $true
} else {
    $dotnetVer = (dotnet --version 2>$null)
    $major = ($dotnetVer -split '\.')[0]
    if ([int]$major -lt 8) { $NEED_DOTNET = $true }
}

if ($NEED_DOTNET) {
    Write-Host "  $($msg.DOTNET_NOT_FOUND)" -ForegroundColor Yellow
    Write-Host "  $($msg.DOTNET_INSTALLING)"

    $dotnetInstallScript = "$env:TEMP\dotnet-install.ps1"
    Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $dotnetInstallScript -UseBasicParsing

    try {
        & $dotnetInstallScript -Channel 8.0 -InstallDir "$env:USERPROFILE\.dotnet"

        $env:DOTNET_ROOT = "$env:USERPROFILE\.dotnet"
        $env:PATH = "$env:DOTNET_ROOT;$env:PATH"

        # Add to user environment permanently
        [Environment]::SetEnvironmentVariable("DOTNET_ROOT", $env:DOTNET_ROOT, "User")
        $userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
        if (-not $userPath.Contains($env:DOTNET_ROOT)) {
            [Environment]::SetEnvironmentVariable("PATH", "$env:DOTNET_ROOT;$userPath", "User")
        }

        Remove-Item $dotnetInstallScript -ErrorAction SilentlyContinue
        Write-Host "  [OK] .NET $(dotnet --version)" -ForegroundColor Green
    } catch {
        Remove-Item $dotnetInstallScript -ErrorAction SilentlyContinue
        Write-Host "  $($msg.DOTNET_INSTALL_FAILED)" -ForegroundColor Red
        Write-Host "  $($msg.DOTNET_INSTALL_MANUAL)"
        exit 1
    }
} else {
    Write-Host "  [OK] .NET $(dotnet --version)" -ForegroundColor Green
}

# --- Claude Code CLI ---
$CLAUDE_BIN = ""

# Strategy 1: VS Code extension (Windows)
$vscodeDirs = Get-ChildItem "$env:USERPROFILE\.vscode\extensions\anthropic.claude-code-*-win32-*" -ErrorAction SilentlyContinue
foreach ($dir in $vscodeDirs) {
    $binary = Join-Path $dir.FullName "resources\native-binary\claude.exe"
    if (Test-Path $binary) { $CLAUDE_BIN = $binary }
}

# Strategy 2: Global command
if (-not $CLAUDE_BIN) {
    $claudeCmd = Get-Command claude -ErrorAction SilentlyContinue
    if (-not $claudeCmd) { $claudeCmd = Get-Command claude.cmd -ErrorAction SilentlyContinue }
    if ($claudeCmd) { $CLAUDE_BIN = $claudeCmd.Source }
}

# Strategy 3: Auto-install via npm
if (-not $CLAUDE_BIN) {
    Write-Host "  $($msg.CLAUDE_INSTALLING)" -ForegroundColor Yellow

    # Check npm
    $npmCmd = Get-Command npm -ErrorAction SilentlyContinue
    if (-not $npmCmd) {
        Write-Host "  $($msg.NODEJS_INSTALLING)"
        $wingetCmd = Get-Command winget -ErrorAction SilentlyContinue
        if ($wingetCmd) {
            winget install OpenJS.NodeJS.LTS --accept-source-agreements --accept-package-agreements 2>$null
            # Refresh PATH
            $env:PATH = [Environment]::GetEnvironmentVariable("PATH", "Machine") + ";" + [Environment]::GetEnvironmentVariable("PATH", "User")
        } else {
            Write-Host "  $($msg.NPM_NOT_FOUND)" -ForegroundColor Red
            exit 1
        }
    }

    # Install Claude Code
    npm install -g @anthropic-ai/claude-code 2>$null

    # Refresh PATH and check
    $env:PATH = [Environment]::GetEnvironmentVariable("PATH", "Machine") + ";" + [Environment]::GetEnvironmentVariable("PATH", "User")
    $claudeCmd = Get-Command claude -ErrorAction SilentlyContinue
    if (-not $claudeCmd) { $claudeCmd = Get-Command claude.cmd -ErrorAction SilentlyContinue }

    if ($claudeCmd) {
        $CLAUDE_BIN = $claudeCmd.Source
        Write-Host "  [OK] $($msg.CLAUDE_INSTALLED): $CLAUDE_BIN" -ForegroundColor Green

        # Login
        Write-Host ""
        Write-Host "  $($msg.CLAUDE_LOGIN)" -ForegroundColor Yellow
        & claude login
    } else {
        Write-Host "  $($msg.CLAUDE_INSTALL_FAILED)" -ForegroundColor Red
        Write-Host "  $($msg.CLAUDE_INSTALL_MANUAL)"
        exit 1
    }
}

Write-Host "  [OK] Claude CLI: $CLAUDE_BIN" -ForegroundColor Green

# ─── Get settings (skip if upgrade) ──────────────

$DB_PROVIDER = "sqlite"
$DB_CONNECTION = "Data Source=$INSTALL_DIR\rclaude.db"

if ($MODE -eq "upgrade") {
    Write-Host ""
    Write-Host "  [2/5] $($msg.SETTINGS_KEPT)" -ForegroundColor Blue
} else {
    Write-Host ""
    Write-Host "  [2/5] $($msg.TG_SETTINGS)" -ForegroundColor Blue
    Write-Host ""
    Write-Host "  $($msg.TG_TOKEN_HINT)"
    Write-Host "  $($msg.TG_TOKEN_EXAMPLE)"
    Write-Host ""
    $BOT_TOKEN = Read-Host "  $($msg.TG_TOKEN_PROMPT)"

    if (-not $BOT_TOKEN) {
        Write-Host "  $($msg.TG_TOKEN_EMPTY)" -ForegroundColor Red
        exit 1
    }

    Write-Host ""
    Write-Host "  $($msg.TG_USER_HINT)"
    Write-Host ""
    $TG_USERNAME = Read-Host "  $($msg.TG_USER_PROMPT)"

    if (-not $TG_USERNAME) {
        Write-Host "  $($msg.TG_USER_EMPTY)" -ForegroundColor Red
        exit 1
    }

    # Permission mode
    Write-Host ""
    Write-Host "  $($msg.PERM_SELECT):" -ForegroundColor Yellow
    Write-Host "  1) $($msg.PERM_FULL)" -ForegroundColor Green
    Write-Host "  2) $($msg.PERM_ASK)" -ForegroundColor Green
    Write-Host ""
    $PERM_CHOICE = Read-Host "  [1]"
    if (-not $PERM_CHOICE) { $PERM_CHOICE = "1" }

    switch ($PERM_CHOICE) {
        "2" { $PERMISSION_MODE = "ask" }
        default { $PERMISSION_MODE = "full" }
    }
}

# ─── Build ────────────────────────────────────────

Write-Host ""
Write-Host "  [4/5] $($msg.BUILDING)" -ForegroundColor Blue

$csproj = Join-Path $SRC_DIR "RClaude.csproj"
if (-not (Test-Path $csproj)) {
    Write-Host "  $($msg.CSPROJ_NOT_FOUND)" -ForegroundColor Red
    Write-Host "  $($msg.RUN_FROM_ROOT)"
    exit 1
}

# Stop running instance
$pidFile = "$INSTALL_DIR\rclaude.pid"
if (Test-Path $pidFile) {
    $pid = Get-Content $pidFile -ErrorAction SilentlyContinue
    if ($pid) {
        Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        Remove-Item $pidFile -ErrorAction SilentlyContinue
        Write-Host "  [OK] $($msg.RUNNING_STOPPED)" -ForegroundColor Green
        Start-Sleep -Seconds 1
    }
}

# Clean and build
Remove-Item -Path "$SRC_DIR\bin", "$SRC_DIR\obj", "$INSTALL_DIR\app" -Recurse -Force -ErrorAction SilentlyContinue

$buildResult = dotnet publish $csproj -c Release -o "$INSTALL_DIR\app" --self-contained false --verbosity minimal 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "  $($msg.BUILD_FAILED)" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}

Write-Host "  [OK] $($msg.BUILD_OK)" -ForegroundColor Green

# ─── Configure ────────────────────────────────────

Write-Host "  [5/5] $($msg.CONFIGURING)" -ForegroundColor Blue

# Write appsettings.json
$appSettings = @{
    Database = @{ Provider = $DB_PROVIDER }
    ConnectionStrings = @{ DefaultConnection = $DB_CONNECTION }
    Logging = @{
        LogLevel = @{
            Default = "Information"
            "Microsoft.Hosting.Lifetime" = "Information"
        }
    }
} | ConvertTo-Json -Depth 3

Set-Content -Path "$INSTALL_DIR\app\appsettings.json" -Value $appSettings -Encoding UTF8
Write-Host "  [OK] $($msg.CONFIG_CREATED)" -ForegroundColor Green

# Write settings to DB via --init-db
if ($MODE -ne "upgrade") {
    if ($MODE -eq "reinstall" -and (Test-Path "$INSTALL_DIR\rclaude.db")) {
        Remove-Item "$INSTALL_DIR\rclaude.db" -Force -ErrorAction SilentlyContinue
    }

    Write-Host "  $($msg.DB_CREATING)"

    # Map language choice to code
    switch ($LANG_CHOICE) {
        "2" { $BOT_LANG = "en" }
        "3" { $BOT_LANG = "ru" }
        default { $BOT_LANG = "uz" }
    }

    $initArgs = @(
        "$INSTALL_DIR\app\RClaude.dll",
        "--init-db",
        "--bot-token", $BOT_TOKEN,
        "--username", $TG_USERNAME,
        "--claude-path", $CLAUDE_BIN,
        "--permission-mode", $PERMISSION_MODE,
        "--language", $BOT_LANG
    )
    dotnet @initArgs 2>$null
    Write-Host "  [OK] $($msg.SETTINGS_SAVED)" -ForegroundColor Green
} else {
    # Upgrade — only update Claude CLI path
    dotnet "$INSTALL_DIR\app\RClaude.dll" --init-db --update-claude-path --claude-path $CLAUDE_BIN 2>$null
    Write-Host "  [OK] $($msg.CLAUDE_PATH_UPDATED)" -ForegroundColor Green
}

# ─── Create launcher script (rclaude.cmd) ────────

$launcherContent = @'
@echo off
setlocal

set "RCLAUDE_DIR=%USERPROFILE%\.rclaude"
set "APP_DIR=%RCLAUDE_DIR%\app"
set "PID_FILE=%RCLAUDE_DIR%\rclaude.pid"
set "LOG_FILE=%RCLAUDE_DIR%\rclaude.log"

if "%~1"=="" set "CMD=start" & goto :dispatch
set "CMD=%~1"

:dispatch
if /i "%CMD%"=="start" goto :cmd_start
if /i "%CMD%"=="stop" goto :cmd_stop
if /i "%CMD%"=="restart" goto :cmd_restart
if /i "%CMD%"=="status" goto :cmd_status
if /i "%CMD%"=="logs" goto :cmd_logs
if /i "%CMD%"=="run" goto :cmd_run
if /i "%CMD%"=="config" goto :cmd_config
goto :usage

:cmd_start
if exist "%PID_FILE%" (
    for /f %%i in (%PID_FILE%) do (
        tasklist /fi "PID eq %%i" 2>nul | find "%%i" >nul 2>nul
        if not errorlevel 1 (
            echo RClaude is already running (PID: %%i^)
            echo Stop: rclaude stop
            exit /b 1
        )
    )
)
echo Starting RClaude...
set "CLAUDECODE="
set "CLAUDE_CODE_ENTRYPOINT="
powershell -NoProfile -Command "$p = Start-Process -FilePath 'dotnet' -ArgumentList ('\"%APP_DIR%\RClaude.dll\" --contentRoot \"%APP_DIR%\"') -WindowStyle Hidden -PassThru -RedirectStandardOutput '%LOG_FILE%'; $p.Id | Out-File -FilePath '%PID_FILE%' -Encoding ascii -NoNewline"
timeout /t 2 /nobreak >nul
if exist "%PID_FILE%" (
    for /f %%i in (%PID_FILE%) do (
        echo RClaude started (PID: %%i^)
        echo.
        echo Logs:    rclaude logs
        echo Stop:    rclaude stop
        echo Status:  rclaude status
    )
) else (
    echo Failed to start!
    echo Logs: type %LOG_FILE%
)
goto :eof

:cmd_stop
if not exist "%PID_FILE%" (
    echo RClaude is not running.
    goto :eof
)
for /f %%i in (%PID_FILE%) do (
    taskkill /pid %%i /t /f >nul 2>nul
    echo RClaude stopped (PID: %%i^)
)
del "%PID_FILE%" 2>nul
goto :eof

:cmd_restart
call "%~f0" stop
timeout /t 1 /nobreak >nul
call "%~f0" start
goto :eof

:cmd_status
if not exist "%PID_FILE%" (
    echo RClaude is not running.
    goto :eof
)
for /f %%i in (%PID_FILE%) do (
    tasklist /fi "PID eq %%i" 2>nul | find "%%i" >nul 2>nul
    if not errorlevel 1 (
        echo RClaude is running (PID: %%i^)
    ) else (
        echo RClaude is not running (stale PID file^)
        del "%PID_FILE%" 2>nul
    )
)
goto :eof

:cmd_logs
if exist "%LOG_FILE%" (
    powershell -NoProfile -Command "Get-Content '%LOG_FILE%' -Wait"
) else (
    echo Log file not found.
)
goto :eof

:cmd_run
set "CLAUDECODE="
set "CLAUDE_CODE_ENTRYPOINT="
dotnet "%APP_DIR%\RClaude.dll" --contentRoot "%APP_DIR%"
goto :eof

:cmd_config
echo Config: %APP_DIR%\appsettings.json
type "%APP_DIR%\appsettings.json"
echo.
goto :eof

:usage
echo RClaude -- Claude Code Agent via Telegram
echo.
echo Usage:
echo   rclaude start    -- Start in background
echo   rclaude stop     -- Stop
echo   rclaude restart  -- Restart
echo   rclaude status   -- Check status
echo   rclaude logs     -- View logs (real-time)
echo   rclaude run      -- Start in foreground
echo   rclaude config   -- View settings
echo.
goto :eof
'@

Set-Content -Path "$INSTALL_DIR\$BINARY_NAME.cmd" -Value $launcherContent -Encoding ASCII
Write-Host "  [OK] $($msg.LAUNCHER_CREATED)" -ForegroundColor Green

# ─── Add to PATH ──────────────────────────────────

$userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
if (-not $userPath) { $userPath = "" }

if (-not $userPath.Contains($INSTALL_DIR)) {
    [Environment]::SetEnvironmentVariable("PATH", "$INSTALL_DIR;$userPath", "User")
    $env:PATH = "$INSTALL_DIR;$env:PATH"
    Write-Host "  [OK] $($msg.PATH_ADDED)" -ForegroundColor Green
}

# ─── Done ─────────────────────────────────────────

Write-Host ""
if ($MODE -eq "upgrade") {
    Write-Host "  ===================================================" -ForegroundColor Green
    Write-Host "    $($msg.UPDATE_DONE)" -ForegroundColor Green
    Write-Host "  ===================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "  $($msg.SETTINGS_PRESERVED)"
} else {
    Write-Host "  ===================================================" -ForegroundColor Green
    Write-Host "    $($msg.INSTALL_DONE)" -ForegroundColor Green
    Write-Host "  ===================================================" -ForegroundColor Green
}

Write-Host ""
Write-Host "  $($msg.INSTALL_LOC): $INSTALL_DIR" -ForegroundColor Cyan
Write-Host "  $($msg.DATABASE):        sqlite" -ForegroundColor Cyan
Write-Host ""
Write-Host "  $($msg.START):" -ForegroundColor Yellow
Write-Host "    rclaude start    -- $($msg.START_BG)"
Write-Host "    rclaude run      -- $($msg.START_FG)"
Write-Host ""
Write-Host "  $($msg.MANAGE):" -ForegroundColor Yellow
Write-Host "    rclaude stop     -- $($msg.STOP)"
Write-Host "    rclaude restart  -- $($msg.RESTART)"
Write-Host "    rclaude status   -- $($msg.STATUS)"
Write-Host "    rclaude logs     -- $($msg.LOGS)"
Write-Host "    rclaude config   -- $($msg.CONFIG)"
Write-Host ""
Write-Host "  $($msg.TG_COMMANDS):" -ForegroundColor Yellow
Write-Host "    /newsession <name> -- New session"
Write-Host "    /setdir <path>     -- Set folder"
Write-Host "    /session           -- Switch session"
Write-Host "    /help              -- All commands"
Write-Host ""
