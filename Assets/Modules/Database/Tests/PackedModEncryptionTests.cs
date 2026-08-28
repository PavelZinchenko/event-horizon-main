using System;
using System.IO;
using GameDatabase.Storage;
using NUnit.Framework;
using UnityEngine;

namespace GameDatabase.Tests
{
#if LICENSE_COMMERCIAL
    [TestFixture]
    public class PackedModEncryptionTests
    {
        private const string ExpectedName = "Authenticated test mod";
        private const string ExpectedId = "authenticated-test-mod";

        [Test]
        public void LoadsAuthenticatedModMetadata()
        {
            var storage = new FileDatabaseStorage(FixturePath("AuthenticatedModV1.bytes"));

            Assert.AreEqual(ExpectedName, storage.Name);
            Assert.AreEqual(ExpectedId, storage.Id);
            Assert.AreEqual(new GameDatabase.Storage.Version(1, 7), storage.Version);
        }

        [Test]
        public void RejectsUnsupportedAuthenticatedFormatVersion()
        {
            var data = ReadFixture("AuthenticatedModV1.bytes");
            WriteInt32(data, sizeof(uint), 2);

            AssertInvalid(data);
        }

        [TestCase(sizeof(uint) + sizeof(int))]
        [TestCase(sizeof(uint) + sizeof(int) + 384)]
        [TestCase(sizeof(uint) + sizeof(int) + 384 + 12)]
        [TestCase(sizeof(uint) + sizeof(int) + 384 + 12 + 16)]
        public void RejectsTamperedAuthenticatedEnvelope(int offset)
        {
            var data = ReadFixture("AuthenticatedModV1.bytes");
            data[offset] ^= 0x01;

            AssertInvalid(data);
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(8)]
        [TestCase(128)]
        [TestCase(423)]
        public void RejectsTruncatedAuthenticatedEnvelope(int length)
        {
            var fixture = ReadFixture("AuthenticatedModV1.bytes");
            var truncated = new byte[length];
            Buffer.BlockCopy(fixture, 0, truncated, 0, Math.Min(length, fixture.Length));

            AssertInvalid(truncated);
        }

        [Test]
        public void LoadsOldMagicLegacyModMetadata()
        {
            var storage = new FileDatabaseStorage(FixturePath("LegacyModV1.bytes"));

            Assert.AreEqual("Legacy test mod", storage.Name);
            Assert.AreEqual("legacy-test-mod", storage.Id);
            Assert.AreEqual(new GameDatabase.Storage.Version(1, 7), storage.Version);
        }

        [Test]
        public void LoadsHeaderlessLegacyModMetadata()
        {
            var storage = new FileDatabaseStorage(FixturePath("HeaderlessLegacyMod.bytes"));

            Assert.AreEqual("Headerless test mod", storage.Name);
            Assert.AreEqual("headerless-test-mod", storage.Id);
            Assert.AreEqual(new GameDatabase.Storage.Version(1, 0), storage.Version);
        }

        private static void AssertInvalid(byte[] data)
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(path, data);
                Assert.Throws<InvalidDataException>(() => new FileDatabaseStorage(path));
            }
            finally
            {
                File.Delete(path);
                Array.Clear(data, 0, data.Length);
            }
        }

        private static byte[] ReadFixture(string name)
        {
            return File.ReadAllBytes(FixturePath(name));
        }

        private static string FixturePath(string name)
        {
            return Path.Combine(
                Application.dataPath,
                "Modules",
                "Database",
                "Tests",
                "Fixtures",
                name);
        }

        private static void WriteInt32(byte[] destination, int offset, int value)
        {
            destination[offset] = (byte)value;
            destination[offset + 1] = (byte)(value >> 8);
            destination[offset + 2] = (byte)(value >> 16);
            destination[offset + 3] = (byte)(value >> 24);
        }
    }
#endif
}
