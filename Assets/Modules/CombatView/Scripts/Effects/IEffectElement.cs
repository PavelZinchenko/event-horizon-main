using UnityEngine;

namespace CombatView.Effects
{
    // An effect prefab that can play a record element (migration phase 5b, step 3). Replaces the legacy
    // IEffectPrefabInitializer and IEffectInstanceInitializer, which took database types.
    public interface IEffectElement
    {
        // Once on the template of each element type and image, as legacy PrefabCache.GetEffectPrefab. Write only
        // serialized state here: Unity copies only that into the instances.
        void Configure(EffectKind kind, Sprite image);

        // On each instance before its Initialize: values that differ between the elements of one template.
        void Setup(EffectElement element);
    }
}
