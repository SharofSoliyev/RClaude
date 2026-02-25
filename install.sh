#!/bin/bash
set -e

# ═══════════════════════════════════════════════════
#  RClaude Installer — macOS / Linux
#  Claude Code AI Agent via Telegram
# ═══════════════════════════════════════════════════

INSTALL_DIR="$HOME/.rclaude"
BINARY_NAME="rclaude"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SRC_DIR="$SCRIPT_DIR/src/RClaude"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# ─── Language Selection ──────────────────────────

echo ""
echo -e "${CYAN}═══════════════════════════════════════════════════${NC}"
echo -e "${CYAN}  RClaude — Claude Code Agent via Telegram${NC}"
echo -e "${CYAN}═══════════════════════════════════════════════════${NC}"
echo ""
echo -e "  ${GREEN}1${NC}) O'zbek tili"
echo -e "  ${GREEN}2${NC}) English"
echo -e "  ${GREEN}3${NC}) Русский"
echo ""
read -p "  Til / Language / Язык [1]: " LANG_CHOICE
LANG_CHOICE=${LANG_CHOICE:-1}

# ─── i18n Messages ───────────────────────────────

case "$LANG_CHOICE" in
    2)
        MSG_CHECKING="Checking prerequisites..."
        MSG_DOTNET_NOT_FOUND=".NET SDK not found. Installing automatically..."
        MSG_DOTNET_INSTALLING="Installing .NET SDK..."
        MSG_DOTNET_INSTALL_FAILED="ERROR: .NET SDK installation failed!"
        MSG_DOTNET_INSTALL_MANUAL="Install manually: https://dotnet.microsoft.com/download"
        MSG_DOTNET_OLD="ERROR: .NET 8+ required (current:"
        MSG_CLAUDE_NOT_FOUND="ERROR: Claude Code CLI not found!"
        MSG_CLAUDE_HINT1="Claude Code extension must be installed in VS Code."
        MSG_CLAUDE_HINT2="Or: npm install -g @anthropic-ai/claude-code"
        MSG_ALREADY_INSTALLED="RClaude is already installed: $INSTALL_DIR"
        MSG_OPT_UPDATE="Update (settings preserved)"
        MSG_OPT_REINSTALL="Reinstall (settings cleared)"
        MSG_OPT_UNINSTALL="Uninstall (everything removed)"
        MSG_OPT_EXIT="Exit"
        MSG_CHOICE="Choice"
        MSG_WRONG_CHOICE="Invalid choice"
        MSG_UNINSTALLING="Uninstalling..."
        MSG_BOT_STOPPED="Bot stopped"
        MSG_DIR_REMOVED="$INSTALL_DIR removed"
        MSG_PATH_REMOVED="Removed from PATH"
        MSG_UNINSTALL_DONE="RClaude successfully uninstalled!"
        MSG_TG_SETTINGS="Telegram Bot settings"
        MSG_TG_TOKEN_HINT="Enter bot token from @BotFather."
        MSG_TG_TOKEN_EXAMPLE="(Example: 123456789:AABBccddEEff...)"
        MSG_TG_TOKEN_PROMPT="Bot Token"
        MSG_TG_TOKEN_EMPTY="ERROR: Token not provided"
        MSG_TG_USER_HINT="Enter your Telegram username (required for security)."
        MSG_TG_USER_PROMPT="Telegram username (without @)"
        MSG_TG_USER_EMPTY="ERROR: Username is required"
        MSG_SETTINGS_KEPT="Existing settings preserved"
        MSG_DB_SELECT="Select database"
        MSG_DB_SQLITE="SQLite (recommended — no setup needed)"
        MSG_DB_PG="PostgreSQL (for larger projects)"
        MSG_DB_KEPT="Database preserved"
        MSG_BUILDING="Building..."
        MSG_CSPROJ_NOT_FOUND="ERROR: src/RClaude/RClaude.csproj not found!"
        MSG_RUN_FROM_ROOT="Run install.sh from the project root folder."
        MSG_RUNNING_STOPPED="Running instance stopped"
        MSG_BUILD_FAILED="ERROR: Build failed!"
        MSG_BUILD_CHECK="Check manually: dotnet publish"
        MSG_BUILD_OK="Build successful"
        MSG_CONFIGURING="Configuring..."
        MSG_CONFIG_CREATED="Config created"
        MSG_DB_CREATING="Creating database..."
        MSG_SQLITE3_NOT_FOUND="ERROR: sqlite3 not found!"
        MSG_SETTINGS_SAVED="Settings saved to DB"
        MSG_CLAUDE_PATH_UPDATED="Claude CLI path updated"
        MSG_LAUNCHER_CREATING="Creating launcher..."
        MSG_PATH_ADDED="Added to PATH"
        MSG_LAUNCHER_CREATED="Launcher created"
        MSG_INSTALL_DONE="RClaude successfully installed!"
        MSG_UPDATE_DONE="RClaude successfully updated!"
        MSG_SETTINGS_PRESERVED="Settings and sessions preserved."
        MSG_INSTALL_LOC="Install location"
        MSG_DATABASE="Database"
        MSG_START="Start"
        MSG_START_BG="Background"
        MSG_START_FG="Foreground"
        MSG_MANAGE="Manage"
        MSG_STOP="Stop"
        MSG_RESTART="Restart"
        MSG_STATUS="Status"
        MSG_LOGS="Logs"
        MSG_CONFIG="Settings"
        MSG_TG_COMMANDS="Telegram commands"
        MSG_NEW_TERMINAL="Open a new terminal or run:"
        MSG_PERM_SELECT="Permission mode"
        MSG_PERM_FULL="Full Access (all tools auto-approved)"
        MSG_PERM_ASK="Ask Permission (Bash/Write/Edit require approval via Telegram)"
        MSG_OPENAI_HINT="OpenAI API key for audio messages (optional)"
        MSG_OPENAI_HINT_DESC="If provided, bot will process voice messages with Whisper STT and optimize prompts with GPT."
        MSG_OPENAI_PROMPT="OpenAI API key (press Enter to skip)"
        MSG_OPENAI_SKIPPED="Audio processing disabled (no API key)"
        MSG_OPENAI_ENABLED="Audio processing enabled"
        MSG_CLAUDE_INSTALLING="Claude Code CLI not found. Installing via npm..."
        MSG_NPM_NOT_FOUND="npm not found! Install Node.js: https://nodejs.org"
        MSG_NODEJS_INSTALLING="Installing Node.js..."
        MSG_CLAUDE_INSTALLED="Claude Code installed"
        MSG_CLAUDE_LOGIN="Log in to your Claude account:"
        MSG_CLAUDE_INSTALL_FAILED="Claude Code installation failed!"
        MSG_CLAUDE_INSTALL_MANUAL="Run manually: npm install -g @anthropic-ai/claude-code"
        ;;
    3)
        MSG_CHECKING="Проверка..."
        MSG_DOTNET_NOT_FOUND=".NET SDK не найден. Устанавливаем автоматически..."
        MSG_DOTNET_INSTALLING="Установка .NET SDK..."
        MSG_DOTNET_INSTALL_FAILED="ОШИБКА: Установка .NET SDK не удалась!"
        MSG_DOTNET_INSTALL_MANUAL="Установите вручную: https://dotnet.microsoft.com/download"
        MSG_DOTNET_OLD="ОШИБКА: Нужен .NET 8+ (текущий:"
        MSG_CLAUDE_NOT_FOUND="ОШИБКА: Claude Code CLI не найден!"
        MSG_CLAUDE_HINT1="Расширение Claude Code должно быть установлено в VS Code."
        MSG_CLAUDE_HINT2="Или: npm install -g @anthropic-ai/claude-code"
        MSG_ALREADY_INSTALLED="RClaude уже установлен: $INSTALL_DIR"
        MSG_OPT_UPDATE="Обновить (настройки сохранятся)"
        MSG_OPT_REINSTALL="Переустановить (настройки очистятся)"
        MSG_OPT_UNINSTALL="Удалить (всё удалится)"
        MSG_OPT_EXIT="Выход"
        MSG_CHOICE="Выбор"
        MSG_WRONG_CHOICE="Неверный выбор"
        MSG_UNINSTALLING="Удаление..."
        MSG_BOT_STOPPED="Бот остановлен"
        MSG_DIR_REMOVED="$INSTALL_DIR удалён"
        MSG_PATH_REMOVED="Удалено из PATH"
        MSG_UNINSTALL_DONE="RClaude успешно удалён!"
        MSG_TG_SETTINGS="Настройки Telegram бота"
        MSG_TG_TOKEN_HINT="Введите токен бота от @BotFather."
        MSG_TG_TOKEN_EXAMPLE="(Пример: 123456789:AABBccddEEff...)"
        MSG_TG_TOKEN_PROMPT="Токен бота"
        MSG_TG_TOKEN_EMPTY="ОШИБКА: Токен не введён"
        MSG_TG_USER_HINT="Введите ваш Telegram username (обязательно для безопасности)."
        MSG_TG_USER_PROMPT="Telegram username (без @)"
        MSG_TG_USER_EMPTY="ОШИБКА: Username обязателен"
        MSG_SETTINGS_KEPT="Существующие настройки сохранены"
        MSG_DB_SELECT="Выберите базу данных"
        MSG_DB_SQLITE="SQLite (рекомендуется — настройка не нужна)"
        MSG_DB_PG="PostgreSQL (для крупных проектов)"
        MSG_DB_KEPT="База данных сохранена"
        MSG_BUILDING="Сборка..."
        MSG_CSPROJ_NOT_FOUND="ОШИБКА: src/RClaude/RClaude.csproj не найден!"
        MSG_RUN_FROM_ROOT="Запустите install.sh из корневой папки проекта."
        MSG_RUNNING_STOPPED="Запущенный экземпляр остановлен"
        MSG_BUILD_FAILED="ОШИБКА: Сборка не удалась!"
        MSG_BUILD_CHECK="Проверьте вручную: dotnet publish"
        MSG_BUILD_OK="Сборка успешна"
        MSG_CONFIGURING="Настройка..."
        MSG_CONFIG_CREATED="Конфиг создан"
        MSG_DB_CREATING="Создание базы данных..."
        MSG_SQLITE3_NOT_FOUND="ОШИБКА: sqlite3 не найден!"
        MSG_SETTINGS_SAVED="Настройки сохранены в БД"
        MSG_CLAUDE_PATH_UPDATED="Путь Claude CLI обновлён"
        MSG_LAUNCHER_CREATING="Создание лаунчера..."
        MSG_PATH_ADDED="Добавлено в PATH"
        MSG_LAUNCHER_CREATED="Лаунчер создан"
        MSG_INSTALL_DONE="RClaude успешно установлен!"
        MSG_UPDATE_DONE="RClaude успешно обновлён!"
        MSG_SETTINGS_PRESERVED="Настройки и сессии сохранены."
        MSG_INSTALL_LOC="Место установки"
        MSG_DATABASE="База данных"
        MSG_START="Запуск"
        MSG_START_BG="Фоновый"
        MSG_START_FG="В терминале"
        MSG_MANAGE="Управление"
        MSG_STOP="Остановить"
        MSG_RESTART="Перезапустить"
        MSG_STATUS="Статус"
        MSG_LOGS="Логи"
        MSG_CONFIG="Настройки"
        MSG_TG_COMMANDS="Команды Telegram"
        MSG_NEW_TERMINAL="Откройте новый терминал или выполните:"
        MSG_PERM_SELECT="Режим разрешений"
        MSG_PERM_FULL="Полный доступ (все инструменты авто-одобрены)"
        MSG_PERM_ASK="Запрашивать (Bash/Write/Edit требуют одобрения через Telegram)"
        MSG_OPENAI_HINT="OpenAI API ключ для голосовых сообщений (опционально)"
        MSG_OPENAI_HINT_DESC="Если указан, бот будет обрабатывать голосовые сообщения с помощью Whisper STT и оптимизировать промпты с помощью GPT."
        MSG_OPENAI_PROMPT="OpenAI API ключ (Enter чтобы пропустить)"
        MSG_OPENAI_SKIPPED="Обработка аудио отключена (нет API ключа)"
        MSG_OPENAI_ENABLED="Обработка аудио включена"
        MSG_CLAUDE_INSTALLING="Claude Code CLI не найден. Установка через npm..."
        MSG_NPM_NOT_FOUND="npm не найден! Установите Node.js: https://nodejs.org"
        MSG_NODEJS_INSTALLING="Устанавливаем Node.js..."
        MSG_CLAUDE_INSTALLED="Claude Code установлен"
        MSG_CLAUDE_LOGIN="Войдите в аккаунт Claude:"
        MSG_CLAUDE_INSTALL_FAILED="Не удалось установить Claude Code!"
        MSG_CLAUDE_INSTALL_MANUAL="Установите вручную: npm install -g @anthropic-ai/claude-code"
        ;;
    *)
        MSG_CHECKING="Tekshirilmoqda..."
        MSG_DOTNET_NOT_FOUND=".NET SDK topilmadi. Avtomatik o'rnatilmoqda..."
        MSG_DOTNET_INSTALLING=".NET SDK o'rnatilmoqda..."
        MSG_DOTNET_INSTALL_FAILED="XATO: .NET SDK o'rnatib bo'lmadi!"
        MSG_DOTNET_INSTALL_MANUAL="Qo'lda o'rnating: https://dotnet.microsoft.com/download"
        MSG_DOTNET_OLD="XATO: .NET 8+ kerak (hozirgi:"
        MSG_CLAUDE_NOT_FOUND="XATO: Claude Code CLI topilmadi!"
        MSG_CLAUDE_HINT1="VS Code da Claude Code extension o'rnatilgan bo'lishi kerak."
        MSG_CLAUDE_HINT2="Yoki: npm install -g @anthropic-ai/claude-code"
        MSG_ALREADY_INSTALLED="RClaude allaqachon o'rnatilgan: $INSTALL_DIR"
        MSG_OPT_UPDATE="Yangilash (sozlamalar saqlanadi)"
        MSG_OPT_REINSTALL="Qayta o'rnatish (sozlamalar tozalanadi)"
        MSG_OPT_UNINSTALL="O'chirish (hamma narsa o'chiriladi)"
        MSG_OPT_EXIT="Chiqish"
        MSG_CHOICE="Tanlov"
        MSG_WRONG_CHOICE="Noto'g'ri tanlov"
        MSG_UNINSTALLING="O'chirish..."
        MSG_BOT_STOPPED="Bot to'xtatildi"
        MSG_DIR_REMOVED="$INSTALL_DIR o'chirildi"
        MSG_PATH_REMOVED="PATH dan olib tashlandi"
        MSG_UNINSTALL_DONE="RClaude muvaffaqiyatli o'chirildi!"
        MSG_TG_SETTINGS="Telegram Bot sozlamalari"
        MSG_TG_TOKEN_HINT="@BotFather dan olingan bot token ni kiriting."
        MSG_TG_TOKEN_EXAMPLE="(Masalan: 123456789:AABBccddEEff...)"
        MSG_TG_TOKEN_PROMPT="Bot Token"
        MSG_TG_TOKEN_EMPTY="XATO: Token kiritilmadi"
        MSG_TG_USER_HINT="Telegram username ni kiriting (xavfsizlik uchun majburiy)."
        MSG_TG_USER_PROMPT="Telegram username (@siz)"
        MSG_TG_USER_EMPTY="XATO: Username kiritilmadi"
        MSG_SETTINGS_KEPT="Mavjud sozlamalar saqlanadi"
        MSG_DB_SELECT="Database tanlang"
        MSG_DB_SQLITE="SQLite (tavsiya — sozlash kerak emas)"
        MSG_DB_PG="PostgreSQL (katta loyihalar uchun)"
        MSG_DB_KEPT="Database saqlanadi"
        MSG_BUILDING="Qurilmoqda..."
        MSG_CSPROJ_NOT_FOUND="XATO: src/RClaude/RClaude.csproj topilmadi!"
        MSG_RUN_FROM_ROOT="install.sh ni loyiha root papkasidan ishga tushiring."
        MSG_RUNNING_STOPPED="Ishlaydigan bot to'xtatildi"
        MSG_BUILD_FAILED="XATO: Build muvaffaqiyatsiz!"
        MSG_BUILD_CHECK="Qo'lda tekshiring: dotnet publish"
        MSG_BUILD_OK="Build muvaffaqiyatli"
        MSG_CONFIGURING="Sozlanmoqda..."
        MSG_CONFIG_CREATED="Config yaratildi"
        MSG_DB_CREATING="Database yaratilmoqda..."
        MSG_SQLITE3_NOT_FOUND="XATO: sqlite3 topilmadi!"
        MSG_SETTINGS_SAVED="Sozlamalar DB ga yozildi"
        MSG_CLAUDE_PATH_UPDATED="Claude CLI yo'li yangilandi"
        MSG_LAUNCHER_CREATING="Launcher yaratilmoqda..."
        MSG_PATH_ADDED="PATH ga qo'shildi"
        MSG_LAUNCHER_CREATED="Launcher yaratildi"
        MSG_INSTALL_DONE="RClaude muvaffaqiyatli o'rnatildi!"
        MSG_UPDATE_DONE="RClaude muvaffaqiyatli yangilandi!"
        MSG_SETTINGS_PRESERVED="Sozlamalar va sessiyalar saqlanib qoldi."
        MSG_INSTALL_LOC="O'rnatilgan joy"
        MSG_DATABASE="Database"
        MSG_START="Ishga tushirish"
        MSG_START_BG="Background da"
        MSG_START_FG="Foreground da"
        MSG_MANAGE="Boshqarish"
        MSG_STOP="To'xtatish"
        MSG_RESTART="Qayta tushirish"
        MSG_STATUS="Holat"
        MSG_LOGS="Loglar"
        MSG_CONFIG="Sozlamalar"
        MSG_TG_COMMANDS="Telegram buyruqlari"
        MSG_NEW_TERMINAL="Yangi terminal oching yoki:"
        MSG_PERM_SELECT="Ruxsat rejimi"
        MSG_PERM_FULL="To'liq kirish (barcha toollar avtomatik ruxsat)"
        MSG_PERM_ASK="So'rash (Bash/Write/Edit uchun Telegram orqali ruxsat so'raydi)"
        MSG_OPENAI_HINT="Audio xabarlar uchun OpenAI API key (ixtiyoriy)"
        MSG_OPENAI_HINT_DESC="Agar kiritilsa, bot ovozli xabarlarni Whisper STT bilan taniydi va promptlarni GPT bilan optimizatsiya qiladi."
        MSG_OPENAI_PROMPT="OpenAI API key (o'tkazib yuborish uchun Enter)"
        MSG_OPENAI_SKIPPED="Audio qayta ishlash o'chirilgan (API key yo'q)"
        MSG_OPENAI_ENABLED="Audio qayta ishlash yoqilgan"
        MSG_CLAUDE_INSTALLING="Claude Code CLI topilmadi. npm orqali o'rnatilmoqda..."
        MSG_NPM_NOT_FOUND="npm topilmadi! Node.js ni o'rnating: https://nodejs.org"
        MSG_NODEJS_INSTALLING="Node.js o'rnatilmoqda..."
        MSG_CLAUDE_INSTALLED="Claude Code o'rnatildi"
        MSG_CLAUDE_LOGIN="Claude hisobingizga kiring:"
        MSG_CLAUDE_INSTALL_FAILED="Claude Code o'rnatib bo'lmadi!"
        MSG_CLAUDE_INSTALL_MANUAL="Qo'lda o'rnating: npm install -g @anthropic-ai/claude-code"
        ;;
