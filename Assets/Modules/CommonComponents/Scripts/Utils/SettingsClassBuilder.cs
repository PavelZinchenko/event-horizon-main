#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace AppConfiguration.Utils
{
    internal class SettingsClassBuilder
    {
        private readonly string _className;
        private string _namespace;
        private bool _static;
        private bool _partial;

        private readonly List<ContentData> _content = new();

        private bool _requiresUnityEngine;

        private int _braceCounter;
        private StringBuilder _stringBuilder;

        private ContentData Content => _content[_content.Count - 1];

        public SettingsClassBuilder(string name)
        {
            Assert.IsFalse(string.IsNullOrEmpty(name));
            _className = name;
            StartNewSection();
        }

        public SettingsClassBuilder SetNamespace(string name)
        {
            _namespace = name;
            return this;
        }

        public SettingsClassBuilder AsStatic()
        {
            _static = true;
            return this;
        }

        public SettingsClassBuilder AsPartial()
        {
            _partial = true;
            return this;
        }

        public SettingsClassBuilder AddString(string name, string value)
        {
            Assert.IsFalse(string.IsNullOrEmpty(name));
            Content.StringFields.Add((name.ToPublicPropertyName(), value));
            return this;
        }

        public SettingsClassBuilder AddColor(string name, Color value)
        {
            Assert.IsFalse(string.IsNullOrEmpty(name));
            _requiresUnityEngine = true;
            Content.ColorFields.Add((name.ToPublicPropertyName(), value));
            return this;
        }

        public SettingsClassBuilder StartNewSection()
        {
            _content.Add(new ContentData());
            return this;
        }

        public SettingsClassBuilder WithColorGetter(string methodName, string enumName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(methodName));
            Assert.IsFalse(string.IsNullOrEmpty(enumName));
            Content.ColorEnumName = enumName;
            Content.ColorGetterName = methodName;
            return this;
        }

        public string Build()
        {
            _stringBuilder = new StringBuilder();
            _braceCounter = 0;

            _stringBuilder.AppendLine(Strings.Header);

            GenerateUsings();
            GenerateNamespace();
            GenerateClass();

            foreach (var item in _content)
            {
                GenerateStringFields(item);
                GenerateColorFields(item);
                GenerateColorGetter(item);
                _stringBuilder.AppendLine();
            }

            while (_braceCounter > 0)
                CloseBrace();

            return _stringBuilder.ToString();
        }

        private void GenerateStringFields(ContentData content)
        {
            foreach (var item in content.StringFields)
            {
                AddIndent();
                _stringBuilder.AppendLine($"{Strings.Public} {Strings.Const} {Strings.String} {item.name} = \"{item.value}\";");
            }
        }

        private void GenerateColorFields(ContentData content)
        {
            foreach (var item in content.ColorFields)
            {
                AddIndent();
                _stringBuilder.AppendLine($"{Strings.Public} {Strings.Static} {Strings.Readonly} {Strings.Color} {item.name} = " +
                    $"{Strings.New}({item.value.r}f,{item.value.g}f,{item.value.b}f,{item.value.a}f);");
            }
        }

        private void GenerateColorGetter(ContentData content)
        {
            if (string.IsNullOrEmpty(content.ColorGetterName)) return;
            if (string.IsNullOrEmpty(content.ColorEnumName)) return;

            var enumName = content.ColorEnumName.FirstCharToUpperCase();
            var values = content.ColorFields.Select(item => item.name);

            _stringBuilder.AppendLine();
            GenerateEnum(enumName, values);
            _stringBuilder.AppendLine();
            GenerateGetter(content.ColorGetterName, Strings.Color, enumName, values);
        }

        private void GenerateEnum(string name, IEnumerable<string> items)
        {
            AddIndent();
            _stringBuilder.AppendLine($"{Strings.Public} {Strings.Enum} {name}");

            OpenBrace();

            foreach (var item in items)
            {
                AddIndent();
                _stringBuilder.AppendLine($"{item},");
            }

            CloseBrace();
        }

        private void GenerateGetter(string methodName, string typeName, string enumName, IEnumerable<string> items)
        {
            var paramName = enumName.FirstCharToLowerCase();
            AddIndent();
            _stringBuilder.AppendLine($"{Strings.Public} {Strings.Static} {typeName} {methodName}({enumName} {paramName})");
            OpenBrace();

            AddIndent();
            _stringBuilder.AppendLine($"{Strings.Switch} ({paramName})");
            OpenBrace();

            foreach (var item in items)
            {
                AddIndent();
                _stringBuilder.AppendLine($"{Strings.Case} {enumName}.{item}: {Strings.Return} {item};");
            }

            AddIndent();
            _stringBuilder.AppendLine($"{Strings.Default}: {Strings.ThrowException}");

            CloseBrace();
            CloseBrace();
        }

        private void GenerateUsings()
        {
            if (_requiresUnityEngine)
            {
                AddIndent();
                _stringBuilder.AppendLine(Strings.UsingUnityEngine);
                _stringBuilder.AppendLine();
            }
        }

        private void GenerateNamespace()
        {
            if (!string.IsNullOrEmpty(_namespace))
            {
                AddIndent();
                _stringBuilder.AppendLine($"{Strings.Namespace} {_namespace}");
                OpenBrace();
            }
        }

        private void GenerateClass()
        {
            AddIndent();

            _stringBuilder.AppendWithSpace(Strings.Public);

            if (_static)
                _stringBuilder.AppendWithSpace(Strings.Static);
            if (_partial)
                _stringBuilder.AppendWithSpace(Strings.Partial);

            _stringBuilder.AppendWithSpace(Strings.Class);
            _stringBuilder.AppendLine(_className);
            OpenBrace();
        }

        private void OpenBrace()
        {
            AddIndent();
            _stringBuilder.Append('{');
            _stringBuilder.AppendLine();
            _braceCounter++;
        }

        private void CloseBrace()
        {
            _braceCounter--;
            AddIndent();
            _stringBuilder.Append('}');
            _stringBuilder.AppendLine();
        }
        
        private void AddIndent(int size = 4)
        {
            _stringBuilder.Append(' ', _braceCounter*size);
        }

        private class ContentData
        {
            public readonly List<(string name, string value)> StringFields = new();
            public readonly List<(string name, Color value)> ColorFields = new();

            public string ColorEnumName;
            public string ColorGetterName;
        }

        private static class Strings
        {
            public const string Header =
                "// This code was automatically generated.\n" +
                "// Changes to this file may cause incorrect behavior and will be lost if\n" +
                "// the code is regenerated.\n";

            public const string Color = "Color";
            public const string String = "string";
            public const string Public = "public";
            public const string Static = "static";
            public const string Partial = "partial";
            public const string Class = "class";
            public const string Const = "const";
            public const string Readonly = "readonly";
            public const string New = "new";
            public const string Enum = "enum";
            public const string Switch = "switch";
            public const string Case = "case";
            public const string Default = "default";
            public const string Return = "return";
            public const string Namespace = "namespace";
            public const string UsingUnityEngine = "using UnityEngine;";
            public const string ThrowException = "throw new System.InvalidOperationException();";
        }
    }

    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendWithSpace(this StringBuilder stringBuilder, string value)
        {
            stringBuilder.Append(value);
            stringBuilder.Append(' ');
            return stringBuilder;
        }

        public static StringBuilder AppendWithSpace(this StringBuilder stringBuilder, string value1, string value2)
        {
            stringBuilder.Append(value1);
            stringBuilder.Append(' ');
            stringBuilder.Append(value2);
            return stringBuilder;
        }
    }
}

#endif
