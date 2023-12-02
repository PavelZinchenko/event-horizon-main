using System.Text;
using UnityEngine.InputSystem;
using UnityEngine;
using CommonComponents.Utils;
using System;

namespace Services.Input
{
    public struct Configuration
    {
        public static Configuration Create(InputActions.CombatActions actions)
        {
            var leftId = actions.Horizontal.bindings.IndexOf(b => b.name == "negative");
            var rightId = actions.Horizontal.bindings.IndexOf(b => b.name == "positive");
            var upId = actions.Vertical.bindings.IndexOf(b => b.name == "positive");

            var defaultActions = new InputActions().Combat;
            var config = new Configuration();

            string GetPath(InputBinding binding)
            {
                if (string.IsNullOrEmpty(binding.overridePath) || binding.overridePath == binding.path) return null;
                return binding.overridePath;
            }

            config.Left = GetPath(actions.Horizontal.bindings[leftId]);
            config.Right = GetPath(actions.Horizontal.bindings[rightId]);
            config.Up = GetPath(actions.Vertical.bindings[upId]);
            config.Fire1 = GetPath(actions.Fire1.bindings[0]);
            config.Fire2 = GetPath(actions.Fire2.bindings[0]);
            config.Fire3 = GetPath(actions.Fire3.bindings[0]);
            config.Fire4 = GetPath(actions.Fire4.bindings[0]);
            config.Fire5 = GetPath(actions.Fire5.bindings[0]);
            config.Fire6 = GetPath(actions.Fire6.bindings[0]);

            return config;
        }

        public static Configuration FromJson(string json)
        {
            try
            {
                return string.IsNullOrEmpty(json) ? new Configuration() : JsonUtility.FromJson<Configuration>(json);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"InputSystemKeyboard: failed to parse {json}");
                return new Configuration();
            }
        }

        public void Apply(InputActions.CombatActions actions)
        {
            var leftId = actions.Horizontal.bindings.IndexOf(b => b.name == "negative");
            var rightId = actions.Horizontal.bindings.IndexOf(b => b.name == "positive");
            var upId = actions.Vertical.bindings.IndexOf(b => b.name == "positive");

            if (!string.IsNullOrEmpty(Left)) actions.Horizontal.ApplyBindingOverride(leftId, Left);
            if (!string.IsNullOrEmpty(Right)) actions.Horizontal.ApplyBindingOverride(rightId, Right);
            if (!string.IsNullOrEmpty(Up)) actions.Vertical.ApplyBindingOverride(upId, Up);
            if (!string.IsNullOrEmpty(Fire1)) actions.Fire1.ApplyBindingOverride(0, Fire1);
            if (!string.IsNullOrEmpty(Fire2)) actions.Fire2.ApplyBindingOverride(0, Fire2);
            if (!string.IsNullOrEmpty(Fire3)) actions.Fire3.ApplyBindingOverride(0, Fire3);
            if (!string.IsNullOrEmpty(Fire4)) actions.Fire4.ApplyBindingOverride(0, Fire4);
            if (!string.IsNullOrEmpty(Fire5)) actions.Fire5.ApplyBindingOverride(0, Fire5);
            if (!string.IsNullOrEmpty(Fire6)) actions.Fire6.ApplyBindingOverride(0, Fire6);
        }

        public string ToJson()
        {
            var builder = new StringBuilder();

            void AddAction(string name, string value)
            {
                if (string.IsNullOrEmpty(value)) return;
                if (builder.Length > 0) builder.Append(',');
                builder.Append(Quote).Append(name).Append(Quote).Append(':').Append(Quote).Append(value).Append(Quote);
            }

            AddAction(nameof(Left), Left);
            AddAction(nameof(Right), Right);
            AddAction(nameof(Up), Up);
            AddAction(nameof(Fire1), Fire1);
            AddAction(nameof(Fire2), Fire2);
            AddAction(nameof(Fire3), Fire3);
            AddAction(nameof(Fire4), Fire4);
            AddAction(nameof(Fire5), Fire5);
            AddAction(nameof(Fire6), Fire6);

            if (builder.Length == 0) return null;

            builder.Insert(0, "{ ");
            builder.Append(" }");

            return builder.ToString();
        }

        public string Left;
        public string Right;
        public string Up;
        public string Fire1;
        public string Fire2;
        public string Fire3;
        public string Fire4;
        public string Fire5;
        public string Fire6;

        private const string Quote = "\"";
    }

    public class KeyBindingsChangedSignal : SmartWeakSignal<string>
    {
        public class Trigger : TriggerBase { }
    }

    public class MouseEnabledSignal : SmartWeakSignal<bool>
    {
        public class Trigger : TriggerBase { }
    }
}
