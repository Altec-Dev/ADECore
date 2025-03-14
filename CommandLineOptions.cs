/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

using CommandLine;

namespace ADEcore
{
    public class CommandLineOptions
    {
        #region <Parser mode>
        [Verb("parse", HelpText = "Start programme in PDF-Parse mode")]
        public class ParseModeOptions
        {
            [Option('d', "directory", Required = true, HelpText = "Directory with the business partners INI files")]
            public string Directory { get; set; }

            [Option('f', "file", Required = true, HelpText = "Text file to parse")]
            public string File { get; set; }
        }
        #endregion </Parser mode>

        #region <Server mode>
        [Verb("server", HelpText = "Start programme in server mode")]
        public class ServerModeOptions
        {
            [Option('m', "monitoredDirectory", Required = true, HelpText = "Root directory to be monitored for new PDF files")]
            public string MonitiredDirectory { get; set; }

            [Option('p', "popplerDirectory", Required = true, HelpText = "Directory that can be accessed by the PDF tool poppler")]
            public string PopplerDirectory { get; set; }

            [Option('b', "bpDirectory", Required = true, HelpText = "Directory with the Business Partners ini files")]
            public string BpDirectory { get; set; }

            [Option('i', "checkIntervall", Required = true, HelpText = "Check interval every n seconds.")]
            public int CheckIntervall { get; set; }


            [Option('e', "emailMode", Required = false, HelpText = "Emails will be sent automatically to this address based on the last directory name listed")]
            public bool EmailMode { get; set; }

            [Option('d', "Domain", Required = false, HelpText = "The domain to which the emails are to be sent. Assumes emailMode true")]
            public string Domain { get; set; }

            [Option('s', "smtpServer", Required = false, HelpText = ". Assumes emailMode true")]
            public string SmtpServer { get; set; }

            [Option('o', "smtpPort", Required = false, HelpText = ". Assumes emailMode true")]
            public string SmtpPort { get; set; }

            [Option('r', "smtpSender", Required = false, HelpText = ". Assumes emailMode true")]
            public string SmtpSender { get; set; }

            [Option('u', "smtpUser", Required = false, HelpText = ". Assumes emailMode true")]
            public string SmtpUser { get; set; }

            [Option('w', "smtpPassword", Required = false, HelpText = ". Assumes emailMode true")]
            public string SmtpPassword { get; set; }

            [Option('j', "smtpSubject", Required = false, HelpText = ". Assumes emailMode true")]
            public string SmtpSubject { get; set; }

            [Option('l', "smtpEnableSSL", Required = false, HelpText = ". Assumes emailMode true")]
            public bool SmtpEnableSSL { get; set; }


            [Option('a', "smtpAdmin", Required = false, HelpText = ". Assumes emailMode true")]
            public string SmtpAdmin { get; set; }


            [Option('f', "LogFile", Required = true, HelpText = "The full file name where the events will be logged")]
            public string LogFile { get; set; }

            [Option('t', "timePerFile", Required = true, HelpText = "The maximum waiting time in sec. for poppler until an error will be thrown")]
            public int timePerFile { get; set; }
        }
        #endregion </Server mode>

    }
}