esac

# ─── Detect existing installation ────────────────
MODE="install"

if [ -d "$INSTALL_DIR/app" ]; then
    echo -e "${YELLOW}  $MSG_ALREADY_INSTALLED${NC}"
    echo ""
    echo -e "  ${GREEN}1${NC}) $MSG_OPT_UPDATE"
    echo -e "  ${GREEN}2${NC}) $MSG_OPT_REINSTALL"
    echo -e "  ${GREEN}3${NC}) $MSG_OPT_UNINSTALL"
    echo -e "  ${GREEN}4${NC}) $MSG_OPT_EXIT"
    echo ""
    read -p "  $MSG_CHOICE [1]: " INSTALL_CHOICE
    INSTALL_CHOICE=${INSTALL_CHOICE:-1}

    case "$INSTALL_CHOICE" in
        1) MODE="upgrade" ;;
        2) MODE="reinstall" ;;
        3) MODE="uninstall" ;;
        4) exit 0 ;;
        *) echo -e "${RED}$MSG_WRONG_CHOICE${NC}"; exit 1 ;;
    esac
fi

# ─── Uninstall ───────────────────────────────────
if [ "$MODE" = "uninstall" ]; then
    echo ""
    echo -e "${YELLOW}$MSG_UNINSTALLING${NC}"

    if [ -f "$INSTALL_DIR/rclaude.pid" ] && kill -0 "$(cat "$INSTALL_DIR/rclaude.pid" 2>/dev/null)" 2>/dev/null; then
        kill "$(cat "$INSTALL_DIR/rclaude.pid")" 2>/dev/null || true
        echo -e "  ${GREEN}✓${NC} $MSG_BOT_STOPPED"
    fi

    rm -rf "$INSTALL_DIR"
    echo -e "  ${GREEN}✓${NC} $MSG_DIR_REMOVED"

    # Remove symlink if exists
    if [ -L "/usr/local/bin/$BINARY_NAME" ]; then
        rm -f "/usr/local/bin/$BINARY_NAME" 2>/dev/null || true
    fi

    SHELL_RC=""
    [ -f "$HOME/.zshrc" ] && SHELL_RC="$HOME/.zshrc"
    [ -z "$SHELL_RC" ] && [ -f "$HOME/.bashrc" ] && SHELL_RC="$HOME/.bashrc"

    if [ -n "$SHELL_RC" ]; then
        sed -i '' '/# RClaude/d' "$SHELL_RC" 2>/dev/null || true
        sed -i '' '/\.rclaude/d' "$SHELL_RC" 2>/dev/null || true
        echo -e "  ${GREEN}✓${NC} $MSG_PATH_REMOVED"
    fi

    echo ""
    echo -e "${GREEN}$MSG_UNINSTALL_DONE${NC}"
    echo ""
    exit 0
