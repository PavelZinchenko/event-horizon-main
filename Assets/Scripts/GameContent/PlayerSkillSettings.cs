using System;
using System.Collections.Generic;
using UnityEngine;
using GameModel.Skills;

namespace GameContent
{
    [CreateAssetMenu(fileName = "PlayerSkillSettings", menuName = "ScriptableObjects/PlayerSkillSettings", order = 1)]
    public class PlayerSkillSettings : ScriptableObject
    {
        [SerializeField] private SkillData[] _skills = { };
        [SerializeField] private Sprite _default;

        private Dictionary<SkillType, Sprite> _spriteTable = new();

        public Sprite SkillIcon(SkillType type)
        {
            if (_spriteTable.Count == 0)
            {
                if (_skills.Length == 0) 
                    return _default;

                foreach (var skill in _skills)
                    _spriteTable.Add(skill.Type, skill.Icon);
            }

            return _spriteTable.TryGetValue(type, out var sprite) ? sprite : _default;
        }

        [Serializable]
        private struct SkillData
        {
            public SkillType Type;
            public Sprite Icon;
        }
    }
}
