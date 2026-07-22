using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;

namespace CodeGenerator.Schema
{
    public class DataSchema
    {
        private DataSchema(SchemaVersion version) 
        {
            _version = version;
        }

        public static DataSchema Load(string xmlPath, SchemaVersion version)
        {
            var schema = new DataSchema(version);
            schema.LoadResources(Path.Combine(xmlPath, version.Path));
            schema.ValidateObjects();

            return schema;
        }

        public SchemaVersion Version => _version;

        public IEnumerable<XmlClassItem> Objects => _classes.Values;
        public IEnumerable<XmlClassItem> Structs => _structs.Values;

        public bool HasObject(string name) => _classes.ContainsKey(name);
        public bool HasStruct(string name) => _structs.ContainsKey(name);

        public XmlClassItem GetObject(string name) => _classes.TryGetValue(name, out var item) ? item : null;
        public XmlClassItem GetStruct(string name) => _structs.TryGetValue(name, out var item) ? item : null;

        private void LoadResources(string path)
        {
            var files = Directory.EnumerateFiles(path, "*.xml", SearchOption.AllDirectories);

            var typeSerializer = new XmlSerializer(typeof(XmlTypeInfo));
			var classSerializer = new XmlSerializer(typeof(XmlClassItem));

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
                        if (typeInfo.type == "object")
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

        private void CheckClass(XmlClassItem data)
        {
            var memberNames = new HashSet<string>();

            foreach (var item in data.members)
            {
                if (string.IsNullOrEmpty(item.name))
                    throw new InvalidSchemaException("Member name cannot be empty - " + data.name);
                if (string.IsNullOrEmpty(item.type))
                    throw new InvalidSchemaException("Member type cannot be empty - " + data.name + "." + item.name);
				if (item.name == data.name)
					throw new InvalidSchemaException("Member name cannot be the same as its enclosing type - " + data.name);
				if (!memberNames.Add(item.name))
					throw new InvalidSchemaException("Member name must be unique - " + data.name);
				if (item.type == Constants.TypeMap && string.IsNullOrEmpty(item.key))
					throw new InvalidSchemaException("Map key cannot be empty - " + data.name + "." + item.name);
				if (item.type == Constants.TypeMap || item.type == Constants.TypeList || item.type == Constants.TypeSet || item.type == Constants.TypeInventory)
					if (string.IsNullOrEmpty(item.type))
						throw new InvalidSchemaException("Value cannot be empty - " + data.name + "." + item.name);
			}
		}

        private void ValidateObjects()
        {
            var objectNames = new HashSet<string>();

            foreach (var item in _classes.Values.Concat(_structs.Values))
            {
                if (!objectNames.Add(item.name))
                    throw new InvalidSchemaException("Duplicate object name - " + item.name);
            }
		}

        private readonly SchemaVersion _version;
        private readonly Dictionary<string, XmlClassItem> _classes = new Dictionary<string, XmlClassItem>();
		private readonly Dictionary<string, XmlClassItem> _structs = new Dictionary<string, XmlClassItem>();
    }
}
