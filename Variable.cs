/*
 * Copyright (c) 2023 Ulf-Dirk Stockburger
*/

namespace ADEcore
{
    public class Variable
    {
        public int Row { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
        public Enums.VariableType Type { get; set; } = Enums.VariableType.unknown;
        public Enums.VariableAssignedTo WhereTo { get; set; } = Enums.VariableAssignedTo.unknown;
        public string Value { get; set; } = string.Empty;
    }
}
