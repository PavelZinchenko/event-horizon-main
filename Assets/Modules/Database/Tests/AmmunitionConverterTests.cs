using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DatabaseMigration.v1;
using DatabaseMigration.v1.Enums;
using DatabaseMigration.v1.Serializable;
using GameDatabase.Storage;
using NUnit.Framework;

namespace GameDatabase.Tests
{
    // Battle simulator phase 4, step 4c (D2 plan, item 6). The fixture holds the 81 obsolete records of the built-in
    // database as they were before the conversion (2026-10-31), with their file names.
    [TestFixture]
    public class AmmunitionConverterTests
    {
        // Next free IDs of the built-in database before the conversion.
        private const int FirstAmmunitionId = 157;
        private const int FirstPrefabId = 22;
        private const int FirstEffectId = 25;

        private static readonly int[] ExistingPrefabs = { 2, 5, 6 };
        private static readonly int[] ExistingEffects = { 1, 2 };

        [Test]
        public void ConvertsEveryBuiltInRecord()
        {
            var output = ConvertAll();
            var ammunition = output.Select(item => item.Item).OfType<AmmunitionSerializable>().ToList();
            var prefabs = output.Select(item => item.Item).OfType<BulletPrefabSerializable>().ToList();
            var effects = output.Select(item => item.Item).OfType<VisualEffectSerializable>().ToList();

            var obsoleteIds = Fixture().Select(item => item.Item.Id).ToList();
            CollectionAssert.IsSubsetOf(obsoleteIds, ammunition.Select(item => item.Id).ToList(), "D3: a converted record keeps its ID");
            CollectionAssert.AllItemsAreUnique(ammunition.Select(item => item.Id).ToList());

            var children = ammunition.Select(item => item.Id).Where(id => !obsoleteIds.Contains(id)).ToList();
            CollectionAssert.AreEqual(Enumerable.Range(FirstAmmunitionId, children.Count), children);
            Assert.AreEqual(29, children.Count, "26 area bullets, 2 fragment copies and the flash of one copy");

            CollectionAssert.AreEqual(Enumerable.Range(FirstPrefabId, 14), prefabs.Select(item => item.Id));
            CollectionAssert.AreEqual(Enumerable.Range(FirstEffectId, 11), effects.Select(item => item.Id));
            CollectionAssert.AllItemsAreUnique(output.Select(item => item.Item.GetType().Name + "/" + item.Name).ToList());

            var ammunitionIds = new HashSet<int>(ammunition.Select(item => item.Id));
            var prefabIds = new HashSet<int>(prefabs.Select(item => item.Id).Concat(ExistingPrefabs));
            var effectIds = new HashSet<int>(effects.Select(item => item.Id).Concat(ExistingEffects));
            foreach (var item in ammunition)
            {
                Assert.IsTrue(item.Body.BulletPrefab == 0 || prefabIds.Contains(item.Body.BulletPrefab), $"ammunition {item.Id}: prefab");
                foreach (var trigger in item.Triggers)
                {
                    if (trigger.EffectType == BulletEffectType.SpawnBullet)
                        Assert.IsTrue(ammunitionIds.Contains(trigger.Ammunition), $"ammunition {item.Id}: spawned record {trigger.Ammunition}");
                    if (trigger.VisualEffect != 0)
                        Assert.IsTrue(effectIds.Contains(trigger.VisualEffect), $"ammunition {item.Id}: effect {trigger.VisualEffect}");
                }

                Assert.IsTrue(item.Body.Lifetime > 0 || item.Body.Velocity > 0 && item.Body.Range > 0, $"ammunition {item.Id}: no lifetime");
            }
        }

