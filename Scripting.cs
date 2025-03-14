/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using System.Text;
using static ADEcore.Enums;

namespace ADEcore
{
    public class Globals
    {
        public Variable Variable { get; set; }
        public Enums Enums { get; set; }
        public List<Variable> vars { get; set; }
        public SysParams sysParams { get; set; }
    }

    public class Scripting
    {
        Helpers helpers = Program.helpers;
        Ini ini = new Ini();

        List<Variable> variables = new List<Variable>();

        public Scripting() { }
        public Scripting(Ini ini, List<Variable> variables)
        {
            this.ini = ini;
            this.variables = variables;
        }

        public void RunScripts(ref RetValues retVal)
        {

            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            Enums.ScriptType scriptType = Enums.ScriptType.error;

            #region <Check if script is valid>
            GetLanguage(ref retVal, ref scriptType, ini.SysParams);
            if (retVal.error || scriptType == Enums.ScriptType.error)
            {
                if (!retVal.error)
                {
                    helpers.SetError(ref retVal, 10009000, @"Unknow script language.");
                }
                return;
            }
            #endregion </Check if script is valid>

            #region <Loop over all scripts>
            foreach (ActionParams param in ini.ScriptParams)
            {
                string newVariableValue = string.Empty;

                #region <Get the script content from file>
                string fileName = param.Value;
                if (!File.Exists(fileName))
                {
                    helpers.SetError(ref retVal, 10009001, @"'" + fileName + @"' not found.");
                    return;
                }

                string scriptContent = File.ReadAllText(fileName, Encoding.UTF8);
                if (string.IsNullOrEmpty(scriptContent))
                {
                    helpers.SetError(ref retVal, 10009001, @"'" + fileName + @"' has no content.");
                    return;
                }
                #endregion </Get the script content from file>

                #region <Replace the variables in the script with their values>
                foreach (Variable var in variables)
                {
                    if (var.WhereTo != VariableAssignedTo.bom && var.WhereTo != VariableAssignedTo.unknown)
                    {
                        scriptContent = scriptContent.Replace(Consts.VarStart + var.Name + Consts.VarEnd, var.Value);
                    }
                }
                #endregion </Replace the variables in the script with their values>

                #region <Processes the scripts, depending on the type identified>
                switch (scriptType)
                {
                    case Enums.ScriptType.cSharp:
                        {
                            newVariableValue = RunCSharpScript(ref retVal, scriptContent, variables);
                            if (retVal.error)
                            {
                                return;
                            }
                        }
                        break;
                    case Enums.ScriptType.error:
                        {
                            helpers.SetError(ref retVal, 10009003, method + @"Unknown script type.");
                            return;
                        }
                        break;
                }
                #endregion </Processes the scripts, depending on the type identified>

                #region <Add the new varible to the variables list>
                // Create and fill in the new variable
                Variable variable = new Variable();
                variable.WhereTo = Enums.VariableAssignedTo.script;
                variable.Name = param.Name;
                variable.Value = newVariableValue;
                variable.Row = -1;
                variable.Type = Enums.VariableType.str;

                // Add the new variable to the list
                this.variables.Add(variable);
                #endregion </Add the new varible to the variables list>
            }
            #endregion </Loop over all scripts>

        }
        public void GetLanguage(ref RetValues retVal, ref Enums.ScriptType type, SysParams sysParams)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";

            try
            {
                switch (sysParams.ScriptLanguage)
                {
                    case "C#":
                        {
                            type = Enums.ScriptType.cSharp;
                        }
                        break;
                    default:
                        {
                            type = Enums.ScriptType.error;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 1009004, method + ex.Message);
            }
        }
        public string RunCSharpScript(ref RetValues retVal, string script, List<Variable> variables)
        {
            string method = @"Error in " + MethodBase.GetCurrentMethod().Name + @": ";
            string ret = string.Empty;

            try
            {
                #region <Classes and lists provided to the script>
                var options = ScriptOptions.Default
                                .AddReferences(Assembly.GetExecutingAssembly())
                                .AddImports("ADEcore");

                var globals = new Globals
                {
                    Variable = new Variable(),
                    Enums = new Enums(),
                    vars = variables,
                    sysParams = ini.SysParams
                };
                #endregion </Classes and lists provided to the script>

                var result = CSharpScript.EvaluateAsync<object>(script, options, globals);
                ret = result.Result.ToString();

                // Check if an error occurred
                if (ret.ToLower() == @"false")
                {
                    helpers.SetError(ref retVal, 10009006, method + Environment.NewLine + script);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                helpers.SetError(ref retVal, 10009005, method + ex.Message + Environment.NewLine + script);
                return ret;
            }

            return ret;
        }
    }
}
