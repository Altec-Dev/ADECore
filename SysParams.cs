/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

namespace ADEcore
{
    public class SysParams
    {
        public string VarName { get; set; } = string.Empty;
        public string ErrMsg { get; set; } = string.Empty;
        public string SysDecimalSeparator { get; set; } = string.Empty;
        public string SysThousandsSeparators { get; set; } = string.Empty;
        public string DocDecimalSeparator { get; set; } = string.Empty;
        public string DocThousandsSeparators { get; set; } = string.Empty;
        public string ScriptLanguage { get; set; } = string.Empty;
        public string DefValueString { get; set; } = string.Empty;
        public string DefValueDouble { get; set; } = string.Empty;
        public string DefValueCleanUpEmptyVariables { get; set; } = string.Empty;
    }
}
