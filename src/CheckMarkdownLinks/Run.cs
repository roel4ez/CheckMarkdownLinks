using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CheckMarkdownLinks.Tests")]

namespace CheckMarkdownLinks
{
    public class Run
    {
        private const string linkPattern = @"\[([^\[]+)\]\(((.*?)\))";
        //not used right now.
        private const string urlPattern = @"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$";

        private int brokenLinkCount;
        private int processedFileCount;
        private string currentDir;
        private string documentRoot;
        private bool verbose;

        internal int ProcessInput(string input, bool processUrl, bool verbose = false)
        {
            this.verbose = verbose;
            FileAttributes attr = File.GetAttributes(input);

            string [] filesToProcess;

            if (attr.HasFlag(FileAttributes.Directory)) 
            {
                filesToProcess = Directory.GetFiles(input, "*.md", SearchOption.AllDirectories);                               
            } 
            else 
            {
                filesToProcess = new string[]{input};
            }

            documentRoot = input; 
            foreach (var file in filesToProcess)
            {
                currentDir = Path.GetDirectoryName(file); 
                if (verbose)
                {
                    //verbose should actually only go to Console.Error though...
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Processing file {file}");
                    Console.WriteLine($"Current base Directory {currentDir}");
                    Console.ResetColor();
                }
                FindAndCheckAllLinksInDocument(file, processUrl);
                processedFileCount++;
            }

            Console.WriteLine($"Processed files [{processedFileCount}] - Broken links: [{brokenLinkCount}]");

            return brokenLinkCount > 0 ? 1 : 0;
        }       

        internal void FindAndCheckAllLinksInDocument(string inputFile, bool processWeblinks)
        {
            var text = File.ReadAllText(inputFile);
            var matches = Regex.Matches(text, linkPattern,RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                var link = match.Groups[3].Value;
                var type = GetLinkType(link);
                switch (type){
                    case LinkType.Anchor:
                        break;
                    case LinkType.External:
                        if (processWeblinks)
                        {
                            WriteOutValidity(inputFile, link, IsExternalLinkValid(link));
                        }
                        break;
                    case LinkType.Internal:
                        WriteOutValidity(inputFile, link, IsInternalLinkValid(link));
                        break;
                    default:
                        break; //throw?
                }                
            }
        }

        internal LinkType GetLinkType(string link)
        {
            bool isUrl = Uri.IsWellFormedUriString(link, UriKind.Absolute);
            //bug: sharepoint do not validate as Well Formed! See unit tests
            if (isUrl || (link.Contains("https://") && link.Contains("sharepoint.com")))
            {
                return LinkType.External;
            }
            else
            {
                return link.StartsWith("#") ? LinkType.Anchor : LinkType.Internal;                
            }
        }

        
        ///<summary>
        /// check if an internal link is valid.
        ///</summary>
        internal bool IsInternalLinkValid(string link)
        {            
            //Does the link contain an anchor? Not supported yet, so remove the anchor for now
            if (link.Contains("#"))
            {
                link = link.Substring(0, link.IndexOf("#"));
            }

            //if the link starts with a / it is absolute and needs to be created from the root up.
            var path = link.StartsWith("/") 
                            ? Path.Combine(documentRoot, link.TrimStart('/'))
                            : Path.Combine(currentDir, link);
                            

            //bug: this check is case-insensitive, and ADO wiki _is_ case sensitive. This potentially results in false positives.
            if (!File.Exists(path))
            {
                brokenLinkCount++;
                return false;
            }

            return true;            
        }

        ///<summary>
        /// very flawed implementation to check if a web link is valid.
        ///</summary>
        internal bool IsExternalLinkValid(string link)
        {
            //use adapted webclient to only send HEAD
            //however we also need to check for 403 (forbidden but online), or 401.
            //404 is what we care about actually.
            using (var client = new WebClientWithHeadSupport())
            {
                client.HeadOnly = true;
                try
                {
                    client.DownloadString(link);
                    return true;
                }
                catch (WebException ex) when (ex.Response is HttpWebResponse r && r.StatusCode == HttpStatusCode.NotFound)
                {     
                    //only fail on NotFound for now
                    brokenLinkCount++;
                    Debug.WriteLine(ex);
                    return false;
                }
                catch (WebException) //we might wanna check for individual status codes
                {
                    return false;
                   // var status = (ex.Response as HttpWebResponse)?.StatusCode.ToString() ?? ex.Status.ToString();
                }
            }
        }

        internal void WriteOutValidity(string inputFile, string link, bool isValid)
        {            
            if (isValid && verbose){
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{inputFile}]-[{link}]=[OK]");
                Console.ResetColor();                
                Console.WriteLine();  
            }
            if (!isValid)
            {
                Console.ForegroundColor =ConsoleColor.Red;
                Console.Write($"[{inputFile}]-[{link}]=[BROKEN]");
                Console.ResetColor();                
                Console.WriteLine();  
            }         
        }
    }
}
