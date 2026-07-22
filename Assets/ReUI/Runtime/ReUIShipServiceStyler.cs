using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    internal static class ReUIShipServiceStyler
    {
        private const string ShipyardType = "Gui.ShipService.ShipyardWindow";
        private const string ModificationsType = "Gui.Craft.ModificationsPanel";

        internal static void Apply(Canvas canvas)
        {
            if (canvas == null) return;

            MonoBehaviour[] behaviours = canvas.GetComponentsInChildren<MonoBehaviour>(true);
            Transform shipyard = null;
            bool hasModificationsPanel = false;
            for (int i = 0; i < behaviours.Length; ++i)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null) continue;
                string typeName = behaviour.GetType().FullName;
                if (typeName == ShipyardType) shipyard = behaviour.transform;
                else if (typeName == ModificationsType) hasModificationsPanel = true;
            }

            if (shipyard == null || !hasModificationsPanel) return;

            Button exit = FindButton(shipyard, "ExitButton");
            if (exit == null || shipyard is not RectTransform windowRect ||
                exit.transform is not RectTransform exitRect)
                return;

#if UNITY_EDITOR
            // Scene-validation opens authored prefab instances directly. Unity
            // correctly rejects hierarchy edits on those instances, while the
            // runtime-created shipyard remains safe to reposition in builds.
            if (!Application.isPlaying && UnityEditor.PrefabUtility.IsPartOfPrefabInstance(exit.gameObject))
            {
                ReUISpecializedVisuals.StyleGlassButton(exit, ReUIIconKind.Close, true, false, 0.74f);
                return;
            }
#endif

            // The prefab places this global close button inside the left control strip,
            // where it overlaps modification entries on narrow Android screens. Make it
            // an independent top-right child of the shipyard window without changing its
            // original onClick event.
            if (exitRect.parent != windowRect)
                exitRect.SetParent(windowRect, false);
            exitRect.anchorMin = Vector2.one;
            exitRect.anchorMax = Vector2.one;
            exitRect.pivot = Vector2.one;
            exitRect.sizeDelta = new Vector2(72f, 72f);
            exitRect.anchoredPosition = new Vector2(-20f, -20f);
            exitRect.localScale = Vector3.one;
            exitRect.SetAsLastSibling();

            LayoutElement layout = exit.GetComponent<LayoutElement>();
            if (layout != null) layout.ignoreLayout = true;
            ReUISpecializedVisuals.StyleGlassButton(exit, ReUIIconKind.Close, true, false, 0.74f);
        }

        private static Button FindButton(Transform root, string name)
        {
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; ++i)
                if (buttons[i].name == name)
                    return buttons[i];
            return null;
        }
    }
}
