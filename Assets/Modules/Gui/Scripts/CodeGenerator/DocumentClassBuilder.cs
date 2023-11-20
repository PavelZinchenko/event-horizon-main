#if UNITY_EDITOR

using System.Text;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Assertions;

namespace Gui.CodeGenerator
{
    internal class DocumentClassBuilder
    {
        private readonly string _className;
        private string _namespace;
        private int _braceCounter;
        private StringBuilder _stringBuilder;
        private List<Property> _properties = new();

        public DocumentClassBuilder(string name)
        {
            Assert.IsFalse(string.IsNullOrEmpty(name));
            _className = name;
        }

        public DocumentClassBuilder SetNamespace(string name)
        {
            _namespace = name;
            return this;
        }

        public string Build(UIDocument document)
        {
            _stringBuilder = new StringBuilder();
            _braceCounter = 0;

            _stringBuilder.AppendLine(
                "// This code was automatically generated.\n" +
                "// Changes to this file may cause incorrect behavior and will be lost if\n" +
                "// the code is regenerated.\n");

            GenerateUsings();
            GenerateNamespace();
            GenerateClass();

            var rootNode = new Node(-1, null, null);
            TryBuildDocumentTree(rootNode, document.rootVisualElement);
            CalculateProperties(rootNode, null, null);

            GenerateProperties();

            while (_braceCounter > 0)
                CloseBrace();

            return _stringBuilder.ToString();
        }

        private bool TryBuildDocumentTree(Node parent, VisualElement element)
        {
            int index = 0;
            bool success = false;

            foreach (var child in element.Children())
            {
                var hasName = !string.IsNullOrEmpty(child.name);
                var type = child.GetType();
                if (type.IsNested) continue;

                var node = new Node(index, child.name, type.Name);

                if (TryBuildDocumentTree(node, child) || hasName)
                {
                    parent.Add(node);
                    success = true;
                }

                index++;
            }

            return success;
        }

        private void CalculateProperties(Node node, string name, string path)
        {
            if (node.Index >= 0)
                path += "." + node.Index;

            if (!string.IsNullOrEmpty(node.Name))
            {
                if (!string.IsNullOrEmpty(name))
                    name += "_";

                name += node.Name;
                _properties.Add(new() { Name = name, Type = node.Type, Path = path });
            }

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                    CalculateProperties(child, name, path);
            }
        }

        private void GenerateProperties()
        {
            AddIndent();
            _stringBuilder.AppendLine($"private UIDocument _uiDocument;");

            foreach (var property in _properties)
            {
                AddIndent();
                _stringBuilder.AppendLine($"private {property.Type} _{property.Name};");
            }

            _stringBuilder.AppendLine();

            AddIndent();
            _stringBuilder.AppendLine($"public UIDocument UiDocument => _uiDocument ??= GetComponent<UIDocument>();");

            foreach (var property in _properties)
            {
                AddIndent();
                _stringBuilder.Append($"public {property.Type} {property.Name} => _{property.Name} ??= ({property.Type})UiDocument.rootVisualElement");

                var path = property.Path.Split('.', System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in path)
                {
                    var index = int.Parse(item);
                    _stringBuilder.Append('[').Append(index).Append(']');
                }

                _stringBuilder.Append(';');
                _stringBuilder.AppendLine();
            }
        }

        private void GenerateUsings()
        {
            AddIndent();
            _stringBuilder.AppendLine("using UnityEngine;");
            AddIndent();
            _stringBuilder.AppendLine("using UnityEngine.UIElements;");
            _stringBuilder.AppendLine();
        }

        private void GenerateNamespace()
        {
            if (!string.IsNullOrEmpty(_namespace))
            {
                AddIndent();
                _stringBuilder.AppendLine($"namespace {_namespace}");
                OpenBrace();
            }
        }

        private void GenerateClass()
        {
            AddIndent();
            _stringBuilder.AppendLine("[RequireComponent(typeof(UIDocument))]");
            AddIndent();
            _stringBuilder.AppendLine($"public partial class {_className} : MonoBehaviour");
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

        private class Node
        {
            public Node(int index, string name, string type)
            {
                Index = index;
                Name = name;
                Type = type;
                Children = null;
            }

            public void Add(Node child)
            {
                if (Children == null)
                    Children = new();

                Children.Add(child);
            }

            public readonly int Index;
            public readonly string Type;
            public readonly string Name;
            public List<Node> Children;
        }

        private struct Property
        {
            public string Name;
            public string Type;
            public string Path;
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
