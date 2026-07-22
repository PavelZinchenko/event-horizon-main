using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using System.IO;

namespace DatabaseCodeGenerator.Schema
{
    public class DatabaseSchema
    {
        private DatabaseSchema(SchemaVersion version) 
        {
            _version = version;
        }

        public static DatabaseSchema Load(string xmlPath, SchemaVersion version)
        {
            var schema = new DatabaseSchema(version);
            schema.LoadResources(Path.Combine(xmlPath, version.Path));
            schema.ParseItemTypes();
            schema.ValidateObjects();

            return schema;
        }

        public SchemaVersion Version => _version;

        public IEnumerable<XmlEnumItem> Enums => _enums.Values;
        public IEnumerable<XmlClassItem> Objects => _classes.Values;
        public IEnumerable<XmlClassItem> Structs => _structs.Values;
        public IEnumerable<XmlClassItem> Configurations => _configurations.Values;
        public IEnumerable<XmlExpressionItem> Expressions => _expressions.Values;

        public bool HasItemType(string type) => _itemTypes.ContainsKey(type);
        public bool HasEnum(string name) => _enums.ContainsKey(name);
        public bool HasObject(string name) => _classes.ContainsKey(name);
        public bool HasStruct(string name) => _structs.ContainsKey(name);
        public bool HasExpression(string name) => _expressions.ContainsKey(name);

        public XmlEnumItem GetEnum(string name) => _enums.TryGetValue(name, out var item) ? item : null;
        public XmlClassItem GetObject(string name) => _classes.TryGetValue(name, out var item) ? item : null;
        public XmlClassItem GetStruct(string name) => _structs.TryGetValue(name, out var item) ? item : null;
        public XmlExpressionItem GetExpression(string name) => _expressions.TryGetValue(name, out var item) ? item : null;

        private void LoadResources(string path)
        {
            var files = Directory.EnumerateFiles(path, "*.xml", SearchOption.AllDirectories);

            var typeSerializer = new XmlSerializer(typeof(XmlTypeInfo));
            var enumSerializer = new XmlSerializer(typeof(XmlEnumItem));
            var classSerializer = new XmlSerializer(typeof(XmlClassItem));
            var expressionSerializer = new XmlSerializer(typeof(XmlExpressionItem));

            var processedFiles = 0;
            foreach (var file in files)
            {
                try
                {
                    Console.WriteLine("Processing file " + file);
                    var data = File.ReadAllText(file);

                    XmlTypeInfo typeInfo;
                    using (var reader = new System.IO.StringReader(data))
                        typeInfo = typeSerializer.Deserialize(reader) as XmlTypeInfo;

                    if (string.IsNullOrEmpty(typeInfo.name))
                        throw new InvalidSchemaException("Object name cannot be empty - " + file);
                    if (string.IsNullOrEmpty(typeInfo.type))
                        throw new InvalidSchemaException("Object type cannot be empty - " + file);

                    using (var reader = new System.IO.StringReader(data))
                    {
                        if (typeInfo.type == "enum")
                        {
                            var item = enumSerializer.Deserialize(reader) as XmlEnumItem;
                            CheckEnum(item, false);
                            _enums.Add(item.name, item);
                        }
                        else if (typeInfo.type == "object")
                        {
                            var item = classSerializer.Deserialize(reader) as XmlClassItem;
                            CheckClass(item);
                            _classes.Add(item.name, item);
                        }
                        else if (typeInfo.type == "struct")
                        {
                            var item = classSerializer.Deserialize(reader) as XmlClassItem;
                            CheckClass(item);
                            _structs.Add(item.name, item);
                        }
                        else if (typeInfo.type == "settings")
                        {
                            var item = classSerializer.Deserialize(reader) as XmlClassItem;
                            CheckClass(item);
                            _configurations.Add(item.name, item);
                        }
                        else if (typeInfo.type == "expression")
                        {
                            var item = expressionSerializer.Deserialize(reader) as XmlExpressionItem;
                            CheckExpression(item);
                            _expressions.Add(item.name, item);
                        }
                        else
                        {
                            throw new InvalidSchemaException("Unknown item type: " + typeInfo.type + " in " + file);
                        }
                    }

                    processedFiles++;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to load asset " + file + ": " + e.Message);
                    throw;
                }

                if (processedFiles == 0)
                    throw new InvalidSchemaException("No xml files found at " + path);
            }
        }

        private void CheckEnum(XmlEnumItem data, bool mustHaveValues)
        {
            Assert.IsNotNull(data);

            foreach (var item in data.items)
            {
                if (string.IsNullOrEmpty(item.name))
                    throw new InvalidSchemaException("Field name cannot be empty - " + data.name);

                var keys = new HashSet<string>();
                var values = new HashSet<int>();

                if (mustHaveValues)
                {
                    if (string.IsNullOrEmpty(item.value))
                        throw new InvalidSchemaException("Value not assigned for " + data.name + "." + item.name);
                    if (keys.Contains(item.value))
                        throw new InvalidSchemaException("Duplicate key in enum " + data.name + "." + item.name);
                    keys.Add(item.name);

                    if (!int.TryParse(item.value, out var value))
                        throw new InvalidSchemaException(data.name + "." + item.value + " must have integer value");
                    if (!values.Add(value))
                        throw new InvalidSchemaException(data.name + "." + item.value + " has duplicate value");
                }
            }
        }

