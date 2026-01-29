using System;
using System.Collections.Generic;

namespace Common.Can
{
    public sealed class CliOptions
    {
        private readonly Dictionary<string, string> _values;
        private readonly HashSet<string> _flags;

        private CliOptions(Dictionary<string, string> values, HashSet<string> flags)
        {
            _values = values;
            _flags = flags;
        }

        public IReadOnlyDictionary<string, string> Values => _values;

        public IReadOnlyCollection<string> Flags => _flags;

        public string GetValue(string key, string defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return defaultValue;
            }

            var normalized = NormalizeKey(key);
            return _values.TryGetValue(normalized, out var value) ? value : defaultValue;
        }

        public bool GetFlag(string key, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return defaultValue;
            }

            var normalized = NormalizeKey(key);
            return _flags.Contains(normalized) || defaultValue;
        }

        public static CliOptions Parse(
            string[] args,
            IDictionary<string, string> defaultValues = null,
            IEnumerable<string> defaultFlags = null)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (defaultValues != null)
            {
                foreach (var kvp in defaultValues)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        continue;
                    }

                    values[NormalizeKey(kvp.Key)] = kvp.Value;
                }
            }

            if (defaultFlags != null)
            {
                foreach (var flag in defaultFlags)
                {
                    if (string.IsNullOrWhiteSpace(flag))
                    {
                        continue;
                    }

                    flags.Add(NormalizeKey(flag));
                }
            }

            if (args == null)
            {
                return new CliOptions(values, flags);
            }

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (string.IsNullOrWhiteSpace(arg))
                {
                    continue;
                }

                if (!arg.StartsWith("--", StringComparison.Ordinal))
                {
                    continue;
                }

                var trimmed = arg.Substring(2);
                if (trimmed.Length == 0)
                {
                    continue;
                }

                var equalsIndex = trimmed.IndexOf('=');
                if (equalsIndex >= 0)
                {
                    var key = trimmed.Substring(0, equalsIndex);
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    var value = trimmed.Substring(equalsIndex + 1);
                    values[NormalizeKey(key)] = value;
                    continue;
                }

                var next = i + 1 < args.Length ? args[i + 1] : null;
                if (!string.IsNullOrWhiteSpace(next) && !next.StartsWith("--", StringComparison.Ordinal))
                {
                    values[NormalizeKey(trimmed)] = next;
                    i++;
                    continue;
                }

                flags.Add(NormalizeKey(trimmed));
            }

            return new CliOptions(values, flags);
        }

        private static string NormalizeKey(string key)
        {
            return key.Trim().TrimStart('-').ToLowerInvariant();
        }
    }
}
