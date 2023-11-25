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
        private string _baseClass = "MonoBehaviour";
        private string _namespace;
        private string[] _rootElement;
        private List<string> _usings = new();
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

        public DocumentClassBuilder SetBaseClass(string baseClass)
        {
            _baseClass = baseClass;
            return this;
        }

        public DocumentClassBuilder Using(string value)
        {
            _usings.Add(value);
            return this;
        }

        public DocumentClassBuilder SetRootElement(string rootElement)
        {
            _rootElement = rootElement?.Split('.', System.StringSplitOptions.RemoveEmptyEntries);
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

            var rootNode = new Node(null, -1, null, null);
            TryBuildDocumentTree(rootNode, document.rootVisualElement);

            if (!TryFindNode(rootNode, 0, out var node))
                throw new System.ArgumentException("Root node not found: " + string.Join('.', _rootElement));

            CalculateProperties(node, null);

            GenerateProperties();

            while (_braceCounter > 0)
                CloseBrace();

            return _stringBuilder.ToString();
        }

        private bool TryFindNode(Node parent, int index, out Node node)
        {
            if (_rootElement == null || index == _rootElement.Length)
            {
                node = parent;
                return true;
            }

            var elementName = _rootElement[index];
            foreach (var child in parent.Children)
            {
                var name = child.Name;
                if (string.IsNullOrEmpty(name))
                {
                    if (TryFindNode(child, index, out node))
                        return true;
                }
                else if (string.Compare(name, elementName, System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (TryFindNode(child, index + 1, out node))
                        return true;
                }
            }

            node = null;
            return false;
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

                var node = new Node(parent, index, child.name, type.Name);

                if (TryBuildDocumentTree(node, child) || hasName)
                {
                    parent.Add(node);
                    success = true;
                }

                index++;
            }

            return success;
        }

        private void CalculateProperties(Node node, string name)
        {
            if (!string.IsNullOrEmpty(node.Name))
            {
                if (!string.IsNullOrEmpty(name))
                    name += "_";

                name += node.Name;
                _properties.Add(new() { Name = name, Type = node.Type, Path = node.GetPath() });
            }

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                    CalculateProperties(child, name);
            }
        }

        private void GenerateProperties()
        {
            foreach (var property in _properties)
            {
                AddIndent();
                _stringBuilder.AppendLine($"private {property.Type} _{property.Name};");
            }

            _stringBuilder.AppendLine();
            if (_properties.Count > 0)
            {
                AddIndent();
                _stringBuilder.AppendLine($"public override VisualElement RootElement => {_properties[0].Name};");
            }

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
            foreach (var value in _usings)
                _stringBuilder.Append("using ").Append(value).AppendLine(";");
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
            _stringBuilder.Append($"public partial class {_className}");

            if (!string.IsNullOrEmpty(_baseClass))
            {
                _stringBuilder.Append(" : ");
                _stringBuilder.Append(_baseClass);
            }

            _stringBuilder.AppendLine();
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
            public Node(Node parent, int index, string name, string type)
            {
                Index = index;
                Name = name;
                Type = type;
                Parent = parent;
                Children = null;
            }

            public void Add(Node child)
            {
                if (Children == null)
                    Children = new();

                Children.Add(child);
            }

            public string GetPath()
            {
                return CreatePathElement(new StringBuilder()).ToString();
            }

            private StringBuilder CreatePathElement(StringBuilder builder)
            {
                if (Parent != null)
                {
                    Parent.CreatePathElement(builder);
                    builder.Append('.');
                    builder.Append(Index);
                }

                return builder;
            }

            public readonly int Index;
            public readonly string Type;
            public readonly string Name;
            public List<Node> Children;
            public Node Parent;
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
