using System;
using System.IO;
using NUnit.Framework;

namespace CodeGenerator.Utils
{
    public class CodeWriter
    {
        public CodeWriter(string rootFolder)
        {
            Assert.IsNotNull(rootFolder);
            Assert.IsNotEmpty(rootFolder);

            _rootFolder = rootFolder;
        }

        public void Write(string ns, string filename, string content)
        {
            var fullpath = Path.Combine(_rootFolder, ns.Replace(".", "/"));
            Directory.CreateDirectory(fullpath);
            File.WriteAllText(Path.Combine(fullpath, filename + Ext), content);
        }

        public void DeleteGeneratedFiles()
        {
            try
            {
                Directory.Delete(_rootFolder, true);
            }
            catch (DirectoryNotFoundException e)
            {
                return;
            }
        }

        private readonly string _rootFolder;
        private const string Ext = ".cs";
    }
}
