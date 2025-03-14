/*
 * Copyright (c) 2023 Ulf-Dirk Stockburger
*/

using CommandLine;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

/*
 * Settings
 * --------
 * 
 * poppler home directory: "\\wsl.localhost\Ubuntu\home\linux\poppler\"
 * Monitord directory local: "...\MonitoredDirectory"
 * Monitord directory unc: "...\ADECore\"
 * Business partner directory: "...\BpDir\"
 * Logfile: "...\ADECore.log"
 * 
 * Smtp Server = "x.x.x.x"
 * Smtp Port = 25;
 * Domain: ...
*/

namespace ADEcore
{
    internal class Program
    {
        #region <Development>
        static string iniFilename = string.Empty;
        static string pdfFilename = string.Empty;
        static string bpDirectory = string.Empty;
        static string monitoredDirectory = string.Empty;
        static string popplerDirectory = string.Empty;
        static string logFile = string.Empty;

        #region <ServerMode>
        static bool canExit = false;
        static bool onExit = false;
        static bool onWork = false;
        #endregion </ServerMode>

        #endregion </Development>

        #region <Consts>
        static string progDir = string.Empty;
        #endregion </Consts>

        #region <Dictionary>
        private static List<Dictionary<string, string>> dict = new List<Dictionary<string, string>>();
        #endregion </Dictionary>

        #region <Global Arrays>
        // Verified ini sections and keys
        private static List<List<BOMParams>> bomRowsValues = new List<List<BOMParams>>();
        private static List<UdfParams> udfValues = new List<UdfParams>();
        private static List<string> bomFormulas = new List<string>();
        private static List<string> udfFormulas = new List<string>();

        // The final variable list
        private static List<Variable> variables = new List<Variable>();
        #endregion </Global Arrays>

        #region <Error handling>
        public static Helpers helpers = new();
        #endregion </Error handling>

        #region <Output Files>
        public static List<SourceDestinationFile> OutputFiles { get; set; } = new List<SourceDestinationFile>();
        #endregion </Output Files>

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions.ParseModeOptions, CommandLineOptions.ServerModeOptions>(args)
                .MapResult(
                    (CommandLineOptions.ParseModeOptions opts) => RunInParseMode(opts),
                    (CommandLineOptions.ServerModeOptions opts) => RunInServerMode(opts),
                    errs => 1);
        }

        // 

