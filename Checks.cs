/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

using AutoMapper;
using MathParserTK;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ADEcore
{
    public class Checks
    {
        #region <private>
        private Helpers helpers = Program.helpers;
        private string Filename { get; set; }
        private Files files = new Files();
        private RegExes regExes = new RegExes();
        private Methods methods = new Methods();
        #endregion </private>

        public Checks() { }

        public Checks(string filename)
        {
            this.Filename = filename;
        }

        #region <Document>
        public void CheckDocumentsIni(ref RetValues retVal, string bpRoot, string filename, ref string iniFilename)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";

            helpers.ErrorFileName = filename;

            BpParams bpParams = new BpParams();

            #region <Initialise the ini file>
            files.ExpandPath(ref bpRoot);
            Ini ini = new Ini();
            #endregion </Initialise the ini file>

            #region <Read the text file in UTF-8 to check in an array>
            string[] txtFileArray = null;
            files.ReadTxtFileIntoArray(ref retVal, ref txtFileArray, filename);
            if (retVal.error)
            {
                helpers.SetError(ref retVal, 1002033, retVal.msg);
                return;
            }
            #endregion </Read the text file in UTF-8 to check in an array>

            #region <Create a string from the array>
            #endregion </Create a string from the array>

            #region <Loop over all INI files in the business partner directory>
            foreach (var file in files.GetFilesLazy(bpRoot, "*.ini", true))
            {
                ini.Filename = file;
                if (ini.parseError)
                {
                    helpers.SetError(ref retVal, 1002034, file + Environment.NewLine + ini.parseErrorMessage);
                    return;
                }

                if (!string.IsNullOrEmpty(ini.Filename))
                {
                    #region <Set variables and defaults>
                    Checks checkFile = new Checks();
                    bool nextCheck = true;
                    string rangeInDocument = null;
                    #endregion </Set variables and defaults>

                    #region <Get the business partner paramters>
                    ini.GetBpParams(ref retVal, out bpParams);
                    if (retVal.error)
                    {
                        return;
                    }
                    #endregion </Get the business partner paramters>

                    #region <Check if the business partner is the right one>
                    var tmp = regExes.GetRangeInDocument(ref retVal, txtFileArray, bpParams.NameAbsolutRow, bpParams.NameRelativeRow);
                    if (tmp == null)
                    {
                        continue;
                    }
                    rangeInDocument = System.String.Join(Environment.NewLine, tmp);
                    if (!regExes.CheckIfRegExMatch(ref retVal, rangeInDocument, bpParams.Name))
                    {
                        nextCheck = false;
                    }
                    #endregion </Check if the business partners document is the right one>

                    #region <Check if the document type is the right one>
                    if (nextCheck)
                    {
                        rangeInDocument = System.String.Join(Environment.NewLine, regExes.GetRangeInDocument(ref retVal, txtFileArray, bpParams.TypeAbsolutRow, bpParams.TypeRelativeRow));
                        if (!regExes.CheckIfRegExMatch(ref retVal, rangeInDocument, bpParams.Type))
                        {
                            nextCheck = false;
                        }
                    }
                    #endregion </Check if the document type is the right one>

                    #region <Check if the id is the right one>
                    if (nextCheck)
                    {
                        if (!System.String.IsNullOrEmpty(bpParams.Id))
                        {
                            rangeInDocument = System.String.Join(Environment.NewLine, regExes.GetRangeInDocument(ref retVal, txtFileArray, bpParams.IdAbsolutRow, bpParams.IdRelativeRow));
                            if (!regExes.CheckIfRegExMatch(ref retVal, rangeInDocument, bpParams.Id))
                            {
                                nextCheck = false;
                            }
                        }
                    }
                    #endregion </Check if the id is the right one>

                    #region <Check if the document can be assigned to the current INI file>
                    if (nextCheck)
                    {
                        iniFilename = ini.Filename;
                        return;
                    }
                    #endregion </Check if the document can be assigned to the current INI file>
                }
            }

            #region <Return an error because non ini file was faound>
            helpers.SetError(ref retVal, 1002001, method + "No business partner found.");
            #endregion </Return an error because non ini file was faound>

            #endregion </Loop over all INI files in the business partner directory>
        }
        #endregion </Document>

        #region <BOMs>
        public void CheckDocumentsBOM(ref RetValues retVal, string[] txtFileArray, ref Ini ini, ref List<List<BOMParams>> bomRowsValues)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";

            #region <Automapper>
            // Mapper configuration
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BOMParams, BOMParams>();
            });

            // Create mapper
            IMapper mapper = config.CreateMapper();
            #endregion </Automapper>

            #region <Get the BOMArea>
            try
            {
                int topRow = -1;
                int bottomRow = -1;
                string[] tmpArray = null;

                #region <Check if number delimiter are given>
                CheckIfNumberDelimiterValid(ref retVal, ini);
                if (retVal.error)
                {
                    return;
                }
                #endregion </Check if number delimiter are given>

                #region <Get the BOM top row>
                tmpArray = new string[txtFileArray.Length];
                Array.Copy(txtFileArray, tmpArray, txtFileArray.Length);

                #region <Check if the search direction for the top search has to be reversed>
                if (Convert.ToInt32(ini.BOMArea.StartFromTop) == 0)
                {
                    Array.Reverse(tmpArray);
                }
                #endregion </Check if the search direction for the top search has to be reversed>

                #region <Search the top row>
                for (int i = 0; i < tmpArray.Length; i++)
                {
                    if (regExes.CheckIfRegExMatch(ref retVal, tmpArray[i], ini.BOMArea.Start))
                    {
                        if (Convert.ToInt32(ini.BOMArea.StartFromTop) == 0)
                        {
                            topRow = (tmpArray.Length - 1) - i;
                        }
                        else
                        {
                            topRow = i;
                        }
                        break;
                    }
                    if (retVal.error)
                    {
                        helpers.SetError(ref retVal, 1002037, method + @"Start position for BOM rows not found");
                        return;
                    }
                }
                #endregion </Search the top row>

                #endregion </Get the BOM top row>

                #region <Get the BOM bottom row>
                tmpArray = new string[txtFileArray.Length];
                Array.Copy(txtFileArray, tmpArray, txtFileArray.Length);

                #region <Check if the search direction for the top search has to be reversed>
                // Attention!
                if (Convert.ToInt32(ini.BOMArea.EndFromTop) == 0)
                {
                    Array.Reverse(tmpArray);
                }
                #endregion </Check if the search direction for the top search has to be reversed>

                #region <Search the bottom row>
                for (int i = 0; i < tmpArray.Length; i++)
                {
                    if (regExes.CheckIfRegExMatch(ref retVal, tmpArray[i], ini.BOMArea.End))
                    {
                        if (Convert.ToInt32(ini.BOMArea.EndFromTop) == 0)
                        {
                            bottomRow = (tmpArray.Length - 1) - i;
                        }
                        else
                        {
                            bottomRow = i;
                        }
                        break;
                    }
                    if (retVal.error)
                    {
                        helpers.SetError(ref retVal, 1002038, method + @"End position in BOM rows not found");
                        return;
                    }
                }
                #endregion </Search the bottom row>

                #endregion </Get the BOM bottom row>

                #region <Check if topRow or BotomRow is -1>
                if (topRow == -1 || bottomRow == -1)
                {
                    helpers.SetError(ref retVal, 1002036, method + @"Error localising the BOM area");
                    return;
                }
                #endregion </Check if topRow or BotomRow is -1>

                #region <Get the BOMArea for further work>
                string[] bomArea = new string[(bottomRow + 1) - topRow];
                Array.Copy(txtFileArray, topRow, bomArea, 0, ((bottomRow + 1) - topRow));
                #endregion </Get the BOMArea for further work>

                #region <Get values to the BOMColumns for each BOM position>

                List<List<BOMParams>> _bomRowsValues = new List<List<BOMParams>>();
                List<string> bomPosition = new List<string>();
                bool rowStart = false;

                #region <Loop over all BOMArea lines>
                for (int i = 0; i < bomArea.Length; i++)
                {
                    // Check if the end of the BOM position has been reached
                    // If the RegExPattern equals string.empty it will always return true to use only one row
                    if (rowStart && regExes.CheckIfRegExMatch(ref retVal, bomArea[i], ini.BOMRow.End))
                    {
                        // Add the last line to the array if the RegEx is not empty
                        if (!string.IsNullOrEmpty(ini.BOMRow.End))
                        {
                            bomPosition.Add(bomArea[i]);
                        }

                        rowStart = false;
                        List<BOMParams> columns = new List<BOMParams>();

                        #region <Loop over all BOMColumns in BomPosition>
                        for (int j = 0; j < ini.BOMParams.Count; j++)
                        {
                            // Use a deep clone from the class via Automapper
                            BOMParams column = mapper.Map<BOMParams, BOMParams>(ini.BOMParams[j]);

                            // The entire block found is returned as a string, with a line feed per line in the operating system's array.
                            if (ini.BOMRow.AsBlock == "1")
                            {
                                column.Value = String.Join("\r\n", bomPosition);
                                columns.Add(column);
                                continue;
                            }

                            // 2, means that the BOM position is searched for 2 lines below the first line and all further lines below are added.
                            // 2,4 means that the BOM item is searched for between the 2nd and 4th line below the first line.
                            // The result is stored in a string, separated by \r\n.
                            if (regExes.CheckIfRegExMatch(ref retVal, column.RelativeRow, @"\d+\,\d*"))
                            {

                                // Find the start and end line of the area to be scanned 
                                string[] range = column.RelativeRow.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                int start = Convert.ToInt32(range[0]);
                                int end = bomPosition.Count;
                                if (range.Length >= 2)
                                {
                                    end = Convert.ToInt32(range[1]);
                                }

                                // Loop over all lines in BOM row
                                string tmpStr = string.Empty;
                                for (int k = start; k < end; k++)
                                {
                                    tmpStr = regExes.GetRegExMatch(ref retVal, bomPosition[k], column.RegExPattern, Convert.ToInt32(column.HitNumber));
                                    if (!string.IsNullOrEmpty(tmpStr))
                                    {
                                        if (k < end - 2)
                                        {
                                            tmpStr = tmpStr + Environment.NewLine;
                                        }
                                        column.Value = column.Value + tmpStr;
                                    }
                                }
                                //break;
                            }
                            // Check if the length of the array is equal to or greater than the relative row
                            else if ((bomPosition.Count - 1) >= Convert.ToInt32(column.RelativeRow))
                            {
                                // From bottom to top
                                if (column.RelativeRow == @"-0")
                                {
                                    for (int k = bomPosition.Count - 1; k >= 0; k--)
                                    {
                                        string tmpValue = regExes.GetRegExMatch(ref retVal, bomPosition[k], column.RegExPattern, Convert.ToInt32(column.HitNumber));
                                        if (!string.IsNullOrEmpty(tmpValue))
                                        {
                                            column.Value = tmpValue;
                                            break;
                                        }
                                    }
                                }
                                // From top to bottom
                                else
                                {
                                    column.Value = regExes.GetRegExMatch(ref retVal, bomPosition[Convert.ToInt32(column.RelativeRow)], column.RegExPattern, Convert.ToInt32(column.HitNumber));
                                }
                            }

                            // Add the column to the Temporary list
                            columns.Add(column);
                        }
                        #endregion </Loop over all BOMColumns in BomPosition>

                        // Add the columns to the temporary list
                        _bomRowsValues.Add(columns);
                    }

                    // Mark the line in BOMArea to be stored in the temporary array bomPosition and delete its contents
                    if (!rowStart && regExes.CheckIfRegExMatch(ref retVal, bomArea[i], ini.BOMRow.Start))
                    {
                        rowStart = true;
                        bomPosition = new List<string>();
                    }

                    // Check if the line in BOMArea has to be saved in the temporary array bomPosition for further analysis
                    if (rowStart)
                    {
                        bomPosition.Add(bomArea[i]);
                    }
                }

                if (_bomRowsValues.Count == 0)
                {
                    helpers.SetError(ref retVal, 1002039, method + @"No BOM positions found");
                    return;
                }

                #endregion </Loop over all BOMArea lines>

                // All other checks are ignored when AsBlock = 1 is set. 
                if (ini.BOMRow.AsBlock == "1")
                {
                    bomRowsValues = _bomRowsValues;
                    return;
                }

                #endregion </Get values to the BOMColumns for each BOM position>

                #region <Check if RegExReplace is necessary>
                for (int i = 0; i < _bomRowsValues.Count; i++)
                {
                    for (int j = 0; j < _bomRowsValues[0].Count; j++)
                    {
                        BOMParams column = _bomRowsValues[i][j];
                        if (column.RegExReplacePattern.Length > 0)
                        {
                            for (int k = 0; k < column.RegExReplacePattern.Length; k++)
                            {
                                #region <Change the pattern>
                                string pattern = column.RegExReplacePattern[k];
                                if (!string.IsNullOrEmpty(pattern))
                                {
                                    string[] patternArr = pattern.Split(new string[] { column.RegExReplaceValueDelimiter }, StringSplitOptions.None);
                                    string tmpStr = regExes.ReplaceRexExPattern(ref retVal, column.Value, patternArr[0], patternArr[1]).Trim();
                                    if (!string.IsNullOrEmpty(tmpStr))
                                    {
                                        column.Value = tmpStr;
                                    }
                                }
                                #endregion </Change the pattern>
                            }
                        }
                    }
                }
                if (retVal.error)
                {
                    retVal.code = 1002021;
                    return;
                }
                #endregion </Check if RegExReplace is necessary>

                #region <Check if param IsMmandatory is valid>
                for (int j = 0; j < _bomRowsValues[0].Count; j++)
                {
                    BOMParams column = _bomRowsValues[0][j];
                    CheckParamIsMandatoryNumber(ref retVal, column, j);
                    if (retVal.error)
                    {
                        retVal.code = 1002002;
                        return;
                    }
                }

                for (int j = 0; j < _bomRowsValues[0].Count; j++)
                {
                    BOMParams column = _bomRowsValues[0][j];
                    CheckParamIsMandatoryValid(ref retVal, column, j);
                    if (retVal.error)
                    {
                        retVal.code = 1002003;
                        return;
                    }
                }
                #endregion </Check if param IsMmandatory is valid>

                #region <Check if all mandatory fields have values>
                for (int i = 0; i < _bomRowsValues.Count; i++)
                {
                    for (int j = 0; j < _bomRowsValues[i].Count; j++)
                    {
                        BOMParams column = _bomRowsValues[i][j];
                        CheckParamIsMandatoryValue(ref retVal, column, j, i);
                        if (retVal.error)
                        {
                            retVal.code = 1002004;
                            return;
                        }
                    }
                }
                #endregion </Check if all mandatory fields have values>

                #region <Check if param IsNumber is valid>
                for (int j = 0; j < _bomRowsValues[0].Count; j++)
                {
                    BOMParams column = _bomRowsValues[0][j];
                    CheckParamIsNumberNumber(ref retVal, column, j);
                    if (retVal.error)
                    {
                        retVal.code = 1002005;
                        return;
                    }
                }

                for (int j = 0; j < _bomRowsValues[0].Count; j++)
                {
                    BOMParams column = _bomRowsValues[0][j];
                    CheckParamIsMandatoryValid(ref retVal, column, j);
                    if (retVal.error)
                    {
                        retVal.code = 1002006;
                        return;
                    }
                }
                #endregion </Check if param IsNumber is valid>

                #region <Check if all IsNumber fields can be convertet to numbers>
                for (int i = 0; i < _bomRowsValues.Count; i++)
                {
                    for (int j = 0; j < _bomRowsValues[i].Count; j++)
                    {
                        BOMParams column = _bomRowsValues[i][j];
                        string replacedString = CheckIsNumberValue(ref retVal, column, j, ini.SysParams);
                        if (retVal.error)
                        {
                            retVal.code = 1002007;
                            return;
                        }
                        if (column.Value != replacedString)
                        {
                            column.Value = replacedString;
                        }
                    }
                }

                #endregion </Check if all IsNumber fields can be convertet to numbers>

                bomRowsValues = _bomRowsValues;
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1002008, method + ex.Message);
            }
            #endregion </Get the BOMArea>

        }

        public void SolveBomformulas(ref RetValues retVal, List<string> bomFormulas)
        {
        
            // Exit if the formulas list is empty
            if (bomFormulas.Count == 0)
            {
                return;
            }

            string method = MethodBase.GetCurrentMethod().Name + @" / ";

            // MathParser
            bool radians = false;
            decimal[] bomPosResult = new decimal[bomFormulas.Count];
            decimal bomEqualValue = 0;
            decimal bomEqualSum = 0;

            // First BOM row for try ... catch
            string firstBomRow = string.Empty;

            try
            {
                #region <Check for equal in formula>
                for (int i = 0; i < bomFormulas.Count; i++)
                {

                    if (string.IsNullOrEmpty(firstBomRow))
                    {
                        firstBomRow = bomFormulas[i];
                    }

                    if (!regExes.CheckIfRegExMatch(ref retVal, bomFormulas[i], Consts.regExBomEqual))
                    {
                        retVal.error = true;
                        retVal.code = 1002009;
                        retVal.msg = @"No valid formula found for BOM items.";
                        return;
                    }
                }
                #endregion </Check for equal in formula>

                #region <Solve the formula and check the results>

                // Solve the BOM position line and save the result into an array
                for (int i = 0; i < bomFormulas.Count; i++)
                {
                    string[] formula = bomFormulas[i].Split(new string[] { Consts.regExBomEqual.Substring(1, Consts.regExBomEqual.Length - 2) }, StringSplitOptions.TrimEntries);
                    int roundingAccuracy = helpers.GetRoundingAccuracyFromFormula(ref retVal, formula[1]);

                    if (i == 0)
                    {
                        bomEqualValue = Convert.ToDecimal(formula[1]);
                    }
                    MathParser parser = new MathParser();
                    bomPosResult[i] = Math.Round(Convert.ToDecimal(parser.Parse(formula[0], radians)), roundingAccuracy);
                }

                // Create the sum of all results in BOM positions
                for (int i = 0; i < bomPosResult.Length; i++)
                {
                    bomEqualSum = bomEqualSum + bomPosResult[i];
                }

                // Compare the sum and the equal value
                if (bomEqualSum != bomEqualValue)
                {
                    retVal.error = true;
                    retVal.code = 1002010;
                    retVal.msg = @"The sum of the values of the BOM items (" + bomEqualSum.ToString() + ") is not equal to the comparison value (" + bomEqualValue.ToString() + ").";
                    return;
                }
                #endregion </Solve the formula and check the results>

            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1002011, method + ex.Message + " (first BOM row: '" + firstBomRow + "')");
            }

        }

        public void GetFormulasFromBOMRows(ref RetValues retVal, ref List<string> bomFormulas, Ini ini, List<List<BOMParams>> bomRowsValues, List<Variable> variables)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";

            string formula = string.Empty;
            List<string> formulas = new List<string>();

            List<Structs.BomUdfValue> rowColumn = new List<Structs.BomUdfValue>();

            // Get needed column variables to BOM row and udfParams
            try
            {
                // Loop over all Actions
                foreach (var item in ini.CalculationParams)
                {
                    // If the action = ActionBomSum then calculate the BOM rows
                    if (item.Name == Consts.calculationBomSumKey && item.Value != Consts.regExNoBomCheck)
                    {
                        // Get formula
                        formula = item.Value;

                        // Get the variable name via RegEx
                        Regex regEx = new Regex(Consts.regExActionVariables, RegexOptions.ECMAScript);
                        MatchCollection matches = regEx.Matches(formula);

                        foreach (Match match in matches)
                        {
                            Structs.BomUdfValue listItem = new Structs.BomUdfValue();
                            listItem.varName = match.Value.Replace(Consts.VarStart, string.Empty).Replace(Consts.VarEnd, string.Empty).Trim();

                            // Add the variable to list if not exists
                            if (!rowColumn.Any(obj => obj.varName == listItem.varName))
                            {
                                rowColumn.Add(listItem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1002012, method + ex.Message);
            }

            // Replace needed column variables with the values from the BOM row
            try
            {
                // Loop over all BOM rows
                for (int i = 0; i < bomRowsValues.Count; i++)
                {
                    // Get a copy of the formula from the ini file
                    string tmpformula = formula;

                    // Loop over all variables in the formula
                    for (int j = 0; j < rowColumn.Count; j++)
                    {
                        // Loop over all columns in the current BOM row
                        for (int k = 0; k < bomRowsValues[i].Count; k++)
                        {
                            // Check if formula found
                            if (rowColumn[j].varName == bomRowsValues[i][k].VarName)
                            {
                                // Check if the vakue is empty
                                if (String.IsNullOrEmpty(bomRowsValues[i][k].Value))
                                {
                                    bomRowsValues[i][k].Value = ini.SysParams.DefValueDouble;
                                }
                                // Replace the variable name with the variable value from the current BOM row
                                tmpformula = tmpformula.Replace(Consts.VarStart + bomRowsValues[i][k].VarName + Consts.VarEnd, bomRowsValues[i][k].Value.ToString());
                                // Exit for
                                break;
                            }
                        }
                    }

                    // Get an array of all variables in the current formula
                    Regex regEx = new Regex(Consts.regExActionVariables, RegexOptions.ECMAScript);
                    MatchCollection matches = regEx.Matches(tmpformula);

                    for (int j = 0; j < matches.Count; j++)
                    {
                        // Loop over all UDFs
                        for (int k = 0; k < variables.Count; k++)
                        {
                            if (variables[k].Name == matches[j].Value.Replace(Consts.VarStart, string.Empty).Replace(Consts.VarEnd, string.Empty).Trim())
                            {
                                // Replace the variable name with the variable value from the udf variable
                                tmpformula = tmpformula.Replace(Consts.VarStart + variables[k].Name + Consts.VarEnd, variables[k].Value.ToString());
                                // Exit for
                                break;
                            }
                        }
                    }

                    formulas.Add(tmpformula);
                }
                bomFormulas = formulas;
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1002013, method + ex.Message);
            }
        }

        #endregion </BOMs>

        #region <UDFs>
        public void GetFormulasFromUDFs(ref RetValues retVal, ref List<string> udfFormulas, Ini ini, List<UdfParams> udfValues)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";

            string formula = string.Empty;
            List<string> formulas = new List<string>();

            List<Structs.BomUdfValue> variables = new List<Structs.BomUdfValue>();

            try
            {
                // Loop over all Actions
                foreach (var item in ini.CalculationParams)
                {
                    // If the action = ActionBomSum then calculate the BOM rows
                    if (item.Name != Consts.calculationBomSumKey && item.Value != Consts.regExNoBomCheck)
                    {
                        // Get formula
                        formula = item.Name + @"=" + item.Value;
                        formulas.Add(formula);

                        // Get the variable name via RegEx
                        Regex regEx = new Regex(Consts.regExActionVariables, RegexOptions.ECMAScript);
                        MatchCollection matches = regEx.Matches(formula);

                        foreach (Match match in matches)
                        {
                            Structs.BomUdfValue listItem = new Structs.BomUdfValue();
                            listItem.varName = match.Value.Replace(Consts.VarStart, string.Empty).Replace(Consts.VarEnd, string.Empty).Trim();

                            // Add the variable to list if not exists
                            if (!variables.Any(obj => obj.varName == listItem.varName))
                            {
                                variables.Add(listItem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1002014, method + ex.Message);
            }

            // Replace variables in the formulas with the values in the UDFs
            try
            {
                // Loop over all formulas
                for (int i = 0; i < formulas.Count; i++)
                {
                    // Get an array of all variables in the current formula
                    Regex regEx = new Regex(Consts.regExActionVariables, RegexOptions.ECMAScript);
                    MatchCollection matches = regEx.Matches(formulas[i]);

                    for (int j = 0; j < matches.Count; j++)
                    {
                        // Loop over all UDFs
                        for (int k = 0; k < udfValues.Count; k++)
                        {
                            if (udfValues[k].VarName == matches[j].Value.Replace(Consts.VarStart, string.Empty).Replace(Consts.VarEnd, string.Empty).Trim())
                            {
                                // Check if the vakue is a number
                                if (udfValues[k].IsNumber == "1")
                                {
                                    // Check if the vakue is empty
                                    if (String.IsNullOrEmpty(udfValues[k].Value))
                                    {
                                        udfValues[k].Value = ini.SysParams.DefValueDouble;
                                    }
                                }
                                // Replace the variable name with the variable value from the udf variable
                                formulas[i] = formulas[i].Replace(Consts.VarStart + udfValues[k].VarName + Consts.VarEnd, udfValues[k].Value.ToString());
                                // Exit for
                                break;
                            }
                        }
                    }
                }
                udfFormulas = formulas;
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1002015, method + ex.Message);
            }
        }

        public void SolveUdfformulas(ref RetValues retVal, ref List<Variable> variables, List<string> udfFormulas)
        {
            // Exit if the formuls list is empty
            if (udfFormulas.Count == 0)
            {
                return;
            }

            string method = MethodBase.GetCurrentMethod().Name + @" / ";

            // MathParser
            bool radians = false;
            string parseResult = string.Empty;

            try
            {

                #region <Check for = in formula>
                for (int i = 0; i < udfFormulas.Count; i++)
                {
                    if (!regExes.CheckIfRegExMatch(ref retVal, udfFormulas[i], Consts.regExUdfEqual))
                    {
                        helpers.SetError(ref retVal, 10020016, @"No valid formula found in " + Consts.calculationBomSumSection + ".");
                        return;
                    }
                }
                #endregion </Check for = in formula>

                #region <Solve the formula and check the results>

                // Solve the BOM position line and save the result into an array
                for (int i = 0; i < udfFormulas.Count; i++)
                {
                    // Get the current formula
                    string[] formula = udfFormulas[i].Split(new string[] { Consts.regExUdfEqual.Substring(1, Consts.regExUdfEqual.Length - 2) }, StringSplitOptions.TrimEntries);
                    // Get the maximum length of decimal places
                    int roundingAccuracy = helpers.GetRoundingAccuracyFromFormula(ref retVal, formula[1]);

                    // Create and fill in the new variable
                    Variable variable = new Variable();
                    variable.WhereTo = Enums.VariableAssignedTo.udf;
                    variable.Name = formula[0];
                    variable.Row = -1;
                    variable.Type = Enums.VariableType.str;
                    // Try to parse the formula
                    try
                    {
                        MathParser parser = new MathParser();
                        parseResult = Math.Round(parser.Parse(formula[1], radians), roundingAccuracy).ToString();
                    }
                    catch (Exception ex)
                    {
                        parseResult = formula[1];
                    }
                    variable.Value = parseResult;

                    // Add the new formula to the list
                    variables.Add(variable);

                }

                #endregion </Solve the formula and check the results>

            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1002017, method + ex.Message);
            }

        }

        #endregion </UDFs>

        #region <UdfParams>
        public void CheckDocumentsUdf(ref RetValues retVal, string pdfTxtFile, ref Ini ini, ref List<UdfParams> udfValues)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";

            #region <Automapper>
            // Mapper configuration
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UdfParams, UdfParams>();
            });

            // Create mapper
            IMapper mapper = config.CreateMapper();
            #endregion </Automapper>

            List<UdfParams> _udfValues = new List<UdfParams>();

            try
            {
                #region <Check if number delimiter are given>
                CheckIfNumberDelimiterValid(ref retVal, ini);
                if (retVal.error)
                {
                    return;
                }
                #endregion </Check if number delimiter are given>

                #region <Loop over all UdfParams in list>
                for (int j = 0; j < ini.UdfParams.Count; j++)
                {
                    // Use a deep clone from the class via Automapper
                    UdfParams _params = mapper.Map<UdfParams, UdfParams>(ini.UdfParams[j]);
                    // Check if the length of the array is equal to or greater than the relative row
                    _params.Value = regExes.GetRegExMatch(ref retVal, pdfTxtFile, _params.RegExPattern, Convert.ToInt32(_params.HitNumber), Convert.ToInt32(_params.IgnoreFirstLine), Convert.ToInt32(_params.IgnoreLastLine), Convert.ToInt32(_params.RemoveEmptyLines));
                    if (retVal.error)
                    {
                        return;
                    }
                    // Add the updated properties to the list
                    _udfValues.Add(_params);

                }
                #endregion </Loop over all UdfParams in list>

                #region <Check if RegExReplace is necessary>
                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    if (_params.RegExReplacePattern.Length > 0)
                    {
                        for (int j = 0; j < _params.RegExReplacePattern.Length; j++)
                        {
                            #region <Change the pattern>
                            string pattern = _params.RegExReplacePattern[j];
                            if (!string.IsNullOrEmpty(pattern))
                            {
                                string[] patternArr = pattern.Split(new string[] { _params.RegExReplaceValueDelimiter }, StringSplitOptions.None);
                                string tmpStr = regExes.ReplaceRexExPattern(ref retVal, _params.Value, patternArr[0], patternArr[1]).Trim();
                                if (!string.IsNullOrEmpty(tmpStr))
                                {
                                    _params.Value = tmpStr;
                                }
                            }
                            #endregion </Change the pattern>
                        }
                    }
                }
                if (retVal.error)
                {
                    retVal.code = 1002018;
                    return;
                }
                #endregion </Check if RegExReplace is necessary>

                // IsMandatory
                #region <Check if param IsMmandatory is valid>
                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    CheckParamIsMandatoryNumber(ref retVal, _params, i);
                    if (retVal.error)
                    {
                        retVal.code = 1002022;
                        return;
                    }
                }

                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    CheckParamIsMandatoryValid(ref retVal, _params, i);
                    if (retVal.error)
                    {
                        retVal.code = 1002023;
                        return;
                    }
                }
                #endregion </Check if param IsMmandatory is valid>

                #region <Check if all mandatory fields have values>
                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams column = _udfValues[i];
                    CheckParamIsMandatoryValue(ref retVal, column);
                    if (retVal.error)
                    {
                        retVal.code = 1002024;
                        return;
                    }
                }
                #endregion </Check if all mandatory fields have values>

                // IsNumber
                #region <Check if param IsNumber is valid>
                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    CheckParamIsNumberNumber(ref retVal, _params, i);
                    if (retVal.error)
                    {
                        retVal.code = 1002025;
                        return;
                    }
                }

                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    CheckParamIsMandatoryValid(ref retVal, _params, i);
                    if (retVal.error)
                    {
                        retVal.code = 1002035;
                        return;
                    }
                }
                #endregion </Check if param IsNumber is valid>

                #region <Check if all IsNumber fields can be convertet to numbers>
                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    string replacedString = CheckIsNumberValue(ref retVal, _params, i, ini.SysParams);
                    if (retVal.error)
                    {
                        retVal.code = 1002026;
                        return;
                    }
                    if (_params.Value != replacedString)
                    {
                        _params.Value = replacedString;
                    }
                }

                #endregion </Check if all IsNumber fields can be convertet to numbers>

                // RemoveEmptyLines
                #region <Check if param RemoveEmptyLines is valid>
                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    CheckParamRemoveEmptyLinesNumber(ref retVal, _params, i);
                    if (retVal.error)
                    {
                        retVal.code = 1002000;
                        return;
                    }
                }

                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    CheckParamRemoveEmptyLinesValid(ref retVal, _params, i);
                    if (retVal.error)
                    {
                        retVal.code = 1002027;
                        return;
                    }
                }
                #endregion </Check if param RemoveEmptyLines is valid>

                // IgnoreFirstLine
                #region <Check if param IgnoreFirstLine is valid>
                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    CheckParamIgnoreFirstLineNumber(ref retVal, _params, i);
                    if (retVal.error)
                    {
                        retVal.code = 1002028;
                        return;
                    }
                }

                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    CheckParamIgnoreFirstLineValid(ref retVal, _params, i);
                    if (retVal.error)
                    {
                        retVal.code = 1002029;
                        return;
                    }
                }
                #endregion </Check if param IgnoreFirstLine is valid>

                // IgnoreLastLine
                #region <Check if param IgnoreLastLine is valid>
                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    CheckParamIgnoreLastLineNumber(ref retVal, _params, i);
                    if (retVal.error)
                    {
                        retVal.code = 1002030;
                        return;
                    }
                }

                for (int i = 0; i < _udfValues.Count; i++)
                {
                    UdfParams _params = _udfValues[i];
                    CheckParamIgnoreLastLineValid(ref retVal, _params, i);
                    if (retVal.error)
                    {
                        retVal.code = 1002031;
                        return;
                    }
                }
                #endregion </Check if param IgnoreLastLine is valid>

                udfValues = _udfValues;
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1002032, method + ex.Message);
            }

        }
        #endregion </UdfParams>

        #region </Valid for BOMParams and UdfParams>
        private void CheckIfNumberDelimiterValid(ref RetValues retVal, Ini ini)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            bool error = false;

            if (string.IsNullOrEmpty(ini.SysParams.SysDecimalSeparator))
            {
                error = true;
            }
            if (string.IsNullOrEmpty(ini.SysParams.SysThousandsSeparators))
            {
                error = true;
            }
            if (string.IsNullOrEmpty(ini.SysParams.DocDecimalSeparator))
            {
                error = true;
            }
            if (string.IsNullOrEmpty(ini.SysParams.DocThousandsSeparators))
            {
                error |= true;
            }
            if (error)
            {
                helpers.SetError(ref retVal, 1002019, method + @"Error in section [ " + nameof(SysParams) + "] *DecimalSeparator or *ThousandsSeparators.");
            }

        }

        // IsMandatory
        private void CheckParamIsMandatoryNumber(ref RetValues retVal, dynamic column, int col)
        {
            bool error = false;
            if (!decimal.TryParse(column.IsMandatory, out decimal value))
            {
                error = true;
                column.ErrMsg = "Error in 'IsMandatory' in column '" + column.VarName + "'. The value '" + column.IsMandatory + "' is non numeric.";
            }
            if (error)
            {
                retVal.error = true;
                if (!System.String.IsNullOrEmpty(column.ErrMsg))
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + column.ErrMsg;
            }
        }

        private void CheckParamIsMandatoryValid(ref RetValues retVal, dynamic column, int col)
        {
            bool error = false;
            if (Convert.ToInt32(column.IsMandatory) < 0 || Convert.ToInt32(column.IsMandatory) > 1)
            {
                error = true;
                column.ErrMsg = "Error in 'IsMandatory' in column '" + column.VarName + "'. Permitted values are 0 or 1.";
            }
            if (error)
            {
                retVal.error = true;
                if (!System.String.IsNullOrEmpty(column.ErrMsg))
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + column.ErrMsg;
            }
        }

        private void CheckParamIsMandatoryValue(ref RetValues retVal, dynamic column, int col = -1, int row = -1)
        {
            if (Convert.ToInt32(column.IsMandatory) == 1)
            {
                if (string.IsNullOrEmpty(column.Value))
                {
                    if (row > -1)
                    {
                        column.ErrMsg = "Error in 'Value' in row '" + row + "' in column '" + column.VarName + "'. The field is marked as 'IsMandatory' but has no value assigned to it.";
                    }
                    else
                    {
                        column.ErrMsg = "Error in 'Value'. The field '" + column.VarName + "'is marked as 'IsMandatory' but has no value assigned to it.";
                    }

                    retVal.error = true;
                    if (!System.String.IsNullOrEmpty(column.ErrMsg))
                    {
                        retVal.msg = retVal.msg + Environment.NewLine;
                    }
                    retVal.msg = retVal.msg + column.ErrMsg;
                }
            }
        }

        // IsNumber
        private void CheckParamIsNumberNumber(ref RetValues retVal, dynamic column, int col = -1)
        {
            bool error = false;
            if (!decimal.TryParse(column.IsNumber, out decimal value))
            {
                error = true;
                if (col > -1)
                {
                    column.ErrMsg = "Error in 'IsNumber' in column '" + column.VarName + "'. The value '" + column.IsMandatory + "' is non integer.";
                }
                else
                {
                    column.ErrMsg = "Error in 'IsNumber' The value '" + column.IsMandatory + "' is non integer.";
                }
            }
            if (error)
            {
                retVal.error = true;
                if (!System.String.IsNullOrEmpty(column.ErrMsg))
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + column.ErrMsg;
            }
        }

        private void CheckParamIsNumberValid(ref RetValues retVal, dynamic column, int col)
        {
            bool error = false;
            if (Convert.ToInt32(column.IsNumber) < 0 || Convert.ToInt32(column.IsNumber) > 1)
            {
                error = true;
                column.ErrMsg = "Error in 'IsNumber' in column '" + column.VarName + "'. Permitted values are 0 or 1.";
            }
            if (error)
            {
                retVal.error = true;
                if (!System.String.IsNullOrEmpty(column.ErrMsg))
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + column.ErrMsg;
            }
        }

        private string CheckIsNumberValue(ref RetValues retVal, dynamic column, int col, SysParams sysParams)
        {
            string ret = column.Value;

            if (Convert.ToInt32(column.IsNumber) == 1)
            {
                bool isMandatory = Convert.ToBoolean(Convert.ToInt32(column.IsMandatory));
                string guid = " " + Guid.NewGuid().ToString() + " ";
                string originalValue = column.Value.Trim();
                string replacedValue = originalValue;

                // Checks if the value is mandatory and the value is not empty
                if (isMandatory && string.IsNullOrEmpty(originalValue))
                {
                    retVal.error = true;
                    retVal.code = 1002020;
                    retVal.msg = "Error in 'IsNumber'. No value found to convert to a number." + Environment.NewLine;
                    return ret;
                }

                // Chance the documents decimal seperator to the temporary guid
                if (sysParams.DocDecimalSeparator != sysParams.SysDecimalSeparator)
                {
                    string patternDecimalSeperator = "(\\d+)" + sysParams.DocDecimalSeparator + "(\\d+)";
                    string returnPattern = regExes.GetRegExMatch(ref retVal, originalValue, patternDecimalSeperator);
                    if (!string.IsNullOrEmpty(returnPattern))
                    {
                        replacedValue = regExes.ReplaceRexExPattern(ref retVal, originalValue, patternDecimalSeperator, "$1" + guid + "$2");
                    }
                    else
                    {
                        replacedValue = originalValue;
                    }
                }
                // Change the documents thousands seperator to system thousands seperator
                if (sysParams.DocThousandsSeparators != sysParams.SysThousandsSeparators)
                {
                    string patternThousandsSeperator = "(\\" + sysParams.DocThousandsSeparators + ")";
                    string returnPattern = regExes.GetRegExMatch(ref retVal, replacedValue, patternThousandsSeperator);
                    if (!string.IsNullOrEmpty(returnPattern))
                    {
                        replacedValue = regExes.ReplaceRexExPattern(ref retVal, replacedValue, "\\.", string.Empty);
                    }
                }
                // Change the temporary Gui if necessary
                if (sysParams.DocThousandsSeparators != sysParams.SysThousandsSeparators
                    & sysParams.DocDecimalSeparator != sysParams.SysDecimalSeparator)
                {
                    string returnPattern = regExes.GetRegExMatch(ref retVal, replacedValue, guid);
                    if (!string.IsNullOrEmpty(returnPattern))
                    {
                        replacedValue = regExes.ReplaceRexExPattern(ref retVal, replacedValue, guid, sysParams.SysDecimalSeparator);
                    }
                }
                // Delete the thousands seperator if necessary
                if (sysParams.DocThousandsSeparators == sysParams.SysThousandsSeparators)
                {
                    replacedValue = replacedValue.Replace(sysParams.SysThousandsSeparators, string.Empty);
                }

                ret = replacedValue;
            }

            return ret;
        }

        // RemoveEmptyLine
        private void CheckParamRemoveEmptyLinesNumber(ref RetValues retVal, dynamic column, int col)
        {
            bool error = false;
            if (!decimal.TryParse(column.RemoveEmptyLines, out decimal value))
            {
                error = true;
                column.ErrMsg = "Error in 'RemoveEmptyLines' in column '" + column.VarName + "'. The value '" + column.RemoveEmptyLines + "' is non numeric.";
            }
            if (error)
            {
                retVal.error = true;
                if (!System.String.IsNullOrEmpty(column.ErrMsg))
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + column.ErrMsg;
            }
        }

        private void CheckParamRemoveEmptyLinesValid(ref RetValues retVal, dynamic column, int col)
        {
            bool error = false;
            if (Convert.ToInt32(column.RemoveEmptyLines) < 0 || Convert.ToInt32(column.RemoveEmptyLines) > 1)
            {
                error = true;
                column.ErrMsg = "Error in 'RemoveEmptyLines' in column '" + column.VarName + "'. Permitted values are 0 or 1.";
            }
            if (error)
            {
                retVal.error = true;
                if (!System.String.IsNullOrEmpty(column.ErrMsg))
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + column.ErrMsg;
            }
        }

        // IgnoreFirstLine
        private void CheckParamIgnoreFirstLineNumber(ref RetValues retVal, dynamic column, int col)
        {
            bool error = false;
            if (!decimal.TryParse(column.IgnoreFirstLine, out decimal value))
            {
                error = true;
                column.ErrMsg = "Error in 'IgnoreFirstLine' in column '" + column.VarName + "'. The value '" + column.RemoveEmptyLines + "' is non numeric.";
            }
            if (error)
            {
                retVal.error = true;
                if (!System.String.IsNullOrEmpty(column.ErrMsg))
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + column.ErrMsg;
            }
        }

        private void CheckParamIgnoreFirstLineValid(ref RetValues retVal, dynamic column, int col)
        {
            bool error = false;
            if (Convert.ToInt32(column.IgnoreFirstLine) < 0 || Convert.ToInt32(column.IgnoreFirstLine) > 1)
            {
                error = true;
                column.ErrMsg = "Error in 'IgnoreFirstLine' in column '" + column.VarName + "'. Permitted values are 0 or 1.";
            }
            if (error)
            {
                retVal.error = true;
                if (!System.String.IsNullOrEmpty(column.ErrMsg))
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + column.ErrMsg;
            }
        }

        // IgnoreLastLine
        private void CheckParamIgnoreLastLineNumber(ref RetValues retVal, dynamic column, int col)
        {
            bool error = false;
            if (!decimal.TryParse(column.IgnoreLastLine, out decimal value))
            {
                error = true;
                column.ErrMsg = "Error in 'IgnoreLastLine' in column '" + column.VarName + "'. The value '" + column.RemoveEmptyLines + "' is non numeric.";
            }
            if (error)
            {
                retVal.error = true;
                if (!System.String.IsNullOrEmpty(column.ErrMsg))
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + column.ErrMsg;
            }
        }

        private void CheckParamIgnoreLastLineValid(ref RetValues retVal, dynamic column, int col)
        {
            bool error = false;
            if (Convert.ToInt32(column.IgnoreLastLine) < 0 || Convert.ToInt32(column.IgnoreLastLine) > 1)
            {
                error = true;
                column.ErrMsg = "Error in 'IgnoreLastLine' in column '" + column.VarName + "'. Permitted values are 0 or 1.";
            }
            if (error)
            {
                retVal.error = true;
                if (!System.String.IsNullOrEmpty(column.ErrMsg))
                {
                    retVal.msg = retVal.msg + Environment.NewLine;
                }
                retVal.msg = retVal.msg + column.ErrMsg;
            }
        }

        #endregion </Valid for BOMParams and UdfParams>

    }
}

