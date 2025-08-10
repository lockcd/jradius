using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DHCPPacketGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: DHCPPacketGenerator <javaSourcePath> <csharpOutputPath>");
                return;
            }

            string javaSourcePath = args[0];
            string csharpOutputPath = args[1];

            if (!Directory.Exists(csharpOutputPath))
            {
                Directory.CreateDirectory(csharpOutputPath);
            }

            var javaFiles = Directory.GetFiles(javaSourcePath, "DHCP*.java");
            var codeRegex = new Regex(@"public static final int CODE = (1024 \+ \d+);");

            foreach (var javaFile in javaFiles)
            {
                var className = Path.GetFileNameWithoutExtension(javaFile);
                if (className == "DHCPPacket" || className == "DHCPFormat")
                {
                    continue;
                }

                var content = File.ReadAllText(javaFile);
                var match = codeRegex.Match(content);
                if (match.Success)
                {
                    var code = match.Groups[1].Value;
                    var csharpClass = $@"
using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{{
    public class {className} : DHCPPacket
    {{
        public const int CODE = {code};

        public {className}()
        {{
            _code = CODE;
        }}

        public {className}(AttributeList attributes)
            : base(attributes)
        {{
            _code = CODE;
        }}
    }}
}}";
                    File.WriteAllText(Path.Combine(csharpOutputPath, $"{className}.cs"), csharpClass);
                    Console.WriteLine($"Generated {className}.cs");
                }
            }
        }
    }
}