fi

# ─── Check prerequisites ─────────────────────────

echo -e "${BLUE}[1/5]${NC} $MSG_CHECKING"

NEED_DOTNET=false
if ! command -v dotnet &> /dev/null; then
    NEED_DOTNET=true
else
    DOTNET_VER=$(dotnet --version 2>/dev/null | cut -d. -f1)
    if [ "$DOTNET_VER" -lt 8 ] 2>/dev/null; then
        NEED_DOTNET=true
    fi
fi

if [ "$NEED_DOTNET" = true ]; then
    echo -e "  ${YELLOW}$MSG_DOTNET_NOT_FOUND${NC}"
    echo -e "  $MSG_DOTNET_INSTALLING"

    # Auto-install .NET SDK via official script
    curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh

    if /tmp/dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet" 2>&1; then
        export DOTNET_ROOT="$HOME/.dotnet"
        export PATH="$DOTNET_ROOT:$PATH"

        # Add to shell rc for future use
        SHELL_RC_DOTNET=""
        [ -f "$HOME/.zshrc" ] && SHELL_RC_DOTNET="$HOME/.zshrc"
        [ -z "$SHELL_RC_DOTNET" ] && [ -f "$HOME/.bashrc" ] && SHELL_RC_DOTNET="$HOME/.bashrc"

        if [ -n "$SHELL_RC_DOTNET" ] && ! grep -q "DOTNET_ROOT" "$SHELL_RC_DOTNET" 2>/dev/null; then
            echo "" >> "$SHELL_RC_DOTNET"
            echo "# .NET SDK" >> "$SHELL_RC_DOTNET"
            echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> "$SHELL_RC_DOTNET"
            echo 'export PATH="$DOTNET_ROOT:$PATH"' >> "$SHELL_RC_DOTNET"
        fi

        rm -f /tmp/dotnet-install.sh
        echo -e "  ${GREEN}✓${NC} .NET $(dotnet --version)"
    else
        rm -f /tmp/dotnet-install.sh
        echo -e "${RED}$MSG_DOTNET_INSTALL_FAILED${NC}"
        echo "$MSG_DOTNET_INSTALL_MANUAL"
        exit 1
    fi