        private static int RunInServerMode(CommandLineOptions.ServerModeOptions opts)
        {
            //
            int InfoLineNumber = 5;
            RetValues retVal = new RetValues();
            //Helpers helpers = new Helpers();

            // FileStream to the log file
            FileStream logFileStream = null;

            try
            {
                #region <Events>
                // Event Handler for CancelKeyPress
                Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPressHandler);
                #endregion </Events>

                #region <Press 'Ctrl+End'>
                string header = @"###########################################"
                                + Environment.NewLine + @"ADECore " + Consts.appVersion
                                + Environment.NewLine + @"Press 'Ctrl+End' to terminate the programme"
                                + Environment.NewLine + @"###########################################";
                #endregion </Press 'Ctrl+End'>

                #region <FileStream to the log file in mode append>
                logFileStream = System.IO.File.Open(opts.LogFile, FileMode.Append, FileAccess.Write, FileShare.None);
                #endregion </FileStream to the log file in mode append>

                #region <Mutex>
                bool oMutex = false;
                System.Threading.Mutex mutex = new System.Threading.Mutex(true, Consts.mutexADEcoreServer, out oMutex);
                if (!oMutex)
                {
                    helpers.SetError(1005000, @"The programme can only be started once in server mode", ref logFileStream, opts, string.Empty);
                    return 1;
                }
                #endregion <Mutex>

                #region <Check Directories>

                // Check if the given directory exists
                if (!retVal.error && !System.IO.Directory.Exists(opts.MonitiredDirectory))
                {
                    helpers.SetError(1005001, @"Monitored directory not exists: " + opts.MonitiredDirectory, ref logFileStream, opts, opts.SmtpAdmin);
                    return 1;
                }
                monitoredDirectory = opts.MonitiredDirectory;

                // Check if the given directory exists
                if (!retVal.error && !System.IO.Directory.Exists(opts.PopplerDirectory))
                {
                    helpers.SetError(1005002, @"Poppler directory not exists: " + opts.PopplerDirectory, ref logFileStream, opts, opts.SmtpAdmin);
                    return 1;
                }
                popplerDirectory = opts.PopplerDirectory;

                // Check if the given directory exists
                if (!retVal.error && !System.IO.Directory.Exists(opts.BpDirectory))
                {
                    helpers.SetError(1005003, @"Poppler directory not exists: " + opts.BpDirectory, ref logFileStream, opts, opts.SmtpAdmin);
                    return 1;
                }
                bpDirectory = opts.BpDirectory;

                #endregion </Check Directories>

                #region <Check EmailMode>

                if (!retVal.error && opts.EmailMode)
                {
                    if (string.IsNullOrEmpty(opts.Domain))
                    {
                        helpers.SetError(1005002, @"No domain specified for sending e-mails", ref logFileStream, opts, opts.SmtpAdmin);
                        return 1;
                    }
                    if (!opts.Domain.StartsWith(@"@"))
                    {
                        opts.Domain = @"@" + opts.Domain;
                    }

                    if (string.IsNullOrEmpty(opts.SmtpServer))
                    {
                        helpers.SetError(1005024, @"No SMTP server specified for sending e-mails", ref logFileStream, opts, opts.SmtpAdmin);
                        return 1;
                    }

                    if (string.IsNullOrEmpty(opts.SmtpPort))
                    {
                        helpers.SetError(1005028, @"No SMTP port specified for sending e-mails", ref logFileStream, opts, opts.SmtpAdmin);
                        return 1;
                    }

                    if (string.IsNullOrEmpty(opts.SmtpSubject))
                    {
                        helpers.SetError(1005027, @"No SMTP subject specified for sending e-mails", ref logFileStream, opts, opts.SmtpAdmin);
                        return 1;
                    }

                    if (string.IsNullOrEmpty(opts.SmtpSender))
                    {
                        helpers.SetError(1005029, @"No SMTP sender specified for sending e-mails", ref logFileStream, opts, opts.SmtpAdmin);
                        return 1;
                    }

                    if (string.IsNullOrEmpty(opts.SmtpAdmin))
                    {
                        helpers.SetError(1005030, @"No SMTP adminstrators email specified for sending e-mails", ref logFileStream, opts, opts.SmtpAdmin);
                        return 1;
                    }
                }

                #endregion </Check EmailMode>

                #region <Loop every n sec.>
                // From msec. to sec.
				int checkIntervall = opts.CheckIntervall * 1000;
                int loppCounter = 0;

                do
                {
                    List<UnprocessedFiles> unprocessedFiles = new List<UnprocessedFiles>();

                    #region <Check if the programme should be terminate>
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                        if (key.Key == ConsoleKey.End && (key.Modifiers & ConsoleModifiers.Control) != 0)
                        {
                            Extensions.ResetConsole(header);
                            Extensions.ConsoleWriteLineAt(InfoLineNumber, "Ctrl+End: The programme will be terminated");
                            System.Threading.Thread.Sleep(2000);
                            break;
                        }
                    }
                    #endregion </Check if the programme should be terminated>

                    #region <Message to the terminal and sleep n sec.>
                    DateTime currentDateTime = DateTime.Now.AddSeconds(opts.CheckIntervall);
                    DateTime roundedTime = new DateTime((currentDateTime.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);

                    string nextCheckStr = @"Next check on " + roundedTime.Date.ToString("d") + " at " + roundedTime.Hour.ToString("D2") + ":" + roundedTime.Minute.ToString("D2") + ":" + roundedTime.Second.ToString("D2");

                    Extensions.ResetConsole(header);
                    Extensions.ConsoleWriteLineAt(InfoLineNumber, "Counter " + loppCounter.ToString() + @" - " + nextCheckStr);

                    ++loppCounter;
                    System.Threading.Thread.Sleep(checkIntervall);
                    #endregion </Message to the terminal and sleep n sec.>

                    #region <Get a ist of all PDF files, orderd by date desc>
                    if (!retVal.error)
                    {
                        string fileType = @"*." + Consts.ParseFilesNew;

                        #region <Get a list of all files recursiv>
                        var files = Directory.EnumerateFiles(opts.MonitiredDirectory, fileType, SearchOption.AllDirectories)
                            .Where(file =>
                            {
                                var attribute = System.IO.File.GetAttributes(file);
                                return !attribute.HasFlag(FileAttributes.Hidden) && !attribute.HasFlag(FileAttributes.System);
                            })
                            .Select(file => new FileInfo(file))
                            .OrderByDescending(file => file.CreationTime)
                            .ThenByDescending(file => file.LastWriteTime)
                            .ToList();
                        #endregion </Get a list of all files recursiv>

                        #region <Create a list of all file names found>
                        foreach (var file in files)
                        {
                            UnprocessedFiles uFile = new UnprocessedFiles();
                            // Check if email is used
                            if (opts.EmailMode)
                            {
                                string[] tmp = System.IO.Path.GetFullPath(file.DirectoryName).Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                                uFile.email = tmp[tmp.Length - 1] + opts.Domain;
                            }

                            uFile.OrgFilename = file.FullName;
                            uFile.Guid = Guid.NewGuid().ToString();

                            uFile.NewFilename = opts.PopplerDirectory;
                            if (!uFile.NewFilename.EndsWith(Path.DirectorySeparatorChar))
                            {
                                uFile.NewFilename = uFile.NewFilename + Path.DirectorySeparatorChar;
                            }
                            uFile.NewFilename = uFile.NewFilename + uFile.Guid + System.IO.Path.GetExtension(uFile.OrgFilename);

                            unprocessedFiles.Add(uFile);
                        }
                        #endregion </Create a list of all file names found>

                    }
                    #endregion </Get a ist of all PDF files, orderd by date desc>

                    #region <Loop over al unprocessed files>
                    foreach (UnprocessedFiles file in unprocessedFiles)
                    {

                        #region <Check if the file exists>
                        if (!System.IO.File.Exists(file.OrgFilename))
                        {
                            // If the file not exists, so go to the next one in the list
                            continue;
                        }
                        #endregion </Check if the file exists>

                        #region <Check if the file is already inprocess, finished or error>
                        if (System.IO.File.Exists(file.OrgFilename + @"." + Consts.ParseFilesInProcess)
                            || System.IO.File.Exists(file.OrgFilename + @"." + Consts.ParseFilesFinished)
                            || System.IO.File.Exists(file.OrgFilename + @"." + Consts.ParseFilesError))
                        {
                            try
                            {
                                System.IO.File.Delete(file.OrgFilename);
                                continue;
                            }
                            catch (Exception ex)
                            {
                                helpers.SetError(1005004, ex.Message, ref logFileStream, opts, file.email);
                                return 1;
                            }
                        }
                        #endregion </Check if the file is already inprocess, finished or error>

                        #region <Copy the PDF files to the poppler directory and mark the files as in progress>
                        try
                        {
                            #region <Copy the original PDF to the Linux share>
                            // Try to open the file exclusively
                            using (FileStream fileStream = System.IO.File.Open(file.OrgFilename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                            {
                                // Check if the destination file exists
                                if (System.IO.File.Exists(file.NewFilename))
                                {
                                    continue;
                                }

                                // Write the opened straam into a new file
                                using (FileStream newStream = System.IO.File.Create(file.NewFilename))
                                {
                                    // Copy the stream to the new file
                                    fileStream.CopyTo(newStream);
                                }
                            }
                            #endregion </Copy the original PDF to the Linux share>

                            #region <Rename the original PDF>
                            System.IO.File.Move(file.OrgFilename, file.OrgFilename + @"." + Consts.ParseFilesInProcess);
                            #endregion </Rename the original PDF>

                        }
                        catch (IOException ex)
                        {
                            // The file is locked, so go to the next one in the list
                            helpers.SetError(1005017, ex.Message, ref logFileStream, opts, file.email);
                            continue;
                        }
                        #endregion </<Copy the PDF files to the poppler directory and mark the files as in progress>>

                        #region <Check if the file exists>
                        {
                            bool fExists = false;
                            DateTime startTime = DateTime.Now;
                            do
                            {
                                if (!System.IO.File.Exists(file.NewFilename + @"." + Consts.ParseFilesText))
                                {
                                    // If the file not exists, so go to the next one in the list
                                    System.Threading.Thread.Sleep(100);
                                    if ((DateTime.Now - startTime).TotalSeconds > opts.timePerFile)
                                    {
                                        string tFilename = file.OrgFilename.Replace(opts.MonitiredDirectory, string.Empty);
                                        helpers.SetError(1005031, @"It looks like poppler has problems: " + tFilename, ref logFileStream, opts, file.email);
                                        #region <Rename the in process file>
                                        System.IO.File.Move(file.OrgFilename + @"." + Consts.ParseFilesInProcess, file.OrgFilename + "." + Consts.ParseFilesError);
                                        #endregion </Rename the in process file>
                                        break;
                                    }
                                }
                                else
                                {
                                    fExists = true;
                                }
                            } while (!fExists);

                            if (!fExists)
                            {
                                continue;
                            }
                        }
                        #endregion </Check if the file exists>

                        #region <Check is file is free>
                        {
                            bool fFree = false;
                            do
                            {
                                try
                                {
                                    using (FileStream newStream = System.IO.File.Create(file.NewFilename))
                                    {
                                        fFree = true;
                                    }
                                }
                                catch (IOException ex)
                                {
                                    System.Threading.Thread.Sleep(100);
                                }
                            } while (!fFree);
                        }
                        #endregion </Check is file is free>

                        #region <Start ADEcore in parse mode and wait for finish>
                        {
                            string exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
                            string ext = System.IO.Path.GetExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);

                            string exeProg = exe.Substring(0, exe.Length - (ext.Length - 1)) + @"exe";

                            // parse -f "W:\_VS\ADEcore\ADEcore\__debug\Examples\1.txt" -d "W:\_VS\ADEcore\ADEcore\__debug\BpDir"
                            string parseParam = "parse -f \"" + (file.NewFilename + @"." + Consts.ParseFilesText) + "\" -d \"" + opts.BpDirectory + "\"";

                            try
                            {
                                using (Process process = new Process())
                                {
                                    process.StartInfo.FileName = exeProg;
                                    process.StartInfo.Arguments = parseParam;
                                    process.StartInfo.UseShellExecute = false;
                                    process.StartInfo.CreateNoWindow = true;
                                    process.StartInfo.RedirectStandardError = true;
                                    process.StartInfo.RedirectStandardOutput = true;
                                    process.Start();

                                    process.WaitForExit(opts.timePerFile * 1000); // In seconds

                                    if (!process.HasExited)
                                    {
                                        helpers.SetError(1005005, @"It looks like poppler has problems", ref logFileStream, opts, file.email);
                                        continue;
                                    }

                                    int exitCode = process.ExitCode;
                                    if (exitCode != 0)
                                    {
                                        #region <Rename the inprogress PDF to error>
                                        try
                                        {
                                            System.IO.File.Move(file.OrgFilename + @"." + Consts.ParseFilesInProcess, file.OrgFilename + @"." + Consts.ParseFilesError);
                                        }
                                        catch (Exception ex)
                                        {
                                            helpers.SetError(1005018, ex.Message, ref logFileStream, opts, file.email);
                                        }
                                        #endregion </Rename the inprogress PDF to error>

                                        #region <Copy the translated PDF into the monitired directory>
                                        try
                                        {
                                            System.IO.File.Move(file.NewFilename + @"." + Consts.ParseFilesText, file.OrgFilename + @"." + Consts.ParseFilesText);
                                        }
                                        catch (Exception ex)
                                        {
                                            helpers.SetError(1005019, ex.Message, ref logFileStream, opts, file.email);
                                        }
                                        #endregion </Copy the translated PDF into the monitired directory>

                                        #region <Delete the tmp PDF file in the poppler directory>
                                        try
                                        {
                                            System.IO.File.Delete(file.NewFilename);
                                        }
                                        catch (Exception ex)
                                        {
                                            helpers.SetError(1005019, ex.Message, ref logFileStream, opts, file.email);
                                        }
                                        #endregion </Delete the tmp PDF file in the poppler directory>

                                        #region <Create the error message>
                                        helpers.ErrorFileName = file.OrgFilename.Substring(opts.MonitiredDirectory.Length);
                                        string errorMessage = process.StandardOutput.ReadToEnd();
                                        errorMessage = errorMessage.Replace(@"-----", @"<Parser>");
                                        errorMessage = errorMessage + Environment.NewLine + @"</Parser>";
                                        helpers.SetError(exitCode, errorMessage, ref logFileStream, opts, file.email);
                                        Extensions.ConsoleWriteLineAt(InfoLineNumber, errorMessage);
                                        #endregion </Create the error message>

                                        continue;
                                    }
                                    else
                                    {

                                        #region <Rename the inprogress PDF to finished>
                                        try
                                        {
                                            System.IO.File.Move(file.OrgFilename + @"." + Consts.ParseFilesInProcess, file.OrgFilename + @"." + Consts.ParseFilesFinished);
                                        }
                                        catch (Exception ex)
                                        {
                                            helpers.SetError(1005020, ex.Message, ref logFileStream, opts, file.email);
                                        }
                                        #endregion </Rename the inprogress PDF to finished>

                                        #region <Delete the tmp PDF file in the poppler directory>
                                        try
                                        {
                                            System.IO.File.Delete(file.NewFilename);
                                            System.IO.File.Delete(file.NewFilename + @"." + Consts.ParseFilesText);
                                        }
                                        catch (Exception ex)
                                        {
                                            helpers.SetError(1005021, ex.Message, ref logFileStream, opts, file.email);
                                        }
                                        #endregion </Delete the tmp PDF file in the poppler directory>

                                        #region <Create the error message>
                                        helpers.ErrorFileName = file.OrgFilename.Substring(opts.MonitiredDirectory.Length);
                                        string successMessage = process.StandardOutput.ReadToEnd(); // .Replace(Environment.NewLine, @" / ");
                                        successMessage = @"<Parser>" + Environment.NewLine + successMessage + Environment.NewLine;
                                        successMessage = successMessage + @"</Parser>";
                                        string msg = successMessage;
                                        helpers.SetError(exitCode, msg, ref logFileStream, opts, file.email);
                                        Extensions.ConsoleWriteLineAt(InfoLineNumber, msg);
                                        #endregion </Create the error message>

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                helpers.SetError(1005016, ex.Message, ref logFileStream, opts, file.email);
                                //Console.WriteLine($"Ein Fehler ist aufgetreten: {ex.Message}");
                            }
                        }
                        #endregion </Start ADEcore in parse mode and wait for finish>

                    }
                    #endregion </Loop over al unprocessed files>

                } while (true);
            }
            catch (Exception ex)
            {
                // Error related to the log file
                helpers.SetError(1005015, ex.Message, ref logFileStream, opts, opts.SmtpAdmin);
                Console.WriteLine(ex.Message);
                Console.WriteLine(@"The programme will be terminated");
                System.Threading.Thread.Sleep(2500);
            }
            finally
            {
                if (logFileStream != null)
                {
                    logFileStream.Close();
                }
            }

            #endregion </Loop every n sec.>

            return 0;
        }

        private static int RunInParseMode(CommandLineOptions.ParseModeOptions opts)
        {
            #region <Stopwatch>
            Stopwatch stopwatch = Stopwatch.StartNew();
            #endregion </Stopwatch>

            RetValues retVal = new RetValues();

            Checks check = new Checks();
            Files files = new Files();

            #region <Check file and directory>
            // Check if the given file exists
            if (!retVal.error && !System.IO.File.Exists(opts.File))
            {
                helpers.SetError(ref retVal, 1005007, @"File not exists: " + opts.File);
            }
            // Check if the given directory exists
            if (!retVal.error && !System.IO.Directory.Exists(opts.Directory))
            {
                helpers.SetError(ref retVal, 1005008, @"Directory not exists: " + opts.Directory);
            }
            #endregion </Check file and directory>

            if (!retVal.error)
            {

                bpDirectory = opts.Directory;
                pdfFilename = opts.File;

                helpers.ErrorFileName = pdfFilename;

                Ini ini = new Ini();

                #region <System>

                #region <Sets the culture info for separators in numbers to en-us> 
                CultureInfo cInfo = new CultureInfo("en-us");
                cInfo.NumberFormat.NumberDecimalSeparator = ".";
                Thread.CurrentThread.CurrentCulture = cInfo;
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
                #endregion </Sets the culture info for separators in numbers to en-us>

                #region <Sets the console title and gets the handle>
                Console.Title = Consts.consoleTitle + @" " + Consts.appVersion;
                #endregion </Sets the console title and gets the handle>

                #region <Get the current directory from which the program was started>
                progDir = Directory.GetCurrentDirectory();
                #endregion </Get the current directory from which the program was started>

                #region <Get the ini file to the current document if exists>
                check.CheckDocumentsIni(ref retVal, bpDirectory, pdfFilename, ref iniFilename);
                if (retVal.error)
                {
                    helpers.ExitWithErrorCode(retVal);
                }
                #endregion </Get the ini file to the current document if exists>

                #region <Create a new Ini instance and get all ini sections>
                ini = new Ini(ref retVal, iniFilename);
                if (retVal.error)
                {
                    helpers.ExitWithErrorCode(retVal);
                }
                #endregion </Create a new Ini instance and get all ini sections>

                #region <Insert the system variables into the variable list>

                #region <Id>
                {
                    Variable variable = new Variable();
                    variable.Row = -1;
                    variable.WhereTo = Enums.VariableAssignedTo.system;
                    variable.Name = @"Id";
                    variable.Value = ini.DocParams.Id;
                    variable.Type = Enums.VariableType.str;
                    variables.Add(variable);
                }
                #endregion </Id>

                #region <Name>
                {
                    Variable variable = new Variable();
                    variable.Row = -1;
                    variable.WhereTo = Enums.VariableAssignedTo.system;
                    variable.Name = @"Name";
                    variable.Value = ini.DocParams.Name;
                    variable.Type = Enums.VariableType.str;
                    variables.Add(variable);
                }
                #endregion </Name>

                #region <Type>
                {
                    Variable variable = new Variable();
                    variable.Row = -1;
                    variable.WhereTo = Enums.VariableAssignedTo.system;
                    variable.Name = @"Type";
                    variable.Value = ini.DocParams.Type;
                    variable.Type = Enums.VariableType.str;
                    variables.Add(variable);
                }
                #endregion </Type>

                #region <Date / Time>
                {
                    DateTime now = DateTime.Now;
                    {
                        Variable variable = new Variable();
                        variable.Row = -1;
                        variable.WhereTo = Enums.VariableAssignedTo.system;
                        variable.Name = @"CheckDate";
                        variable.Value = now.ToString("yyyy.MM.dd");
                        variable.Type = Enums.VariableType.str;
                        variables.Add(variable);
                    }
                    {
                        Variable variable = new Variable();
                        variable.Row = -1;
                        variable.WhereTo = Enums.VariableAssignedTo.system;
                        variable.Name = @"CheckTime";
                        variable.Value = now.ToString("HH:mm:ss");
                        variable.Type = Enums.VariableType.str;
                        variables.Add(variable);
                    }
                }
                #endregion </Date / Time>

                #region <Client>
                {
                    string client = Path.GetFileName(Path.GetDirectoryName(pdfFilename) ?? string.Empty);
                    Variable variable = new Variable();
                    variable.Row = -1;
                    variable.WhereTo = Enums.VariableAssignedTo.system;
                    variable.Name = @"Client";
                    variable.Value = variable.Value = client;
                    variable.Type = Enums.VariableType.str;
                    variables.Add(variable);
                }
                #endregion </Client>

                #endregion <Insert the system variables into the variable list>

                #endregion </System>

                #region <Document>

                #region <Get the variables and values to the given document>
                GetDocumentInformationAndWriteOutputFile(ref retVal, ref ini, pdfFilename);
                #endregion </Get the variables and values to the given document>

                #endregion </Document>

            }

            #region <Stopwatch>
            stopwatch.Stop();
            var stopwatchTime = stopwatch.Elapsed;
            string stopwatchResult = $"{stopwatchTime.Minutes:D2}:{stopwatchTime.Seconds:D2}:{stopwatchTime.Milliseconds:D3}";
            retVal.msg = @"Time needed " + stopwatchResult;
            #endregion </Stopwatch>

            #region <Output file>
            string replacement = @"\\" + GetDomainName() + @"\output";
            string needle = @"C:\ADECore\OutputDir";

            retVal.msg = retVal.msg + Environment.NewLine + "File saved as:";

            for (int i = 0; i < OutputFiles.Count; i++)
            {
                if (i < OutputFiles.Count)
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + OutputFiles[i].Destination.Replace(needle, replacement);
            }
            #endregion </Output file>

            #region <Exit programme with error code 0>
            helpers.ExitWithErrorCode(retVal);
            #endregion </Exit programme with error code 0>

            return 0;

        }

        // Get the variables and values to the given document
        private static void GetDocumentInformationAndWriteOutputFile(ref RetValues retVal, ref Ini ini, string pdfFilename)
        {
            Checks check = new Checks();
            Files files = new Files();

            #region <Gobal main values>
            bool useBom = true;
            #endregion </Gobal main values>

            #region <Read the text file in UTF-8 to check in an array>
            string[] txtFileArray = null;
            files.ReadTxtFileIntoArray(ref retVal, ref txtFileArray, pdfFilename);
            if (retVal.error)
            {
                helpers.SetError(ref retVal, 1005009, retVal.msg);
                return;
            }
            #endregion </Read the text file in UTF-8 to check in an array>

            #region <BOM>

            #region <Check if BOMArea is used>
            if (ini.BOMArea.Start == string.Empty)
            {
                useBom = false;
            }
            #endregion </Check if BOMArea is used>

            #region <Get all BOM rows and check the values if BOMArea is used>
            if (useBom)
            {
                check.CheckDocumentsBOM(ref retVal, txtFileArray, ref ini, ref bomRowsValues);
                if (retVal.error)
                {
                    helpers.ExitWithErrorCode(retVal);
                }
            }
            #endregion </Get all BOM rows and check the values if BOMArea is used>

            #endregion </BOM>

            #region <UDF>

            #region <Check all UdfParams>
            string pdfTxtFile = System.IO.File.ReadAllText(pdfFilename, System.Text.Encoding.UTF8);
            check.CheckDocumentsUdf(ref retVal, pdfTxtFile, ref ini, ref udfValues);
            if (retVal.error)
            {
                helpers.ExitWithErrorCode(retVal);
            }
            #endregion </Check all UdfParams>

            #endregion </UDF>

            #region <Variable list>

            #region <BOM>
            // Loop over all BOM rows
            for (int i = 0; i < bomRowsValues.Count; i++)
            {
                // Loop over all comumns in the current row
                for (int j = 0; j < bomRowsValues[i].Count; j++)
                {
                    Variable variable = new Variable();
                    variable.Row = i;
                    variable.Name = bomRowsValues[i][j].VarName;
                    variable.Value = bomRowsValues[i][j].Value;
                    variable.WhereTo = Enums.VariableAssignedTo.bom;
                    if (Convert.ToInt32(bomRowsValues[i][j].IsNumber) == 1)
                    {
                        variable.Type = Enums.VariableType.dbl;
                    }
                    else if (Convert.ToInt32(bomRowsValues[i][j].IsNumber) == 0)
                    {
                        variable.Type = Enums.VariableType.str;
                    }
                    variables.Add(variable);
                }

            }
            #endregion </BOM>

            #region <UDF>
            // Loop over all rows in the current row
            for (int i = 0; i < udfValues.Count; i++)
            {
                Variable variable = new Variable();
                variable.Name = udfValues[i].VarName;
                variable.Value = udfValues[i].Value;
                variable.WhereTo = Enums.VariableAssignedTo.udf;
                if (Convert.ToInt32(udfValues[i].IsNumber) == 1)
                {
                    variable.Type = Enums.VariableType.dbl;
                }
                else if (Convert.ToInt32(udfValues[i].IsNumber) == 0)
                {
                    variable.Type = Enums.VariableType.str;
                }
                variables.Add(variable);
            }
            #endregion </UDF>

            #endregion </Variable list>

            #region <formulas>

            // First: The order is very important, as calculated values from UDFs can be used in assembly positions.
            #region <Get the udf formulas>
            check.GetFormulasFromUDFs(ref retVal, ref udfFormulas, ini, udfValues);
            if (retVal.error)
            {
                helpers.ExitWithErrorCode(retVal);
            }
            #endregion </Get the udf formulas>

            #region <Solve UDF formulas>
            check.SolveUdfformulas(ref retVal, ref variables, udfFormulas);
            if (retVal.error)
            {
                helpers.ExitWithErrorCode(retVal);
            }
            #endregion </Solve UDF formulas>

            // Second: The order is very important, as calculated values from UDFs can be used in assembly positions.
            #region <Get the formulas for the BOM>
            check.GetFormulasFromBOMRows(ref retVal, ref bomFormulas, ini, bomRowsValues, variables);
            if (retVal.error)
            {
                helpers.ExitWithErrorCode(retVal);
            }
            #endregion </Get the formulas for the BOM>

            #region <Solve BOM formulas>
            if (bomFormulas.Count > 0)
            {
                if (@"(" + bomFormulas[0] + @")" != Consts.regExNoBomCheck)
                {
                    check.SolveBomformulas(ref retVal, bomFormulas);
                    if (retVal.error)
                    {
                        helpers.ExitWithErrorCode(retVal);
                    }
                }
            }
            #endregion </Solve BOM formulas>

            #endregion </formulas>

            #region <Set default values to empty variables>
            foreach (Variable var in variables)
            {
                if (string.IsNullOrEmpty(var.Value))
                {
                    if (var.Type == Enums.VariableType.str)
                    {
                        var.Value = ini.SysParams.DefValueString;
                    }
                    else if (var.Type == Enums.VariableType.dbl)
                    {
                        var.Value = ini.SysParams.DefValueDouble;
                    }
                }
            }
            #endregion </Set default values to empty variables>

            #region <Run scripts>
            // Check if script is used
            if (!string.IsNullOrEmpty(ini.SysParams.ScriptLanguage))
            {
                Scripting scripting = new Scripting(ini, variables);

                // Run scripts if necessary
                scripting.RunScripts(ref retVal);
                if (retVal.error)
                {
                    helpers.ExitWithErrorCode(retVal);
                }
            }
            #endregion </Run scripts>

            #region <Test scripts>
            TestScripts(variables);
            #endregion </Test scripts>

            #region <Write the output file>
            PrepareAndSaveDocuments(ref retVal, variables, ini.OutputFiles, ini);
            if (retVal.error)
            {
                helpers.ExitWithErrorCode(retVal);
            }
            #endregion </Write the output file>
        }

        // Prepare and save the output files
        private static void PrepareAndSaveDocuments(ref RetValues retVal, List<Variable> variables, List<SourceDestinationFile> files, Ini ini)
        {
            // Exit if no files exists
            if (files.Count == 0)
            {
                return;
            }

            RegExes regExes = new RegExes();

            string method = MethodBase.GetCurrentMethod().Name + @" / ";

            try
            {
                #region <Check if current file exists>
                foreach (SourceDestinationFile file in files)
                {
                    // Check if the destination filename use variables
                    if (regExes.CheckIfRegExMatch(ref retVal, file.Destination, Consts.RegExVar))
                    {
                        string destFilename = file.Destination;

                        // Loop over all variables unequal bom rows
                        foreach (Variable variable in variables)
                        {
                            if (variable.WhereTo != Enums.VariableAssignedTo.bom)
                            {
                                // Replace the variables name with the variables value
                                destFilename = destFilename.Replace(Consts.VarStart + variable.Name + Consts.VarEnd, variable.Value);
                            }
                        }

                        // Return the solved destination filename
                        file.Destination = destFilename;
                    }

                    // Check if the current destination file exists
                    if (System.IO.File.Exists(file.Destination))
                    {
                        helpers.SetError(ref retVal, 1005023, method + @"At least one of the files to be created already exists");
                        return;
                    }
                }
                #endregion </Check if current file exists>

                #region <Prepare and save the output files>
                foreach (SourceDestinationFile file in files)
                {
                    // Read the original content from the template
                    string content = System.IO.File.ReadAllText(file.Source, System.Text.Encoding.UTF8);

                    #region <BOM>
                    if (bomRowsValues.Count > 0)
                    {
                        // Check if a BomRow section exists
                        if (regExes.CheckIfRegExMatch(ref retVal, content, Consts.RegExBomRow))
                        {
                            string bomRows = string.Empty;

                            // Get the maximum row counter from variables
                            int maxRow = variables.Max(var => var.Row);
                            if (maxRow < 0)
                            {
                                helpers.SetError(ref retVal, 1005010, method + @"BOM rows not found.");
                            }

                            // Get the BomRow template and replace RegEx identifier
                            string rowTemplate = regExes.GetRegExMatch(ref retVal, content, Consts.RegExBomRow);
                            string replaceBomRow = rowTemplate;

                            rowTemplate = regExes.ReplaceRexExPattern(ref retVal, rowTemplate, Consts.RegExBomRowStart, string.Empty);
                            rowTemplate = regExes.ReplaceRexExPattern(ref retVal, rowTemplate, Consts.RegExBomRowEnd, string.Empty);

                            // Loop over all variables in BOM rows
                            for (int i = 0; i < maxRow + 1; i++)
                            {
                                bomRows = bomRows + rowTemplate;
                                foreach (Variable variable in variables)
                                {
                                    if (variable.Row == i)
                                    {
                                        bomRows = bomRows.Replace(Consts.VarStart + variable.Name + Consts.VarEnd, variable.Value);
                                    }
                                }
                            }
                            // Replace the template with the found BomRows
                            content = regExes.ReplaceRexExPattern(ref retVal, content, replaceBomRow, bomRows);
                        }
                    }
                    #endregion </BOM>

                    #region <UDF>
                    // Loop over all variables unequal bom rows
                    foreach (Variable variable in variables)
                    {
                        if (variable.WhereTo != Enums.VariableAssignedTo.bom)
                        {
                            // Replace the variables name with the variables value
                            content = content.Replace(Consts.VarStart + variable.Name + Consts.VarEnd, variable.Value);
                        }
                    }
                    #endregion </UDF>

                    #region <Destination filename>
                    // Check if the destination filename use variables
                    if (regExes.CheckIfRegExMatch(ref retVal, file.Destination, Consts.RegExVar))
                    {
                        string destFilename = file.Destination;

                        // Loop over all variables unequal bom rows
                        foreach (Variable variable in variables)
                        {
                            if (variable.WhereTo != Enums.VariableAssignedTo.bom)
                            {
                                // Replace the variables name with the variables value
                                destFilename = destFilename.Replace(Consts.VarStart + variable.Name + Consts.VarEnd, variable.Value);
                            }
                        }

                        // Return the solved destination filename
                        file.Destination = destFilename;
                    }
                    #endregion </Destination filename>

                    #region <CleanUp content>
                    // Replace empty variables with the value in SysParams.DefValueCleanUpEmptyVariables
                    content = Regex.Replace(content, @"\<.*?\>", ini.SysParams.DefValueCleanUpEmptyVariables);
                    #endregion </CleanUp content>

                    // Create the directories if necessary
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file.Destination));

                    // Save the destination File
                    System.IO.File.WriteAllText(file.Destination, content, System.Text.Encoding.UTF8);
                    if (!System.IO.File.Exists(file.Destination))
                    {
                        helpers.SetError(ref retVal, 1005011, method + file.Destination + @" cannot be saved.");
                        return;
                    }
                }
                #endregion </Prepare and save the output files>
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1005012, method + ex.Message);
            }
        }

        private static void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Prevent default action
            args.Cancel = true;
        }

        private static void TestScripts(List<Variable> variables)
        {
            return;

            System.Collections.Generic.List<Variable> vars = variables;


            #region <Combine descritions>
            const string _checked = "checked";
            string desc1 = string.empty;
            string desc2 = string.empty;
            string desc = string.empty;
            int currentrow = 0;

            for (int i = 0; i < vars.count; i++)
            {
               if (vars[i].whereto == enums.variableassignedto.bom)
               {
                   if (vars[i].name == "bomrowitemdescription1")
                   {
                       desc1 = vars[i].value;
                   }

                   if (vars[i].name == "bomrowitemdescription2")
                   {
                       desc2 = vars[i].value;
                   }

                   if (vars[i].name == "bomrowitemdescription")
                   {
                       currentrow = i;
                       desc = _checked;
                   }

                   if (!string.isnullorempty(desc1) & !string.isnullorempty(desc2) & desc == _checked)
                   {
                       string delimiter = string.empty;

                       if (!string.isnullorempty(desc1) & !string.isnullorempty(desc2))
                       {
                           delimiter = " / ";
                       }

                       vars[currentrow].value = desc1 + delimiter + desc2;

                       desc = string.empty;
                       desc1 = string.empty;
                       desc2 = string.empty;
                   }

               }
            }
            return true;
            #endregion </Combine descritions>

        }

        #region <Environment>
        private static string GetDomainName()
        {
            string envDomain = string.Empty;

            envDomain = Environment.GetEnvironmentVariable(@"USERDOMAIN", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(envDomain))
            {
                envDomain = Environment.GetEnvironmentVariable(@"USERDOMAIN", EnvironmentVariableTarget.User);
            }

            if (string.IsNullOrEmpty(envDomain))
            {
                envDomain = Environment.GetEnvironmentVariable(@"USERDOMAIN", EnvironmentVariableTarget.Machine);
            }
            return envDomain;
        }
        #endregion </Environment>

    }

}


