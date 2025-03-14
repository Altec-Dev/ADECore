/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

namespace ADEcore
{
    public class BOMArea
    {
        public string VarName { get; set; } = string.Empty;
        public string Start { get; set; } = string.Empty;
        public string StartFromTop { get; set; } = "1";
        public string End { get; set; } = string.Empty;
        public string EndFromTop { get; set; } = "0";
    }

    public class BOMRow
    {
        public string VarName { get; set; } = string.Empty;
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public string AsBlock { get; set; } = "0";
    }
    public class BOMColumns
    {
        public string VarName { get; set; } = string.Empty;
        public string ColumnsCounter { get; set; } = "0";
    }

    public class BOMParams
    {
        public string VarName { get; set; } = string.Empty;
        public string ErrMsg { get; set; } = string.Empty;
        public string IsMandatory { get; set; } = "1";
        public string RelativeRow { get; set; } = "0";
        public string HitNumber { get; set; } = "-1";
        public string IsNumber { get; set; } = "0";
        public string RegExPattern { get; set; } = string.Empty;
        public string RegExReplacePatternDelimiter { get; set; } = "<@>";
        public string RegExReplaceValueDelimiter { get; set; } = "<|>";
        public string[] RegExReplacePattern { get; set; } = null;
        public string Value { get; set; } = string.Empty;
    }
}