else
    echo -e "  ${GREEN}✓${NC} .NET $(dotnet --version)"
fi

CLAUDE_BIN=""
for dir in "$HOME"/.vscode/extensions/anthropic.claude-code-*-darwin-*/resources/native-binary/claude; do
    [ -f "$dir" ] && CLAUDE_BIN="$dir"
done
# Linux VS Code extensions
for dir in "$HOME"/.vscode/extensions/anthropic.claude-code-*-linux-*/resources/native-binary/claude; do
    [ -f "$dir" ] && CLAUDE_BIN="$dir"
done
[ -z "$CLAUDE_BIN" ] && command -v claude &> /dev/null && CLAUDE_BIN=$(which claude)

if [ -z "$CLAUDE_BIN" ]; then
    echo -e "  ${YELLOW}$MSG_CLAUDE_INSTALLING${NC}"

    # npm tekshirish, yo'q bo'lsa Node.js o'rnatish
    if ! command -v npm &> /dev/null; then
        echo -e "  $MSG_NODEJS_INSTALLING"
        if command -v brew &> /dev/null; then
            brew install node
        elif [ -f /etc/debian_version ]; then
            sudo apt-get update -qq && sudo apt-get install -y -qq nodejs npm
        elif [ -f /etc/redhat-release ]; then
            sudo dnf install -y nodejs npm
        else
            echo -e "${RED}$MSG_NPM_NOT_FOUND${NC}"
            exit 1
        fi
    fi

    # Claude Code o'rnatish
    npm install -g @anthropic-ai/claude-code 2>&1

    if command -v claude &> /dev/null; then
        CLAUDE_BIN=$(which claude)
        echo -e "  ${GREEN}✓${NC} $MSG_CLAUDE_INSTALLED: $CLAUDE_BIN"

        # Login so'rash
        echo ""
        echo -e "  ${YELLOW}$MSG_CLAUDE_LOGIN${NC}"
        claude login
    else
        echo -e "${RED}$MSG_CLAUDE_INSTALL_FAILED${NC}"
        echo "$MSG_CLAUDE_INSTALL_MANUAL"
        exit 1
    fi