        [Test]
        public void SameInputGivesSameOutput()
        {
            var serializer = new NewtonJsonSerializer();
            var first = ConvertAll().Select(item => item.Name + serializer.ToJson(item.Item)).ToList();
            var second = ConvertAll().Select(item => item.Name + serializer.ToJson(item.Item)).ToList();
            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void CommonBulletGetsStrikeDamageAndPush()
        {
            // PlasmaCannon_S_1: heat 1, impulse 0.01, speed 20, prefab Energy1 (k 1.2).
            var item = Find(ConvertAll(), 48);
            Assert.AreEqual(BulletControllerType.Projectile, item.Controller.Type);
            Assert.AreEqual(0.6f, item.Body.Size);
            Assert.AreEqual(0f, item.Body.Lifetime);
            Assert.AreEqual(0.01f, item.Body.Weight);
            Assert.AreEqual(3, item.Effects.Length);
            AssertEffect(item.Effects[0], ImpactEffectType.Damage, DamageType.Heat, 1f);
            AssertEffect(item.Effects[1], ImpactEffectType.Damage, DamageType.Impact, 0.2f);
            AssertEffect(item.Effects[2], ImpactEffectType.Push, DamageType.Impact, 0.01f);

            var hit = item.Triggers.Single(trigger => trigger.Condition == BulletTriggerCondition.Hit);
            Assert.AreEqual(1, hit.VisualEffect);
            Assert.AreEqual(0.625f, hit.Size);
        }

        [Test]
        public void RocketSpawnsAreaBulletWithFalloff()
        {
            // MissileLauncher_L_2: heat 50, area 8, impulse 0.2.
            var output = ConvertAll();
            var item = Find(output, 38);
            Assert.AreEqual(BulletControllerType.Homing, item.Controller.Type);
            Assert.AreEqual(0.1f, item.Controller.StartingVelocityModifier);
            Assert.AreEqual(0.3f, item.Body.Size);
            Assert.AreEqual(3, item.Body.HitPoints);
            Assert.AreEqual(0, item.Effects.Length);

            var spawns = item.Triggers.Where(trigger => trigger.EffectType == BulletEffectType.SpawnBullet).ToList();
            CollectionAssert.AreEquivalent(new[] { BulletTriggerCondition.Detonated, BulletTriggerCondition.Expired }, spawns.Select(trigger => trigger.Condition));

            var area = Find(output, spawns[0].Ammunition);
            Assert.AreEqual(BulletImpactType.HitAllTargets, area.ImpactType);
            Assert.AreEqual(16f, area.Body.Size);
            Assert.AreEqual(0, area.Body.BulletPrefab);
            AssertEffect(area.Effects[0], ImpactEffectType.DamageWithFalloff, DamageType.Heat, 50f);
            AssertEffect(area.Effects[1], ImpactEffectType.PushFromCenterWithFalloff, DamageType.Impact, 1.6f);
            Assert.AreEqual(2, area.Triggers.Single().VisualEffect, "Explosion1");
        }

        [Test]
        public void EmpAreaDrainsShieldAndEnergy()
        {
            // EmpTorpedo_L_1: energy 20, area 7, energy cost 20.
            var output = ConvertAll();
            var item = Find(output, 13);
            var area = Find(output, item.Triggers.First(trigger => trigger.EffectType == BulletEffectType.SpawnBullet).Ammunition);
            Assert.AreEqual(3, area.Effects.Length);
            AssertEffect(area.Effects[0], ImpactEffectType.DamageWithFalloff, DamageType.Energy, 20f);
            AssertEffect(area.Effects[1], ImpactEffectType.DrainEnergy, DamageType.Impact, 200f);
            AssertEffect(area.Effects[2], ImpactEffectType.DrainShield, DamageType.Impact, 500f);
        }

        [Test]
        public void FragBombSpawnsOwnCopyOfCoupledRecord()
        {
            // FragBomb_M_1: impact 6, area 10, coupled PlasmaCannon_S_1 (48).
            var output = ConvertAll();
            var item = Find(output, 19);
            var spawns = item.Triggers.Where(trigger => trigger.EffectType == BulletEffectType.SpawnBullet).ToList();
            Assert.AreEqual(2, spawns.Count);
            Assert.IsTrue(spawns.All(trigger => trigger.Quantity == 20 && trigger.RandomFactor == 0.75f));
            Assert.IsTrue(spawns.All(trigger => trigger.ColorMode == ColorMode.UseMyOwn && trigger.Color == "#FFE2A7"));

            var fragment = Find(output, spawns[0].Ammunition);
            Assert.AreNotEqual(48, fragment.Id);
            Assert.AreEqual(10f, fragment.Body.Range);
            Assert.AreEqual(2, fragment.Effects.Length);
            AssertEffect(fragment.Effects[0], ImpactEffectType.Damage, DamageType.Impact, 6.2f);
        }

        [Test]
        public void ExplosionSpawnsStrongExplosionAtLaunch()
        {
            // AntiMatterBomb_L_1: heat 75, area 7, no impulse.
            var output = ConvertAll();
            var item = Find(output, 3);
            Assert.IsTrue(item.Body.AttachedToParent);
            Assert.AreEqual(0.5f, item.Body.Lifetime);
            Assert.AreEqual(0, item.Effects.Length);

            var spawn = item.Triggers.Single(trigger => trigger.EffectType == BulletEffectType.SpawnBullet);
            Assert.AreEqual(BulletTriggerCondition.Created, spawn.Condition);
            var area = Find(output, spawn.Ammunition);
            Assert.AreEqual(14f, area.Body.Size);
            AssertEffect(area.Effects.Single(), ImpactEffectType.DamageWithFalloff, DamageType.Heat, 75f);
        }

        [Test]
        public void BeamsKeepLifetimeAndTrack()
        {
            var output = ConvertAll();
            var siphon = Find(output, 16); // EnergySiphon_M
            Assert.AreEqual(BulletControllerType.Beam, siphon.Controller.Type);
            Assert.AreEqual(60f, siphon.Controller.TrackingAngle);
            Assert.AreEqual(0.05f, siphon.Body.Lifetime);
            Assert.AreEqual(6, siphon.Body.BulletPrefab);
            AssertEffect(siphon.Effects[1], ImpactEffectType.DrainEnergy, DamageType.Impact, 20f);

            var lightning = Find(output, 34); // LightningCannon_S_3 has a speed
            Assert.AreEqual(0.25f, lightning.Body.Lifetime);
            Assert.AreEqual(0f, lightning.Controller.TrackingAngle);
        }

        [Test]
        public void RejectsClassWithoutRule()
        {
            var converter = new AmmunitionConverter(FirstAmmunitionId, FirstPrefabId, FirstEffectId);
            var item = new AmmunitionObsoleteSerializable { Id = 1, AmmunitionClass = AmmunitionClassObsolete.BlackHole };
            Assert.Throws<InvalidOperationException>(() => converter.Convert(item, null, "BlackHole"));
        }

        // D2 plan, item 6: after the run the built-in database has no obsolete records, and no component points to one.
        [Test]
        public void BuiltInDatabaseHasNoObsoleteAmmunition()
        {
            var ids = new Dictionary<int, HashSet<int>>();
            var components = new List<(string File, int Ammunition)>();
            var serializer = new NewtonJsonSerializer();
            foreach (var path in Directory.GetFiles(DatabasePath(), "*.json", SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(path);
                SerializableItem item;
                try { item = serializer.FromJson<SerializableItem>(text); }
                catch (Exception) { continue; }
                if (item == null) continue;

                var type = (int)item.ItemType;
                if (!ids.TryGetValue(type, out var set)) ids.Add(type, set = new HashSet<int>());
                set.Add(item.Id);

                if (item.ItemType == ItemType.Component)
                {
                    var component = serializer.FromJson<ComponentSerializable>(text);
                    if (component.AmmunitionId != 0) components.Add((Path.GetFileName(path), component.AmmunitionId));
                }
            }

            Assert.IsFalse(ids.ContainsKey((int)ItemType.AmmunitionObsolete), "obsolete ammunition left");
            var ammunition = ids[(int)ItemType.Ammunition];
            foreach (var (file, id) in components)
                Assert.IsTrue(ammunition.Contains(id), $"{file}: ammunition {id} not found");
        }

        private static List<AmmunitionConverter.NewRecord> ConvertAll()
        {
            var fixture = Fixture();
            var byId = fixture.ToDictionary(item => item.Item.Id, item => item.Item);
            var converter = new AmmunitionConverter(FirstAmmunitionId, FirstPrefabId, FirstEffectId);
            var output = new List<AmmunitionConverter.NewRecord>();
            foreach (var (name, item) in fixture.OrderBy(item => item.Item.Id))
                output.AddRange(converter.Convert(item, item.CoupledAmmunitionId > 0 ? byId[item.CoupledAmmunitionId] : null, name));
            return output;
        }

        private static List<(string Name, AmmunitionObsoleteSerializable Item)> Fixture()
        {
            var text = File.ReadAllText(Path.Combine(TestsPath(), "Fixtures", "AmmunitionObsolete.json"));
            return new NewtonJsonSerializer().FromJson<FixtureItem[]>(text).Select(item => (item.Name, item.Record)).ToList();
        }

        private class FixtureItem
        {
            public string Name;
            public AmmunitionObsoleteSerializable Record;
        }

        private static AmmunitionSerializable Find(List<AmmunitionConverter.NewRecord> output, int id)
        {
            return output.Select(item => item.Item).OfType<AmmunitionSerializable>().Single(item => item.Id == id);
        }

        private static void AssertEffect(ImpactEffectSerializable effect, ImpactEffectType type, DamageType damageType, float power)
        {
            Assert.AreEqual(type, effect.Type);
            Assert.AreEqual(damageType, effect.DamageType);
            Assert.AreEqual(power, effect.Power);
        }

        private static string DatabasePath() => Path.Combine(TestsPath(), "..", "Resources", "Database");

        // The compiler records this file's path, so the files are found without UnityEngine.Application.
        private static string TestsPath([CallerFilePath] string path = "") => Path.GetDirectoryName(path);
    }
}
