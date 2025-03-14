/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

namespace ADEcore
{
    public class UdfParams
    {
        public string VarName { get; set; } = string.Empty;
        public string ErrMsg { get; set; } = string.Empty;
        public string RegExPattern { get; set; } = string.Empty;
        public string RegExReplacePatternDelimiter { get; set; } = "<@>";
        public string RegExReplaceValueDelimiter { get; set; } = "<|>";
        public string[] RegExReplacePattern { get; set; } = null;
        public string IsMandatory { get; set; } = "1";
        public string RemoveEmptyLines { get; set; } = "0";
        public string IgnoreFirstLine { get; set; } = "0";
        public string IgnoreLastLine { get; set; } = "0";
        public string HitNumber { get; set; } = "-1";
        public string IsNumber { get; set; } = "0";
        public string Value { get; set; } = string.Empty;
    }
}
