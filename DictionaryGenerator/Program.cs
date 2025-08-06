using System;
using System.IO;
using System.Text;

namespace DictionaryGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: DictionaryGenerator <namespace> <dictionaryPath> <outputPath>");
                return;
            }

            string ns = args[0];
            string dictionaryPath = args[1];
            string outputPath = args[2];

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var dictionaryFiles = Directory.GetFiles(dictionaryPath, "dictionary.*");

            foreach (var dictionaryFile in dictionaryFiles)
            {
                ProcessDictionaryFile(ns, dictionaryFile, outputPath);
            }
        }

        static void ProcessDictionaryFile(string ns, string dictionaryFile, string outputPath)
        {
            Console.WriteLine($"Processing {dictionaryFile}...");
            var lines = File.ReadAllLines(dictionaryFile);
            var className = Path.GetFileNameWithoutExtension(dictionaryFile).Replace(".", "_");
            var sb = new StringBuilder();

            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");

            foreach (var line in lines)
            {
                if (line.StartsWith("ATTRIBUTE"))
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        var attributeName = parts[1];
                        var attributeId = parts[2];
                        sb.AppendLine($"        public const int {attributeName} = {attributeId};");
                    }
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(Path.Combine(outputPath, $"{className}.cs"), sb.ToString());
        }
    }
}
