/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

using System.Reflection;
using System.Text.RegularExpressions;

namespace ADEcore
{
    public class RegExes
    {
        Helpers helpers = Program.helpers;
        public RegExes() { }

        public bool CheckIfRegExMatch(ref RetValues retVal, string source, string pattern)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            bool ret = false;

            try
            {
                Regex regEx = new Regex(pattern, Consts.regExOptions);
                Match match = regEx.Match(source);

                if (match.Success)
                {
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1003003, method + ex.Message);
            }

            return ret;
        }

        public string GetRegExMatch(ref RetValues retVal, string source, string pattern, int hitNumber = -1, int ignoreFirstLine = 0, int ignoreLastLine = 0, int removeEmptyLines = 0)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string ret = string.Empty;

            // Only positive numbers are valid
            // hitNumber = Math.Abs(hitNumber);

            try
            {
                Regex regEx = new Regex(pattern, Consts.regExOptions);
                Match match = regEx.Match(source);
                if (match.Success)
                {
                    if (match.Groups.Count >= hitNumber + 1)
                    {
                        string retStr = match.Groups[hitNumber + 1].Value.Trim();

                        if (ignoreFirstLine == 1 || ignoreLastLine == 1 || removeEmptyLines == 1)
                        {
                            string[] stringArr = retStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                            if (ignoreFirstLine == 1)
                            {
                                string[] tmpArr = new string[stringArr.Length - 1];
                                Array.Copy(stringArr, 1, tmpArr, 0, stringArr.Length - 1);
                                stringArr = tmpArr;
                                retStr = string.Join(Environment.NewLine, stringArr);
                            }

                            if (ignoreLastLine == 1)
                            {
                                string[] tmpArr = new string[stringArr.Length - 1];
                                Array.Copy(stringArr, 0, tmpArr, 0, stringArr.Length - 1);
                                stringArr = tmpArr;
                                retStr = string.Join(Environment.NewLine, stringArr);
                            }

                            if (removeEmptyLines == 1)
                            {
                                List<string> tmpList = new List<string>();
                                for (int i = 0; i < stringArr.Length; i++)
                                {
                                    string tmpStr = stringArr[i].Trim();
                                    if (!string.IsNullOrEmpty(tmpStr))
                                    {
                                        tmpList.Add(tmpStr);
                                    }
                                }
                                retStr = string.Join(Environment.NewLine, stringArr);
                            }
                        }
                        return retStr.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1003000, method + ex.Message);
            }

            return ret;
        }

        public string[] GetRangeInDocument(ref RetValues retVal, string[] source, string rngPattern, string rows)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string[] ret = null;

            if (rows == Consts.regExSearchInCurrentRow)
            {
                return source;
            }

            List<string> strList = new List<string>();

            int startRow = -1;
            int ii = -1;

            try
            {
                for (int i = 0; i < source.Length; i++)
                {
                    Regex regEx = new Regex(rngPattern, Consts.regExOptions);
                    Match match = regEx.Match(source[i]);

                    if (match.Success)
                    {
                        startRow = i;
                        break;
                    }
                }

                if (startRow > -1)
                {
                    // Return all lines over the range pattern and reverse the lines in the array
                    if (rows.Trim() == Consts.regExSearchInUnknownRowToTop || rows.StartsWith(@"-"))
                    {
                        ii = 0;
                        if (rows.Trim() != Consts.regExSearchInUnknownRowToTop)
                        {
                            ii = startRow + Convert.ToInt32(rows.Trim());
                        }
                        for (int i = ii; i <= startRow; i++)
                        {
                            strList.Add(source[i]);
                        }
                        ret = strList.ToArray();
                        Array.Reverse(ret);
                    }
                    // Return all lines after the range pattern
                    else if (rows.Trim() == Consts.regExSearchInUnknownRowToBottom || rows.StartsWith(@"+"))
                    {
                        ii = source.Length;
                        if (rows.Trim() != Consts.regExSearchInUnknownRowToBottom)
                        {
                            ii = (startRow + Convert.ToInt32(rows.Trim())) + 1;
                        }
                        for (int i = startRow; i < ii; i++)
                        {
                            strList.Add(source[i]);
                        }
                        ret = strList.ToArray();
                    }

                }
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1003001, method + ex.Message);
            }

            return ret;
        }

        public string ReplaceRexExPattern(ref RetValues retVal, string source, string pattern, string replacement)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";

            string ret = string.Empty;
            try
            {
                Regex regEx = new Regex(pattern, Consts.regExOptions);
                Match match = regEx.Match(source);
                if (match.Success)
                {
                    //string x = regEx.Replace(source, replacement); 
                    return regEx.Replace(source, replacement);
                }
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1003002, method + ex.Message);
            }
            return ret;
        }
    }
}
