#!/bin/bash
# Auto-format C# files after Claude edits them
INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

if [ -z "$FILE_PATH" ]; then
  exit 0
fi

# Only format .cs files
if [[ "$FILE_PATH" == *.cs ]]; then
  PROJECT_DIR="${CLAUDE_PROJECT_DIR:-$(pwd)}"

  # Normalize both paths to forward slashes (Windows compatibility)
  NORM_PROJECT=$(echo "$PROJECT_DIR" | sed 's|\\|/|g')
  NORM_FILE=$(echo "$FILE_PATH" | sed 's|\\|/|g')

  # Strip project root prefix using bash parameter expansion (safe, no regex)
  RELATIVE_PATH="${NORM_FILE#"$NORM_PROJECT"/}"

  cd "$PROJECT_DIR" && dotnet format --include "$RELATIVE_PATH" --no-restore 2>/dev/null || true
fi

exit 0
