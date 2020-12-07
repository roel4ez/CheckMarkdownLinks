using CommandLine;
using System;

namespace CheckMarkdownLinks
{
    public partial class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var exitCode = 0;

                Parser.Default.ParseArguments<Options>(args)
                            .WithParsed<Options>((o) => exitCode = RunWithOptions(o)); 

                return exitCode;          
            }
            catch (System.Exception exc)
            {
                Console.Error.WriteLine(exc.GetBaseException().Message);
                return 2;
            }           
        }

        internal static int RunWithOptions(Options obj)
        {
            Console.WriteLine($"MarkdownLinkChecker - Input: {obj.Input}");

            var inputFile = obj.Input;
            var processURL = obj.ProcessWeblinks;        

            var run = new Run();
            var result = run.ProcessInput(inputFile, processURL, obj.Verbose);

            return result;
        }    
    }
}
