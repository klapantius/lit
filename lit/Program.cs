using System;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("lit_utest")]
[assembly: InternalsVisibleTo("InternalsVisible.DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace lit
{
    class Program
    {
        static void Main(string[] args)
        {
            var configFile = System.IO.Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location) + ".xml";
            if (null != args && args.Length > 0)
            {
                configFile = args[0];
            }
            if (!File.Exists(configFile))
            {
                Console.WriteLine("Error: could not find configuration file \"{0}\"", configFile);
            }

            Console.ReadKey();
        }
    }
}
