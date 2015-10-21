using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("lit_utest")]
[assembly: InternalsVisibleTo("InternalsVisible.DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace lit
{
    class Program
    {
        static void Main(string[] args)
        {
            var configFile = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location) + ".xml";
            if (null != args && args.Length > 0)
            {
                configFile = args[0];
            }
            if (!File.Exists(configFile))
            {
                Console.WriteLine("Error: could not find configuration file \"{0}\"", configFile);
                return;
            }

            var configuration=new Configuration(configFile);
            if (!File.Exists(configuration.InputFile))
            {
                Console.WriteLine("Error: could not find input file \"{0}\"", configFile);
                return;
            }
            var tail = new Tail(configuration.InputFile, Encoding.Default);
            var parser = new Parser(tail, configuration);
            var transfer = new HttpTransferModule(configuration);
            parser.Changed += transfer.ReceiveChanges;
            parser.Run();

            Console.ReadKey();
        }
    }
}
