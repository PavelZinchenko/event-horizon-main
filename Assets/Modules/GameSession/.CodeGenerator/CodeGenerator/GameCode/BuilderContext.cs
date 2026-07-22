using CodeGenerator.Schema;
using CodeGenerator.Utils;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator.GameCode
{
	public struct ObjectTypeInfo
	{
		public ObjectTypeInfo(XmlClassItem schema, SchemaVersion version)
		{
			Schema = schema;
			Version = version;
		}

		public readonly XmlClassItem Schema;
		public readonly SchemaVersion Version;
	}

	public class SchemaVersionInfo
	{
		private readonly Dictionary<string, ObjectTypeInfo> _objects;
		private readonly BuilderContext _builderContext;
		private SchemaVersion _version;

		public SchemaVersionInfo(BuilderContext builderContext, SchemaVersion version, Dictionary<string, ObjectTypeInfo> objects)
		{
			_builderContext = builderContext;
			_version = version;

			if (objects != null)
				_objects = new Dictionary<string, ObjectTypeInfo>(objects);
			else
				_objects = new Dictionary<string, ObjectTypeInfo>();
		}

		public SchemaVersion Version => _version;
		public Dictionary<string, ObjectTypeInfo> Objects => _objects;

		public bool HasClass(string name) => _objects.TryGetValue(name, out var item) && item.Schema.type == Constants.TypeObject;
		public bool HasStruct(string name) => _objects.TryGetValue(name, out var item) && item.Schema.type == Constants.TypeStruct;
		public XmlClassItem GetObject(string name) => _objects.TryGetValue(name, out var item) ? item.Schema : null;

		public IEnumerable<ObjectTypeInfo> ModifiedObjects => _objects.Values.Where(item => item.Schema.type == Constants.TypeObject && item.Version == _version);
		public IEnumerable<ObjectTypeInfo> ModifiedStructs => _objects.Values.Where(item => item.Schema.type == Constants.TypeStruct && item.Version == _version);

		public bool IsLatestObjectVersion(string name)
		{
			if (!_objects.TryGetValue(name, out var data)) return false;
			return _builderContext.Current.Objects.TryGetValue(name, out var latest) && latest.Version == data.Version;
		}

		public string GetObjectNamespace(string name)
		{
			if (!_objects.TryGetValue(name, out var data)) return null;
			if (_builderContext.Current.Objects.TryGetValue(name, out var latest) && latest.Version == data.Version)
				return Utils.ClassesNamespace;

			return data.Version.ToNamespace();
		}

		public string GetObjectName(string name)
		{
			if (!_objects.TryGetValue(name, out var data)) return null;
			return $"{GetObjectNamespace(name)}.{data.Schema.name}";
		}

		public bool IsChangedInThisVersion(XmlClassMember member) =>
			IsTypeChanged(member.type) || IsTypeChanged(member.key) || IsTypeChanged(member.value);

		public void UpdateObjectsVersion()
		{
			foreach (var item in _objects.Keys.ToArray())
				CheckIfContentChanged(item);
		}

		private bool CheckIfContentChanged(string typename)
		{
			if (IsTypeChanged(typename)) return true;
			if (!_objects.TryGetValue(typename, out var item)) return false;

			if (CheckIfMembersChanged(item.Schema))
			{
				_objects[typename] = new ObjectTypeInfo(item.Schema, _version);
				return true;
			}

			return false;
		}

		private bool CheckIfMembersChanged(XmlClassItem item)
		{
			foreach (var member in item.members)
			{
				if (CheckIfContentChanged(member.type)) return true;
				if (CheckIfContentChanged(member.key)) return true;
				if (CheckIfContentChanged(member.value)) return true;
			}

			return false;
		}

		private bool IsTypeChanged(string typename) => _objects.TryGetValue(typename, out var data) && data.Version == _version;
	}

	public class BuilderContext
	{
		private Dictionary<int, SchemaVersionInfo> _history = new Dictionary<int, SchemaVersionInfo>();
		private SchemaVersionInfo _current;

		public SchemaVersion Version => _current.Version;
		public SchemaVersionInfo Current => _current;
		public IEnumerable<SchemaVersionInfo> Versions => _history.Values;
		public SchemaVersionInfo this[int versionMajor] => _history.TryGetValue(versionMajor, out var data) ? data : null;

		public void ApplySchema(DataSchema schema)
		{
			_current = new SchemaVersionInfo(this, schema.Version, _current?.Objects);
			_history.Add(schema.Version.Major, _current);

			foreach (var item in _current.Objects)
				if (!schema.HasObject(item.Key) && !schema.HasStruct(item.Key))
					_current.Objects.Remove(item.Key);

			foreach (var item in schema.Objects.Concat(schema.Structs))
				if (!Helpers.AreEqual(item, _current.GetObject(item.name)))
					_current.Objects[item.name] = new ObjectTypeInfo(item, schema.Version);

			_current.UpdateObjectsVersion();
		}

		public static bool AreVersionsEqual(
			string firstType,
			string secondType,
			SchemaVersionInfo firstSchema,
			SchemaVersionInfo secondSchema)
		{
			if (firstType != secondType) return false;
			var firstDataFound = firstSchema.Objects.TryGetValue(firstType, out var firstData);
			var secondDataFound = secondSchema.Objects.TryGetValue(firstType, out var secondData);
			if (firstDataFound != secondDataFound)
				throw new System.InvalidOperationException();

			if (!firstDataFound) return true;
			return firstData.Version == secondData.Version;
		}

		public static bool AreVersionsEqual(
			XmlClassMember first,
			XmlClassMember second,
			SchemaVersionInfo firstSchema,
			SchemaVersionInfo secondSchema)
		{
			if (!Helpers.AreEqual(first, second)) return false;
			if (!AreVersionsEqual(first.type, second.type, firstSchema, secondSchema)) return false;
			if (!AreVersionsEqual(first.key, second.key, firstSchema, secondSchema)) return false;
			if (!AreVersionsEqual(first.value, second.value, firstSchema, secondSchema)) return false;
			return true;
		}
	}
}
