using System;
using System.IO;
using System.Linq;
using System.Globalization;

namespace DatabaseCodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: " + AppDomain.CurrentDomain.FriendlyName + "<schema dir> <output dir> [editor] [game]");
                return;
            }

            try
            {
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

                var filename = args[0];
                var versions = new Schema.VersionList(filename);
                if (!versions.TryGetLatest(out var latestVersion))
                {
                    Console.WriteLine("No schema versions found");
                    return;
                }

                var path = Path.GetDirectoryName(filename);
                var latestSchema = Schema.DatabaseSchema.Load(path, latestVersion);

                if (args.Skip(2).Contains("game"))
                {
                    var builder = new GameCode.Builder(latestSchema, new Utils.CodeWriter(Path.Combine(args[1], "GeneratedGameCode")));
                    builder.Build();
                }

                if (args.Skip(2).Contains("editor"))
                {
                    var builder = new EditorCode.Builder(latestSchema, new Utils.CodeWriter(Path.Combine(args[1], "GeneratedEditorCode")));
                    builder.Build();
                }

                if (args.Skip(2).Contains("upgrade_game"))
                {
                    var builder = new MigrationCode.Builder(
                        new Utils.CodeWriter(Path.Combine(args[1], "GeneratedMigrationCode")), 
                        versions, path, MigrationCode.Builder.Context.GameCodeContext());
                    
                    builder.Build();
                }

                if (args.Skip(2).Contains("upgrade_editor"))
                {
                    var builder = new MigrationCode.Builder(
                        new Utils.CodeWriter(Path.Combine(args[1], "GeneratedMigrationCode")),
                        versions, path, MigrationCode.Builder.Context.EditorCodeContext());

                    builder.Build();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
