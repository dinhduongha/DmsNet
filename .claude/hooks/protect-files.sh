#!/bin/bash
# Prevent Claude from modifying critical config and generated files
INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

if [ -z "$FILE_PATH" ]; then
  exit 0
fi

# Normalize: get basename and convert backslashes to forward slashes
NORMALIZED=$(echo "$FILE_PATH" | sed 's|\\|/|g')
BASENAME=$(basename "$NORMALIZED")

# Protected by exact basename
PROTECTED_BASENAMES=(
  "common.props"
  "NuGet.Config"
  "launchSettings.json"
  ".editorconfig"
  ".claudeignore"
  ".mcp.json"
)

for name in "${PROTECTED_BASENAMES[@]}"; do
  if [[ "$BASENAME" == "$name" ]]; then
    echo "Blocked: '$BASENAME' is a protected file. Ask user before modifying." >&2
    exit 2
  fi
done

# Protected by extension
PROTECTED_EXTENSIONS=(
  "csproj"
  "slnx"
  "sln"
  "pfx"
)

for ext in "${PROTECTED_EXTENSIONS[@]}"; do
  if [[ "$BASENAME" == *."$ext" ]]; then
    echo "Blocked: '$BASENAME' (.$ext) is a protected file type. Ask user before modifying." >&2
    exit 2
  fi
done

# Protected by path pattern (check if path contains these segments)
if [[ "$NORMALIZED" == */Migrations/*.cs ]]; then
  echo "Blocked: Migration files are auto-generated. Ask user before modifying." >&2
  exit 2
fi

if [[ "$NORMALIZED" == */.github/workflows/* ]]; then
  echo "Blocked: CI/CD workflows are protected. Ask user before modifying." >&2
  exit 2
fi

if [[ "$NORMALIZED" == */.claude/settings.json ]]; then
  echo "Blocked: Claude settings is protected. Ask user before modifying." >&2
  exit 2
fi

if [[ "$BASENAME" == appsettings.*.json ]]; then
  echo "Blocked: Environment-specific config files are protected. Ask user before modifying." >&2
  exit 2
fi

exit 0
