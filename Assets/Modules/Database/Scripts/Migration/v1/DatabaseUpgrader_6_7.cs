using DatabaseMigration.v1.Enums;
using DatabaseMigration.v1.Serializable;
using System.Collections.Generic;
using UnityEngine;

namespace DatabaseMigration.v1
{
    public partial class DatabaseUpgrader
    {
		partial void Migrate_6_7()
		{
            GameDiagnostics.Trace.LogWarning("Database migration: v1.6 -> v1.7");

            var deviceIds = new Dictionary<int, int>();
            for (int i = 0; i < Content.DeviceList.Count; ++i)
                deviceIds.Add(Content.DeviceList[i].Id, i);

            var tags = new ComponentGroupTags();
            foreach (var component in Content.ComponentList)
            {
                if (!string.IsNullOrEmpty(component.Restrictions.UniqueComponentTag))
                {
                    var id = tags.Create(component.Restrictions.UniqueComponentTag, component.Restrictions.MaxComponentAmount);
                    component.Restrictions.ComponentGroupTag = id;
                    component.Restrictions.UniqueComponentTag = null;
                }
                else if (component.DeviceId != 0)
                {
                    if (!deviceIds.TryGetValue(component.DeviceId, out var deviceIndex))
                    {
                        GameDiagnostics.Trace.LogError($"Unknown device ID {component.DeviceId} in {component.FileName}");
                        continue;
                    }

                    var device = Content.DeviceList[deviceIndex];
                    int tag = 0;
                    int maxComponents = component.Restrictions.MaxComponentAmount;

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
                            tag = tags.Create(device.DeviceClass.ToString(), maxComponents);
                            break;
                        case DeviceClass.Stealth:
                        case DeviceClass.SuperStealth:
                            tag = tags.Create(DeviceClass.Stealth.ToString(), maxComponents);
                            break;
                        case DeviceClass.EnergyShield:
                        case DeviceClass.PartialShield:
                            tag = tags.Create(DeviceClass.EnergyShield.ToString(), maxComponents);
                            break;
                        case DeviceClass.WormTail:
                            tag = tags.Create(DeviceClass.WormTail.ToString(), maxComponents);
                            break;
                    }

                    if (tag > 0) component.Restrictions.ComponentGroupTag = tag;
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
                    data = new(_tags.Count + 1, Mathf.Max(1, maxComponents));
                else if (data.maxCount < maxComponents)
                    data.maxCount = maxComponents;

                _tags[key] = data;
                return data.index;
            }

            public IEnumerable<ComponentGroupTagSerializable> Serialize()
            {
                foreach (var tag in _tags.Values)
                {
                    yield return new ComponentGroupTagSerializable
                    {
                        Id = tag.index,
                        MaxInstallableComponents = tag.maxCount,
                    };
                }
            }
        }
    }
}
