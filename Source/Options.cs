using System;
using CommandLine;
using CommandLine.Text;

namespace javaidx
{
    /// <summary>
    /// Internal class used for the command line parsing
    /// </summary>
    internal class Options : CommandLineOptionsBase
    {
        [Option("f", "file", Required = false, DefaultValue = "", HelpText = "Java IDX file")]
        public string File { get; set; }

        [Option("d", "directory", Required = false, DefaultValue = "", HelpText = "Java IDX Cache Directory")]
        public string Directory { get; set; }

        [Option("l", "delimiter", Required = false, DefaultValue = ",", HelpText = "The delimiter used for the export. Defaults to \",\"")]
        public string Delimiter { get; set; }

        [Option("s", "sort", Required = false, DefaultValue = "", HelpText = "Sort column. Valid options are: url,length,modified,expiration,validation. Defaults to sorting by url")]
        public string Sort { get; set; }

        [Option("o", "output", Required = false, DefaultValue = "", HelpText = "Output csv file")]
        public string Output { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Copyright = new CopyrightInfo("woanware", 2013),
                AdditionalNewLineAfterOption = false,
                AddDashesToOption = true
            };

            this.HandleParsingErrorsInHelp(help);

            help.AddPreOptionsLine("Usage: javaidx -f \"CACHE FILE\" -d \"CACHE DIR\" -d \"\\t\" -s \"url\" -o \"C:\\output.csv\"");
            help.AddOptions(this);

            return help;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="help"></param>
        private void HandleParsingErrorsInHelp(HelpText help)
        {
            if (this.LastPostParsingState.Errors.Count > 0)
            {
                var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces
                if (!string.IsNullOrEmpty(errors))
                {
                    help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
                    help.AddPreOptionsLine(errors);
                }
            }
        }
    }
}
