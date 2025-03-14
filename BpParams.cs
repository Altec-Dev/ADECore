/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

namespace ADEcore
{
    public class BpParams
    {
        public string VarName { get; set; } = string.Empty;
        public string ErrMsg { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameRelativeRow { get; set; } = "0";
        public string NameAbsolutRow { get; set; } = string.Empty;
        //
        public string Type { get; set; } = string.Empty;
        public string TypeRelativeRow { get; set; } = "0";
        public string TypeAbsolutRow { get; set; } = string.Empty;
        //
        public string Id { get; set; } = string.Empty;
        public string IdRelativeRow { get; set; } = "0";
        public string IdAbsolutRow { get; set; } = string.Empty;
    }
}
