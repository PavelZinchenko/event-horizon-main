using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace GameServices.Captains
{
    public enum CaptainId
    {
        ZhangBeihai,
        ChuYan,
    }

    public sealed class CaptainDefinition
    {
        public CaptainDefinition(CaptainId id, string name, string skillName, string description, string portraitPath)
        {
            Id = id;
            Name = name;
            SkillName = skillName;
            Description = description;
            PortraitPath = portraitPath;
        }

        public CaptainId Id { get; }
        public string Name { get; }
        public string SkillName { get; }
        public string Description { get; }
        public string PortraitPath { get; }
    }

    /// <summary>
    /// Stores the chosen captain independently from a save slot.  Captain
    /// battle effects are reset by each combat scene; only the choice persists.
    /// </summary>
    public sealed class CaptainService : IInitializable
    {
        public CaptainId Selected { get; private set; } = CaptainId.ZhangBeihai;
        public event Action<CaptainId> Changed;

        public void Initialize()
        {
            var stored = PlayerPrefs.GetInt(SelectedCaptainKey, (int)CaptainId.ZhangBeihai);
            Selected = Enum.IsDefined(typeof(CaptainId), stored)
                ? (CaptainId)stored
                : CaptainId.ZhangBeihai;
        }

        public void Select(CaptainId captain)
        {
            if (!Enum.IsDefined(typeof(CaptainId), captain) || Selected == captain)
                return;

            Selected = captain;
            PlayerPrefs.SetInt(SelectedCaptainKey, (int)captain);
            PlayerPrefs.Save();
            Changed?.Invoke(captain);
        }

        public CaptainDefinition Get(CaptainId captain)
        {
            for (var i = 0; i < Definitions.Count; i++)
                if (Definitions[i].Id == captain)
                    return Definitions[i];
            return Definitions[0];
        }

        public static IReadOnlyList<CaptainDefinition> Definitions { get; } = new[]
        {
            new CaptainDefinition(
                CaptainId.ZhangBeihai,
                "章北海",
                "走为上策",
                "更换飞船操作瞬间完成。每场战斗限一次：第一艘玩家操控的飞船被击毁时，保留其 10% 装甲并立即进入选择飞船页面。",
                "Textures/Captains/ZhangBeihai"),
            new CaptainDefinition(
                CaptainId.ChuYan,
                "褚岩",
                "黑暗战役",
                "每有一名友军舰船被击毁，当前舰船回复 15% 装甲和 50% 能量；其本局武器伤害提高 20%，最多提高 200%。",
                "Textures/Captains/ChuYan"),
        };

        private const string SelectedCaptainKey = "ThreeBody.SelectedCaptain";
    }
}
