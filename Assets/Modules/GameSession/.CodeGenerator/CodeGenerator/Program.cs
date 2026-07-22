using System;
using System.IO;
using System.Globalization;

namespace CodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: " + AppDomain.CurrentDomain.FriendlyName + "<schema dir> <output dir>");
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
                var latestSchema = Schema.DataSchema.Load(path, latestVersion);

				var builder = new GameCode.Builder(new Utils.CodeWriter(Path.Combine(args[1], "GeneratedCode")), versions, path);
				builder.Build();
			}
			catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
		}
	}
}