fi
echo -e "  ${GREEN}✓${NC} Claude CLI: $CLAUDE_BIN"

# ─── Get settings (skip if upgrade) ──────────────

if [ "$MODE" = "upgrade" ]; then
    echo ""
    echo -e "${BLUE}[2/5]${NC} $MSG_SETTINGS_KEPT"

    if [ -f "$INSTALL_DIR/app/appsettings.json" ]; then
        DB_PROVIDER=$(python3 -c "import json; print(json.load(open('$INSTALL_DIR/app/appsettings.json'))['Database']['Provider'])" 2>/dev/null || echo "sqlite")
        DB_CONNECTION=$(python3 -c "import json; print(json.load(open('$INSTALL_DIR/app/appsettings.json'))['ConnectionStrings']['DefaultConnection'])" 2>/dev/null || echo "Data Source=$INSTALL_DIR/rclaude.db")
    else
        DB_PROVIDER="sqlite"
        DB_CONNECTION="Data Source=$INSTALL_DIR/rclaude.db"
    fi
else
    echo ""
    echo -e "${BLUE}[2/5]${NC} $MSG_TG_SETTINGS"
    echo ""
    echo -e "  $MSG_TG_TOKEN_HINT"
    echo -e "  $MSG_TG_TOKEN_EXAMPLE"
    echo ""
    read -p "  $MSG_TG_TOKEN_PROMPT: " BOT_TOKEN

    if [ -z "$BOT_TOKEN" ]; then
        echo -e "${RED}$MSG_TG_TOKEN_EMPTY${NC}"
        exit 1
    fi

    echo ""
    echo -e "  $MSG_TG_USER_HINT"
    echo ""
    read -p "  $MSG_TG_USER_PROMPT: " TG_USERNAME

    if [ -z "$TG_USERNAME" ]; then
        echo -e "${RED}$MSG_TG_USER_EMPTY${NC}"
        exit 1
    fi

    # OpenAI API Key (optional)
    echo ""
    echo -e "  ${YELLOW}$MSG_OPENAI_HINT${NC}"
    echo -e "  ${GRAY}$MSG_OPENAI_HINT_DESC${NC}"
    echo ""
    read -p "  $MSG_OPENAI_PROMPT: " OPENAI_KEY

    if [ -n "$OPENAI_KEY" ]; then
        echo -e "  ${GREEN}$MSG_OPENAI_ENABLED${NC}"
    else
        echo -e "  ${GRAY}$MSG_OPENAI_SKIPPED${NC}"
    fi

    # Permission mode
    echo ""
    echo -e "  ${YELLOW}$MSG_PERM_SELECT:${NC}"
    echo -e "  ${GREEN}1${NC}) $MSG_PERM_FULL"
    echo -e "  ${GREEN}2${NC}) $MSG_PERM_ASK"
    echo ""
    read -p "  [1]: " PERM_CHOICE
    PERM_CHOICE=${PERM_CHOICE:-1}

    case "$PERM_CHOICE" in
        2) PERMISSION_MODE="ask" ;;
        *) PERMISSION_MODE="full" ;;
    esac

    DB_PROVIDER="sqlite"
    DB_CONNECTION="Data Source=$INSTALL_DIR/rclaude.db"
