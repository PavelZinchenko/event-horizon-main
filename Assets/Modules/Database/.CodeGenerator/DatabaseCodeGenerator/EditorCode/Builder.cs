using System;
using DatabaseCodeGenerator.EditorCode.Templates;
using DatabaseCodeGenerator.Schema;
using DatabaseCodeGenerator.Utils;

namespace DatabaseCodeGenerator.EditorCode
{
    public class Builder
    {
        public Builder(DatabaseSchema schema, CodeWriter codeWriter)
        {
            _schema = schema;
            _codeWriter = codeWriter;
        }

        public void Build()
        {
            GenerateTypes();
            GenerateDatabase();

            foreach (var item in _schema.Enums)
                GenerateEnum(item);

            foreach (var item in _schema.Objects)
            {
                GenerateSerializableClass(item, ObjectType.Class);
                GenerateDataClass(item, ObjectType.Class);
            }

            foreach (var item in _schema.Structs)
            {
                GenerateSerializableClass(item, ObjectType.Struct);
                GenerateDataClass(item, ObjectType.Struct);
            }

            foreach (var item in _schema.Configurations)
            {
                GenerateSerializableClass(item, ObjectType.Configuration);
                GenerateDataClass(item, ObjectType.Configuration);
            }
            
            _codeWriter.DeleteOldFiles();
        }

        private void GenerateTypes()
        {
            _codeWriter.Write(Utils.SerializableNamespace, "SerializableItem", new SerializableItemTemplate().TransformText());
        }

        private void GenerateDatabase()
        {
            _codeWriter.Write(string.Empty, "Database", new DatabaseTemplate(_schema).TransformText());
            _codeWriter.Write(string.Empty, "DatabaseException", new DatabaseExceptionTemplate().TransformText());
            _codeWriter.Write(Utils.StorageNamespace, "Interfaces", new StorageInterfacesTemplate().TransformText());
            _codeWriter.Write(Utils.StorageNamespace, "DatabaseContent", new DatabaseContentTemplate(_schema).TransformText());
        }

        private void GenerateEnum(XmlEnumItem item)
        {
            var template = new EnumTemplate(item);
            var data = template.TransformText();

            _codeWriter.Write(Utils.EnumsNamespace, item.name, data);
        }

        private void GenerateSerializableClass(XmlClassItem item, ObjectType type)
        {
            var template = new SerializableTemplate(item, _schema, type);
            var data = template.TransformText();

            _codeWriter.Write(Utils.SerializableNamespace, item.name, data);
        }

        private void GenerateDataClass(XmlClassItem item, ObjectType type)
        {
            string data;
            if (!string.IsNullOrEmpty(item.switchEnum))
                data = new MutableObjectTemplate(item, _schema, type).TransformText();
            else
                data = new ObjectTemplate(item, _schema, type).TransformText();

            _codeWriter.Write(Utils.ClassesNamespace, item.name, data);
        }

        private readonly CodeWriter _codeWriter;
        private readonly DatabaseSchema _schema;
    }
}
