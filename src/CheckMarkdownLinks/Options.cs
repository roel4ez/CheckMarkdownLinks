using CommandLine;

namespace CheckMarkdownLinks
{
    public partial class Program
    {
        public class Options
        {
            [Option('i', "input", Required = true, HelpText = "Input file")]
            public string Input { get; set; }

            [Option('w', "web", Required = false, HelpText = "Enable processing of web urls", Default = false)]
            public bool ProcessWeblinks { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Verbose logging", Default = false)]
            public bool Verbose { get; set; }

        }
    }
}
