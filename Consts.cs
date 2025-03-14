/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

using System.Text.RegularExpressions;

namespace ADEcore
{
    public class Consts
    {

        #region <Programme info>
        public static readonly string appVersion = @"V0.4.0.128";
        public static readonly string consoleTitle = @"Automated Document Entry";
        public static readonly string copyRight = @"® 2023 by Ulf-Dirk Stockburger";
        #endregion </Programme info>

        #region <Mutex>
        public static readonly string mutexADEcoreServer = @"09481efc-5c19-42af-9756-166c171d653b";
        #endregion </Mutex>

        #region <INI>
        string iniRelativPath = "./ini/";
        #endregion </INI>

        #region <BOMParams>
        public static readonly string bomParamsDelimiter = "_";
        #endregion </BOMParams>

        #region <Params>
        public static readonly string paramEOL = "eol";
        #endregion </Params>

        #region <RegEx>
        public const string regExSearchInUnknownRowToTop = @"-0";
        public const string regExSearchInUnknownRowToBottom = @"+0";
        public const string regExSearchInCurrentRow = @"0";
        public const RegexOptions regExOptions = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.ECMAScript;
        public const string regExActionVariables = @"(\<.*?\>)";
        public const string regExBomEqual = @"(equal)";
        public const string regExUdfEqual = @"(=)";
        public const string regExDecimal = @"\d+\.\d+";
        public const string regExNoBomCheck = @"(NoCalculationBomSum)";
        #endregion </RegEx>

        #region <BOM>
        public static readonly string RegExBomRow = @"(<BomRow>)([\s\S]*?)(<\/BomRow>)";
        public static readonly string RegExBomRowStart = @"(<BomRow>)";
        public static readonly string RegExBomRowEnd = @"(<\/BomRow>)";

        #endregion </BOM>

        #region <Variables>
        public static readonly string VarStart = @"<";
        public static readonly string VarEnd = @">";
        public static readonly string RegExVar = @"(<[^>]*>)";
        #endregion </Variables>

        #region <Calculation>
        // Section
        public static readonly string calculationBomSumSection = "~Calculation";
        // Keys
        public static readonly string calculationBomSumKey = "CalculationBomSum";
        #endregion </Calculation>

        #region <Script>
        // Section
        public static readonly string ScriptSection = "~Script";
        #endregion </Script>

        #region <Output>
        public static readonly string OutputSection = "~Output";
        // Delimiter
        public static readonly string FileNamesDelimiter = ";";
        public static readonly string FilesDelimiter = "::";
        // Keys
        public static readonly string outputFileNamesKey = "FileNames";
        #endregion </Output>

        #region <Parse files>
        public static readonly string ParseFilesFinished = "finished";
        public static readonly string ParseFilesInProcess = "inprocess";
        public static readonly string ParseFilesError = "error";
        public static readonly string ParseFilesText = "txt";
        public static readonly string ParseFilesNew = "pdf";
        #endregion </Parse files>

        #region <EMail>
        public static readonly string[] emailFakeAccounts = new string[] { "ADECore" };
        #endregion </EMail>

    }
}
