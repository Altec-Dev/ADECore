/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

namespace ADEcore
{
    public class Enums
    {
        public enum VariableType
        {
            unknown = 0,
            dbl = 1,
            str = 2
        };

        public enum VariableAssignedTo
        {
            unknown = 0,
            bom = 1,
            udf = 2,
            script = 3,
            system = 4
        };

        public enum ScriptType
        {
            error = 0,
            cSharp = 1,
        };
    }
}