fi

# ─── Build ────────────────────────────────────────

echo ""
echo -e "${BLUE}[4/5]${NC} $MSG_BUILDING"

if [ ! -f "$SRC_DIR/RClaude.csproj" ]; then
    echo -e "${RED}$MSG_CSPROJ_NOT_FOUND${NC}"
    echo "$MSG_RUN_FROM_ROOT"
    exit 1
fi

# Stop running instance
if [ -f "$INSTALL_DIR/rclaude.pid" ] && kill -0 "$(cat "$INSTALL_DIR/rclaude.pid" 2>/dev/null)" 2>/dev/null; then
    kill "$(cat "$INSTALL_DIR/rclaude.pid")" 2>/dev/null || true
    rm -f "$INSTALL_DIR/rclaude.pid"
    echo -e "  ${GREEN}✓${NC} $MSG_RUNNING_STOPPED"
    sleep 1
fi

rm -rf "$SRC_DIR/bin" "$SRC_DIR/obj" "$INSTALL_DIR/app"

if ! dotnet publish "$SRC_DIR/RClaude.csproj" \
    -c Release \
    -o "$INSTALL_DIR/app" \
    --self-contained false \
    --verbosity minimal 2>&1; then
    echo -e "${RED}$MSG_BUILD_FAILED${NC}"
    echo "$MSG_BUILD_CHECK $SRC_DIR/RClaude.csproj -c Release"
    exit 1
