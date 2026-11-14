using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Combat.Integration.Core
{
    // Phase 7, step 9: a hash of the source code that decides a duel. Two builds of one game version with different
    // combat code refuse each other (MatchIdentity.SourceHash, HandshakeRejection.Source). It hashes source, not
    // binaries, so a Mono and an IL2CPP build of one source agree. Comments and whitespace do not count.
    // A build reads the value from a Resources text file that CoreSourceHashBuildStep writes before each build; the
    // editor computes it from the files once per domain reload.
    public static class CoreSourceHash
    {
        public const string ResourceName = "CoreSourceHash";

        // Relative to the project folder: every .cs file below them, without Tests folders.
        public static readonly string[] Folders =
        {
            "Assets/ModulesShared/CombatSimulation",
            "Assets/Scripts/Combat/Integration/Core",
        };

        // Empty in a build without the hash file; it then matches no other build.
        public static byte[] Value => _value ?? (_value = Load());

        public static byte[] Compute(string projectPath)
        {
            var files = new List<string>();
            foreach (var folder in Folders)
            {
                foreach (var file in Directory.GetFiles(Path.Combine(projectPath, folder), "*.cs", SearchOption.AllDirectories))
                {
                    var relative = file.Substring(projectPath.Length).TrimStart('/', '\\').Replace('\\', '/');
                    if (relative.Contains("/Tests/")) continue;
                    files.Add(relative);
                }
            }
            files.Sort(string.CompareOrdinal);

            using (var sha = SHA256.Create())
            {
                foreach (var file in files)
                {
                    var bytes = Encoding.UTF8.GetBytes(file + "\n" + StripCode(File.ReadAllText(Path.Combine(projectPath, file))) + "\n");
                    sha.TransformBlock(bytes, 0, bytes.Length, null, 0);
                }
                sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return sha.Hash;
            }
        }

        // Removes comments and whitespace outside string and character literals. The scan is simple (an interpolated
        // string with nested quotes may end early), but it is the same on every machine, so the hash is too.
        private static string StripCode(string text)
        {
            var result = new StringBuilder(text.Length);
            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                var next = i + 1 < text.Length ? text[i + 1] : '\0';
                if (c == '/' && next == '/')
                {
                    while (i < text.Length && text[i] != '\n') ++i;
                    continue;
                }
                if (c == '/' && next == '*')
                {
                    var end = text.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    if (end < 0) break;
                    i = end + 1;
                    continue;
                }
                if (c == '"' || c == '\'')
                {
                    var verbatim = c == '"' && i > 0 && text[i - 1] == '@';
                    result.Append(c);
                    for (++i; i < text.Length; ++i)
                    {
                        result.Append(text[i]);
                        if (!verbatim && text[i] == '\\' && i + 1 < text.Length)
                        {
                            result.Append(text[++i]);
                            continue;
                        }
                        if (text[i] != c) continue;
                        if (verbatim && i + 1 < text.Length && text[i + 1] == '"')
                        {
                            result.Append(text[++i]);
                            continue;
                        }
                        break;
                    }
                    continue;
                }
                if (!char.IsWhiteSpace(c)) result.Append(c);
            }
            return result.ToString();
        }

        private static byte[] Load()
        {
#if UNITY_EDITOR
            return Compute(Path.GetDirectoryName(Application.dataPath));
#else
            var asset = Resources.Load<TextAsset>(ResourceName);
            if (asset == null)
            {
                Debug.LogError("CoreSourceHash: the build has no source hash; online duels will refuse it.");
                return Array.Empty<byte>();
            }
            return Convert.FromBase64String(asset.text.Trim());
#endif
        }

        private static byte[] _value;
    }
}
