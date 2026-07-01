using System;

namespace CTG2.Content.Commands
{
    internal static class CommandInputParser
    {
        public static bool TryParseTargetAndOptionalReason(
            string rawInput,
            string commandName,
            string[] fallbackArgs,
            out string targetName,
            out string reason,
            out string error)
        {
            targetName = string.Empty;
            reason = "none";
            error = string.Empty;

            string message = ExtractCommandArguments(rawInput, commandName, fallbackArgs);
            if (string.IsNullOrWhiteSpace(message))
            {
                error = $"Usage: /{commandName} <playerName> [\"reason\"]";
                return false;
            }

            int index = 0;
            if (!TryReadArgument(message, ref index, "player name", out targetName, out error))
                return false;

            SkipWhitespace(message, ref index);
            if (index >= message.Length)
                return true;

            if (message[index] == '"')
            {
                if (!TryReadArgument(message, ref index, "reason", out reason, out error))
                    return false;

                SkipWhitespace(message, ref index);
                if (index < message.Length)
                {
                    error = "Unexpected text after reason.";
                    return false;
                }
            }
            else
            {
                reason = message.Substring(index).Trim();
            }

            if (string.IsNullOrWhiteSpace(reason))
                reason = "none";

            return true;
        }

        private static string ExtractCommandArguments(string rawInput, string commandName, string[] fallbackArgs)
        {
            if (!string.IsNullOrWhiteSpace(rawInput))
            {
                string trimmedInput = rawInput.Trim();
                string slashCommand = "/" + commandName;

                if (trimmedInput.StartsWith(slashCommand, StringComparison.OrdinalIgnoreCase))
                    return trimmedInput.Substring(slashCommand.Length).Trim();
            }

            return fallbackArgs == null ? string.Empty : string.Join(" ", fallbackArgs);
        }

        private static bool TryReadArgument(
            string message,
            ref int index,
            string argumentName,
            out string value,
            out string error)
        {
            value = string.Empty;
            error = string.Empty;

            SkipWhitespace(message, ref index);
            if (index >= message.Length)
            {
                error = $"Missing {argumentName}.";
                return false;
            }

            if (message[index] != '"')
            {
                int start = index;
                while (index < message.Length && !char.IsWhiteSpace(message[index]))
                    index++;

                value = message.Substring(start, index - start);
                return true;
            }

            index++;
            int quotedStart = index;
            while (index < message.Length && message[index] != '"')
                index++;

            if (index >= message.Length)
            {
                error = $"Missing closing quote for {argumentName}.";
                return false;
            }

            value = message.Substring(quotedStart, index - quotedStart);
            index++;
            return true;
        }

        private static void SkipWhitespace(string message, ref int index)
        {
            while (index < message.Length && char.IsWhiteSpace(message[index]))
                index++;
        }
    }
}