fi

echo -e "  ${GREEN}✓${NC} $MSG_BUILD_OK"

# ─── Configure ────────────────────────────────────

echo -e "${BLUE}[5/5]${NC} $MSG_CONFIGURING"

cat > "$INSTALL_DIR/app/appsettings.json" << JSONEOF
{
  "Database": {
    "Provider": "$DB_PROVIDER"
  },
  "ConnectionStrings": {
    "DefaultConnection": "$DB_CONNECTION"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
JSONEOF

echo -e "  ${GREEN}✓${NC} $MSG_CONFIG_CREATED"

# Write settings to DB
if [ "$MODE" != "upgrade" ]; then
    if [ "$MODE" = "reinstall" ] && [ "$DB_PROVIDER" = "sqlite" ]; then
        rm -f "$INSTALL_DIR/rclaude.db"
    fi

    echo -e "  $MSG_DB_CREATING"

    if [ -n "$TG_USERNAME" ]; then
        USERNAMES_JSON="[\"$TG_USERNAME\"]"
    else
        USERNAMES_JSON="[]"
    fi

    # Map language choice to code
    case "$LANG_CHOICE" in
        2) BOT_LANG="en" ;;
        3) BOT_LANG="ru" ;;
        *) BOT_LANG="uz" ;;
    esac

    INIT_ARGS=(
        "$INSTALL_DIR/app/RClaude.dll"
        "--init-db"
        "--bot-token" "$BOT_TOKEN"
        "--username" "$TG_USERNAME"
        "--claude-path" "$CLAUDE_BIN"
        "--permission-mode" "$PERMISSION_MODE"
        "--language" "$BOT_LANG"
    )

    if [ -n "$OPENAI_KEY" ]; then
        INIT_ARGS+=("--openai-key" "$OPENAI_KEY")
    fi

    dotnet "${INIT_ARGS[@]}"
    echo -e "  ${GREEN}✓${NC} $MSG_SETTINGS_SAVED"
else
    # Upgrade — only update Claude CLI path
    dotnet "$INSTALL_DIR/app/RClaude.dll" --init-db --update-claude-path --claude-path "$CLAUDE_BIN"
    echo -e "  ${GREEN}✓${NC} $MSG_CLAUDE_PATH_UPDATED"
fi

# ─── Create launcher script ──────────────────────

cat > "$INSTALL_DIR/$BINARY_NAME" << 'LAUNCHER'
#!/bin/bash
RCLAUDE_DIR="$HOME/.rclaude"
APP_DIR="$RCLAUDE_DIR/app"

case "${1:-start}" in
    start)
        if [ -f "$RCLAUDE_DIR/rclaude.pid" ] && kill -0 "$(cat "$RCLAUDE_DIR/rclaude.pid")" 2>/dev/null; then
            echo "RClaude is already running (PID: $(cat "$RCLAUDE_DIR/rclaude.pid"))"
            echo "Stop: rclaude stop"
            exit 1
        fi
        echo "Starting RClaude..."
        unset CLAUDECODE
        unset CLAUDE_CODE_ENTRYPOINT
        nohup dotnet "$APP_DIR/RClaude.dll" --contentRoot "$APP_DIR" > "$RCLAUDE_DIR/rclaude.log" 2>&1 &
        echo $! > "$RCLAUDE_DIR/rclaude.pid"
        sleep 2
        if kill -0 "$(cat "$RCLAUDE_DIR/rclaude.pid")" 2>/dev/null; then
            echo "RClaude started (PID: $(cat "$RCLAUDE_DIR/rclaude.pid"))"
            echo ""
            echo "Logs:    rclaude logs"
            echo "Stop:    rclaude stop"
            echo "Status:  rclaude status"
        else
            echo "Failed to start!"
            echo "Logs: cat $RCLAUDE_DIR/rclaude.log"
        fi
        ;;
    stop)
        if [ -f "$RCLAUDE_DIR/rclaude.pid" ]; then
            PID=$(cat "$RCLAUDE_DIR/rclaude.pid")
            if kill -0 "$PID" 2>/dev/null; then
                kill "$PID"
                rm -f "$RCLAUDE_DIR/rclaude.pid"
                echo "RClaude stopped (PID: $PID)"
            else
                rm -f "$RCLAUDE_DIR/rclaude.pid"
                echo "Process already stopped."
            fi
        else
            echo "RClaude is not running."
        fi
        ;;
    restart)
        "$0" stop
        sleep 1
        "$0" start
        ;;
    status)
        if [ -f "$RCLAUDE_DIR/rclaude.pid" ] && kill -0 "$(cat "$RCLAUDE_DIR/rclaude.pid")" 2>/dev/null; then
            echo "RClaude is running (PID: $(cat "$RCLAUDE_DIR/rclaude.pid"))"
        else
            echo "RClaude is not running"
        fi
        ;;
    logs)
        if [ -f "$RCLAUDE_DIR/rclaude.log" ]; then
            tail -f "$RCLAUDE_DIR/rclaude.log"
        else
            echo "Log file not found."
        fi
        ;;
    run)
        unset CLAUDECODE
        unset CLAUDE_CODE_ENTRYPOINT
        dotnet "$APP_DIR/RClaude.dll" --contentRoot "$APP_DIR"
        ;;
    config)
        echo "Config: $APP_DIR/appsettings.json"
        cat "$APP_DIR/appsettings.json"
        echo ""
        echo "DB Settings:"
        if command -v sqlite3 &> /dev/null && [ -f "$RCLAUDE_DIR/rclaude.db" ]; then
            sqlite3 "$RCLAUDE_DIR/rclaude.db" "SELECT key, CASE WHEN key='telegram:bot_token' THEN substr(value,1,10)||'...' ELSE value END FROM settings;" 2>/dev/null || echo "(Cannot read DB)"
        else
            echo "(sqlite3 not found or DB file missing)"
        fi
        ;;
    *)
        echo "RClaude — Claude Code Agent via Telegram"
        echo ""
        echo "Usage:"
        echo "  rclaude start    — Start in background"
        echo "  rclaude stop     — Stop"
        echo "  rclaude restart  — Restart"
        echo "  rclaude status   — Check status"
        echo "  rclaude logs     — View logs (real-time)"
        echo "  rclaude run      — Start in foreground"
        echo "  rclaude config   — View settings"
        echo ""
        ;;
