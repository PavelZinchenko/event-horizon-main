using CodeGenerator.GameCode.Templates;
using CodeGenerator.Schema;
using CodeGenerator.Utils;

namespace CodeGenerator.GameCode
{
	public class Builder
    {
		private readonly CodeWriter _codeWriter;
		private readonly VersionList _versionList;
		private readonly string _schemaRootFolder;

		public Builder(CodeWriter codeWriter, VersionList versionList, string schemaRootFolder)
        {
            _codeWriter = codeWriter;
            _versionList = versionList;
            _schemaRootFolder = schemaRootFolder;
        }

        public void Build()
        {
            _codeWriter.DeleteGeneratedFiles();

			var context = new BuilderContext();			
            foreach (var version in _versionList.Items)
            {
				var schema = DataSchema.Load(_schemaRootFolder, version);
				context.ApplySchema(schema);
            }

			foreach (var data in context.Versions)
				GenerateSchemaCode(data);

			GenerateSessionLoader(context);
        }

		private void GenerateSessionLoader(BuilderContext context)
		{
			_codeWriter.Write(string.Empty, "SessionLoader",
				new SessionLoaderTemplate(_versionList, context).TransformText());
		}

		private void GenerateSchemaCode(SchemaVersionInfo context)
        {
			foreach (var item in context.ModifiedObjects)
				GenerateClass(item.Schema, context);

			foreach (var item in context.ModifiedStructs)
				GenerateStruct(item.Schema, context);
		}

		private void GenerateClass(XmlClassItem item, SchemaVersionInfo context)
		{
			string data = new ObjectTemplate(item, context).TransformText();
			_codeWriter.Write(context.GetObjectNamespace(item.name), item.name, data);
		}

		private void GenerateStruct(XmlClassItem item, SchemaVersionInfo context)
		{
			string data = new StructTemplate(item, context).TransformText();
			_codeWriter.Write(context.GetObjectNamespace(item.name), item.name, data);
		}
    }
}
