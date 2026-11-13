using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using GameDatabase.Model;

namespace GameDatabase.Storage
{
    // The database hash of the online duel (battle simulator phase 7, step 4): both players must load the same records.
    // SHA-256 over the JSON records of the loaded storages: the mod, then the built-in catalog unless the mod unloads it.
    // Each record counts by its own digest and the digests are sorted, so file order does not count. Carriage returns
    // are removed, so line endings do not count. Images, audio and localization do not change a battle and do not count.
    public static class DatabaseContentHash
    {
        public const int Length = 32;

        public static byte[] Compute(IDataStorage mod, IDataStorage builtIn)
        {
            using var sha = SHA256.Create();
            var total = new List<byte>();
            Append(total, sha, mod, 1);
            Append(total, sha, builtIn, 2);
            return sha.ComputeHash(total.ToArray());
        }

        // A tag per storage, the record count, then the sorted record digests. A missing storage adds nothing.
        private static void Append(List<byte> total, SHA256 sha, IDataStorage storage, byte tag)
        {
            if (storage == null) return;
            var loader = new Loader(sha);
            storage.LoadContent(loader);
            loader.Digests.Sort(Compare);
            total.Add(tag);
            total.AddRange(BitConverter.GetBytes(loader.Digests.Count));
            foreach (var digest in loader.Digests) total.AddRange(digest);
        }

        private static int Compare(byte[] a, byte[] b)
        {
            for (var i = 0; i < a.Length; ++i)
                if (a[i] != b[i]) return a[i].CompareTo(b[i]);
            return 0;
        }

        private sealed class Loader : IContentLoader
        {
            internal readonly List<byte[]> Digests = new List<byte[]>();

            internal Loader(SHA256 sha) => _sha = sha;

            public void LoadJson(string name, string data) =>
                Digests.Add(_sha.ComputeHash(Encoding.UTF8.GetBytes(data.Replace("\r", string.Empty))));

            public void LoadLocalization(string name, string data) { }
            public void LoadImage(string name, IImageData image) { }
            public void LoadAudioClip(string name, IAudioClipData audioClip) { }

            private readonly SHA256 _sha;
        }
    }
}