        private void CheckClass(XmlClassItem data)
        {
            Assert.IsNotNull(data);

            var memberNames = new HashSet<string>();
            foreach (var item in data.members)
            {
                if (string.IsNullOrEmpty(item.name))
                    throw new InvalidSchemaException("Member name cannot be empty - " + data.name);
                if (string.IsNullOrEmpty(item.type))
                    throw new InvalidSchemaException("Member type cannot be empty - " + data.name + "." + item.name);

                if (string.IsNullOrEmpty(item.caseValue))
                {
                    var name = string.IsNullOrEmpty(item.alias) ? item.name : item.alias;
                    if (memberNames.Contains(name))
                        throw new InvalidSchemaException("Duplicate member name - " + data.name + "." + item.name);
                    memberNames.Add(name);
                }
            }
        }

        private void CheckExpression(XmlExpressionItem data)
        {
            Assert.IsNotNull(data);

            var paramNames = new HashSet<string>();
            foreach (var item in data.items)
            {
                if (string.IsNullOrEmpty(item.name))
                    throw new InvalidSchemaException("Param name cannot be empty - " + data.name);
                if (string.IsNullOrEmpty(item.type))
                    throw new InvalidSchemaException("Param type cannot be empty - " + data.name + "." + item.name);
            }
        }

        private void ParseItemTypes()
        {
            if (!_enums.TryGetValue(Constants.ItemTypeEnum, out var itemTypeEnum))
                throw new InvalidSchemaException(Constants.ItemTypeEnum + " enum must be defined");

            CheckEnum(itemTypeEnum, true);

            foreach (var item in itemTypeEnum.items)
                _itemTypes.Add(item.name, int.Parse(item.value));
        }

        private void ValidateObjects()
        {
            var objectNames = new HashSet<string>();
            var objectTypes = new HashSet<string>();
            foreach (var item in _classes.Values.Concat(_configurations.Values).Concat(_structs.Values))
            {
                if (!objectNames.Add(item.name))
                    throw new InvalidSchemaException("Duplicate object name - " + item.name);
                if (!string.IsNullOrEmpty(item.typeid) && !objectTypes.Add(item.typeid))
                    throw new InvalidSchemaException("Duplicate object type - " + item.name);
            }

            foreach (var item in _structs.Values)
            {
                if (!string.IsNullOrEmpty(item.typeid))
                    throw new InvalidSchemaException("Sturcts can't have typeid - " + item.name);
            }

            foreach (var item in _classes.Values.Concat(_configurations.Values))
            {
                if (string.IsNullOrEmpty(item.typeid))
                    throw new InvalidSchemaException("Object typeid cannot be empty - " + item.name);
                if (!HasItemType(item.typeid))
                    throw new InvalidSchemaException("Unknown typeid " + item.typeid + " for object " + item.name);
            }

            foreach (var item in _classes.Values.Concat(_structs.Values))
                CheckMutableMembers(item);

            foreach (var item in _configurations.Values)
            {
                if (!string.IsNullOrEmpty(item.switchEnum))
                    throw new InvalidSchemaException("Settings can't use switch - " + item.name);
            }
        }

        private void CheckMutableMembers(XmlClassItem classItem)
        {
            HashSet<string> enumValues = null;
            var isMutable = !string.IsNullOrEmpty(classItem.switchEnum);
            if (isMutable)
            {
                var enumMember = classItem.members.FirstOrDefault(item => item.name.Equals(classItem.switchEnum, StringComparison.Ordinal));
                if (enumMember == null)
                    throw new InvalidSchemaException("Switch member not found - " + classItem.name + "." + classItem.switchEnum);
                if (enumMember.type != Constants.TypeEnum)
                    throw new InvalidSchemaException("Switch member should have enum type - " + classItem.name + "." + classItem.switchEnum);

                if (!_enums.TryGetValue(enumMember.typeid, out var switchEnum))
                    throw new InvalidSchemaException("Unknown enum type - " + classItem.name + "." + classItem.switchEnum);
                enumValues = new HashSet<string>(switchEnum.items.Select(item => item.name));
            }

            foreach (var item in classItem.members)
            {
                if (string.IsNullOrEmpty(item.caseValue))
                    continue;

                if (!isMutable)
                    throw new InvalidSchemaException("Member has case attribute but switch attribute hasn't been defined " + classItem.name + "." + item.name);

                var caseValues = item.caseValue.Split(Constants.ValueSeparators, StringSplitOptions.RemoveEmptyEntries);
                var wrongValue = caseValues.FirstOrDefault(value => !enumValues.Contains(value));
                if (wrongValue != null)
                    throw new InvalidSchemaException("Unknown case value in " + classItem.name + "." + item.name + " - " + wrongValue);
            }
        }

        private readonly SchemaVersion _version;
        private readonly Dictionary<string, int> _itemTypes = new Dictionary<string, int>();
        private readonly Dictionary<string, XmlEnumItem> _enums = new Dictionary<string, XmlEnumItem>();
        private readonly Dictionary<string, XmlClassItem> _structs = new Dictionary<string, XmlClassItem>();
        private readonly Dictionary<string, XmlClassItem> _classes = new Dictionary<string, XmlClassItem>();
        private readonly Dictionary<string, XmlClassItem> _configurations = new Dictionary<string, XmlClassItem>();
        private readonly Dictionary<string, XmlExpressionItem> _expressions = new Dictionary<string, XmlExpressionItem>();
    }
}