esac
LAUNCHER

chmod +x "$INSTALL_DIR/$BINARY_NAME"

# ─── Add to PATH ──────────────────────────────────
# Strategy 1: symlink to /usr/local/bin (works immediately, no source needed)
SYMLINK_CREATED=false
if ln -sf "$INSTALL_DIR/$BINARY_NAME" "/usr/local/bin/$BINARY_NAME" 2>/dev/null; then
    SYMLINK_CREATED=true
    echo -e "  ${GREEN}✓${NC} $MSG_PATH_ADDED: /usr/local/bin/$BINARY_NAME"
fi

# Strategy 2: add to shell rc as fallback
SHELL_RC=""
[ -f "$HOME/.zshrc" ] && SHELL_RC="$HOME/.zshrc"
[ -z "$SHELL_RC" ] && [ -f "$HOME/.bashrc" ] && SHELL_RC="$HOME/.bashrc"

PATH_LINE='export PATH="$HOME/.rclaude:$PATH"'
if [ -n "$SHELL_RC" ] && ! grep -q "\.rclaude" "$SHELL_RC" 2>/dev/null; then
    echo "" >> "$SHELL_RC"
    echo "# RClaude" >> "$SHELL_RC"
    echo "$PATH_LINE" >> "$SHELL_RC"
fi

# Strategy 3: export for current shell session
export PATH="$INSTALL_DIR:$PATH"
echo -e "  ${GREEN}✓${NC} $MSG_LAUNCHER_CREATED"

# ─── Done ─────────────────────────────────────────

echo ""
if [ "$MODE" = "upgrade" ]; then
    echo -e "${GREEN}═══════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}  $MSG_UPDATE_DONE${NC}"
    echo -e "${GREEN}═══════════════════════════════════════════════════${NC}"
    echo ""
    echo -e "  $MSG_SETTINGS_PRESERVED"
else
    echo -e "${GREEN}═══════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}  $MSG_INSTALL_DONE${NC}"
    echo -e "${GREEN}═══════════════════════════════════════════════════${NC}"
fi
echo ""
echo -e "  $MSG_INSTALL_LOC: ${CYAN}$INSTALL_DIR${NC}"
echo -e "  $MSG_DATABASE:        ${CYAN}$DB_PROVIDER${NC}"
echo ""
echo -e "  ${YELLOW}$MSG_START:${NC}"
echo -e "    rclaude start    — $MSG_START_BG"
echo -e "    rclaude run      — $MSG_START_FG"
echo ""
echo -e "  ${YELLOW}$MSG_MANAGE:${NC}"
echo -e "    rclaude stop     — $MSG_STOP"
echo -e "    rclaude restart  — $MSG_RESTART"
echo -e "    rclaude status   — $MSG_STATUS"
echo -e "    rclaude logs     — $MSG_LOGS"
echo -e "    rclaude config   — $MSG_CONFIG"
echo ""
echo -e "  ${YELLOW}$MSG_TG_COMMANDS:${NC}"
echo -e "    /newsession <name> — New session"
echo -e "    /setdir <path>     — Set folder"
echo -e "    /session           — Switch session"
echo -e "    /help              — All commands"
echo ""

if [ "$SYMLINK_CREATED" = false ] && [ -n "$SHELL_RC" ] && ! echo "$PATH" | grep -q ".rclaude"; then
    echo -e "  ${YELLOW}$MSG_NEW_TERMINAL${NC}"
    echo -e "    source $SHELL_RC"
    echo ""
fi
