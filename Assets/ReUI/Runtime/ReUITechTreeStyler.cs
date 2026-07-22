using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUITechTreeStyler
    {
        private const string SceneName = "StarMapScene";
        private const int StarshipEarthFactionId = 21;
        private const int TrisolarisFactionId = 22;

        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null || canvas.gameObject.scene.name != SceneName) return;

            // Technology entries and faction selectors encode gameplay state through
            // their native sprites, colors and Toggle state. ReUI must never replace
            // those graphics. Repair any artifacts created by an earlier generic scan.
            RestoreGeneratedIcons(canvas.transform);
            RestoreNativeViewModels(canvas.transform);
        }

        private static void RestoreGeneratedIcons(Transform root)
        {
            ReUIIconGraphic[] generated = root.GetComponentsInChildren<ReUIIconGraphic>(true);
            for (int i = 0; i < generated.Length; i++)
            {
                ReUIIconGraphic graphic = generated[i];
                if (graphic == null || !IsTechnologyPanelContext(graphic.transform)) continue;

                Transform host = graphic.transform.parent;
                if (host != null)
                {
                    Image hostImage = host.GetComponent<Image>();
                    if (hostImage != null)
                    {
                        hostImage.enabled = true;
                        hostImage.material = null;
                    }
                }

                graphic.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(graphic.gameObject);
            }
        }

        private static void RestoreNativeViewModels(Transform root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null) continue;

                string typeName = behaviour.GetType().FullName;
                if (typeName == "ViewModel.TechItemViewModel")
                    RestoreTechItem(behaviour);
                else if (typeName == "ViewModel.FactionViewModel")
                    RestoreFactionItem(behaviour);
            }
        }

        private static void RestoreTechItem(MonoBehaviour viewModel)
        {
            bool hadArtifacts = RemoveReUIArtifacts(viewModel.transform);
            EnableNativeImages(viewModel.transform);
            if (!hadArtifacts) return;

            FieldInfo technologyField = viewModel.GetType().GetField("_technology", InstanceFlags);
            MethodInfo initialize = viewModel.GetType().GetMethod("Initialize", InstanceFlags);
            object technology = technologyField?.GetValue(viewModel);
            if (technology != null && initialize != null)
            {
                try
                {
                    initialize.Invoke(viewModel, new[] { technology });
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[ReUI] Failed to restore technology item '{viewModel.name}': {exception.GetBaseException().Message}");
                }
            }
        }

        private static void RestoreFactionItem(MonoBehaviour viewModel)
        {
            bool hadArtifacts = RemoveReUIArtifacts(viewModel.transform);
            EnableNativeImages(viewModel.transform);

            FieldInfo factionField = viewModel.GetType().GetField("_faction", InstanceFlags);
            object faction = factionField?.GetValue(viewModel);
            int factionId = ReadFactionId(faction);

            if (hadArtifacts && faction != null)
            {
                MethodInfo setFaction = viewModel.GetType().GetMethod("SetFaction", InstanceFlags);
                if (setFaction != null)
                {
                    try
                    {
                        setFaction.Invoke(viewModel, new[] { faction });
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning($"[ReUI] Failed to restore faction item '{viewModel.name}': {exception.GetBaseException().Message}");
                    }
                }
            }

            if (factionId != StarshipEarthFactionId && factionId != TrisolarisFactionId) return;

            Toggle toggle = GetFieldValue<Toggle>(viewModel, "Toggle");
            if (toggle != null)
            {
                // The Three-Body technology database is bundled and should always be
                // inspectable. Its discovery flag can be absent in old Beta3 saves,
                // which previously left this Toggle permanently disabled.
                toggle.interactable = true;
            }

            Text label = GetFieldValue<Text>(viewModel, "Name");
            if (label != null && (string.IsNullOrWhiteSpace(label.text) || label.text.Contains("?")))
                label.text = factionId == StarshipEarthFactionId ? "星舰地球" : "三体";
        }

        private static bool RemoveReUIArtifacts(Transform root)
        {
            bool changed = false;

            ReUIButtonMotion[] motions = root.GetComponentsInChildren<ReUIButtonMotion>(true);
            for (int i = 0; i < motions.Length; i++)
            {
                if (motions[i] == null) continue;
                motions[i].enabled = false;
                UnityEngine.Object.Destroy(motions[i]);
                changed = true;
            }

            ReUIStyledElement[] markers = root.GetComponentsInChildren<ReUIStyledElement>(true);
            for (int i = 0; i < markers.Length; i++)
            {
                if (markers[i] == null) continue;
                UnityEngine.Object.Destroy(markers[i]);
                changed = true;
            }

            Outline[] outlines = root.GetComponentsInChildren<Outline>(true);
            for (int i = 0; i < outlines.Length; i++)
            {
                // Preserve original outlines. Only disable outlines attached to an
                // object that also carried a ReUI marker or motion component.
                GameObject target = outlines[i].gameObject;
                if (target.GetComponent<ReUIStyledElement>() == null &&
                    target.GetComponent<ReUIButtonMotion>() == null)
                    continue;
                outlines[i].enabled = false;
                changed = true;
            }

            return changed;
        }

        private static void EnableNativeImages(Transform root)
        {
            Image[] images = root.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                string name = image.name.ToLowerInvariant();
                if (name == "icon" || name == "image")
                {
                    image.enabled = true;
                    image.material = null;
                }
            }
        }

        private static bool IsTechnologyPanelContext(Transform transform)
        {
            Transform current = transform;
            int depth = 0;
            while (current != null && depth++ < 16)
            {
                string name = current.name.ToLowerInvariant();
                if (name == "researchpanel" || name == "techtree" ||
                    name == "techtreepanel" || name == "techitem" ||
                    name == "factions")
                    return true;

                MonoBehaviour[] behaviours = current.GetComponents<MonoBehaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    string typeName = behaviours[i] != null ? behaviours[i].GetType().FullName : null;
                    if (typeName == "ViewModel.TechItemViewModel" ||
                        typeName == "ViewModel.TechTreePanelViewModel" ||
                        typeName == "ViewModel.FactionViewModel" ||
                        typeName == "ViewModel.ResearchPanelViewModel")
                        return true;
                }

                current = current.parent;
            }
            return false;
        }

        private static int ReadFactionId(object faction)
        {
            if (faction == null) return -1;
            Type factionType = faction.GetType();
            object id = factionType.GetProperty("Id", InstanceFlags)?.GetValue(faction) ??
                        factionType.GetField("Id", InstanceFlags)?.GetValue(faction);
            if (id == null) return -1;

            Type idType = id.GetType();
            object value = idType.GetProperty("Value", InstanceFlags)?.GetValue(id) ??
                           idType.GetField("Value", InstanceFlags)?.GetValue(id);
            if (value == null) return -1;

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return -1;
            }
        }

        private static T GetFieldValue<T>(object target, string fieldName) where T : class
        {
            if (target == null) return null;
            return target.GetType().GetField(fieldName, InstanceFlags)?.GetValue(target) as T;
        }
    }
}
