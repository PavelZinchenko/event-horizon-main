using DatabaseMigration.v1.Enums;
using DatabaseMigration.v1.Serializable;
using System;

namespace DatabaseMigration.v1
{
	public partial class DatabaseUpgrader
	{
		partial void Migrate_6_7()
		{
			Console.WriteLine("Database migration: v1.6 -> v1.7");

            var deviceIds = new Dictionary<int, int>();
            for (int i = 0; i < Content.DeviceList.Count; ++i)
                deviceIds.Add(Content.DeviceList[i].Id, i);

            var tags = new ComponentGroupTags();
            foreach (var component in Content.ComponentList)
            {
                var uniqueKey = component.Restrictions?.UniqueComponentTag;
                var maxComponents = component.Restrictions != null ? component.Restrictions.MaxComponentAmount : 0;

                if (!string.IsNullOrEmpty(uniqueKey))
                {
                    var id = tags.Create(uniqueKey, maxComponents);
                    component.Restrictions.ComponentGroupTag = id;
                    component.Restrictions.UniqueComponentTag = null;
                }
                else if (component.DeviceId != 0)
                {
                    if (!deviceIds.TryGetValue(component.DeviceId, out var deviceIndex))
                        throw new DatabaseException($"Unknown device ID {component.DeviceId} in {component.FileName}");

                    var device = Content.DeviceList[deviceIndex];
                    int maxCount = component.Restrictions?.MaxComponentAmount ?? 0;
                    int tag = 0;

                    switch (device.DeviceClass)
                    {
                        case DeviceClass.Teleporter:
                        case DeviceClass.Fortification:
                        case DeviceClass.Brake:
                        case DeviceClass.RepairBot:
                        case DeviceClass.PointDefense:
                        case DeviceClass.GravityGenerator:
                        case DeviceClass.Ghost:
                        case DeviceClass.Decoy:
                        case DeviceClass.Detonator:
                        case DeviceClass.Accelerator:
                        case DeviceClass.ToxicWaste:
                            tag = tags.Create(device.DeviceClass.ToString(), maxCount);
                            break;
                        case DeviceClass.Stealth:
                        case DeviceClass.SuperStealth:
                            tag = tags.Create(DeviceClass.Stealth.ToString(), maxCount);
                            break;
                        case DeviceClass.EnergyShield:
                        case DeviceClass.PartialShield:
                            tag = tags.Create(DeviceClass.EnergyShield.ToString(), maxCount);
                            break;
                        case DeviceClass.WormTail:
                            tag = tags.Create(DeviceClass.WormTail.ToString(), maxCount);
                            break;
                    }

                    if (tag > 0) (component.Restrictions ??= new()).ComponentGroupTag = tag;
                }
            }

            Content.ComponentGroupTagList.AddRange(tags.Serialize());
        }

        private class ComponentGroupTags
        {
            private readonly Dictionary<string, (int index, int maxCount)> _tags = new();

            public int Create(string key, int maxComponents)
            {
                if (!_tags.TryGetValue(key, out var data))
                    data = new(_tags.Count + 1, Math.Max(1, maxComponents));
                else if (data.maxCount < maxComponents)
                    data.maxCount = maxComponents;

                _tags[key] = data;
                return data.index;
            }

            public IEnumerable<ComponentGroupTagSerializable> Serialize()
            {
                foreach (var tag in _tags)
                {
                    yield return new ComponentGroupTagSerializable
                    {

                        Id = tag.Value.index,
                        FileName = $"Tag_{tag.Value.index}_{RemoveInvalidChars(tag.Key)}.json",
                        MaxInstallableComponents = tag.Value.maxCount,
                    };
                }
            }

            private static string RemoveInvalidChars(string filename)
            {
                return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
            }
        }
    }
}
