/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

using IniParser;
using IniParser.Model;
using System.Reflection;
using System.Text;

// Error range 1000.000 - last error 1000.022
namespace ADEcore
{
    // https://github.com/rickyah/ini-parser/tree/development
    public class Ini
    {
        #region <private>
        private string _filename;
        public string Filename
        {
            get
            {
                return _filename;
            }

            set
            {
                if (File.Exists(value))
                {
                    try
                    {
                        Parser = new FileIniDataParser();
                        Data = Parser.ReadFile(value, Encoding.UTF8);
                        _filename = value;
                    }
                    catch (Exception ex)
                    {
                        _filename = string.Empty;
                        parseError = true;
                        parseErrorMessage = ex.Message;
                    }
                }
            }
        }

        private FileIniDataParser Parser { get; set; }
        private IniData Data { get; set; }
        private Helpers helper = new Helpers();
        #endregion </private>

        #region <Error handling>
        public bool parseError { get; set; } = false;
        public string parseErrorMessage { get; private set; } = string.Empty;
        #endregion </Error handling>

        #region <public>
        public SysParams SysParams = new SysParams();
        public DocParams DocParams = new DocParams();
        public BpParams BpParams = new BpParams();
        public BOMArea BOMArea = new BOMArea();
        public BOMRow BOMRow = new BOMRow();
        public BOMColumns BOMColumns = new BOMColumns();
        public List<BOMParams> BOMParams = new List<BOMParams>();
        public List<UdfParams> UdfParams = new List<UdfParams>();
        public List<ActionParams> CalculationParams = new List<ActionParams>();
        public List<ActionParams> ScriptParams = new List<ActionParams>();
        public OutputParams OutputParams = new OutputParams();
        public List<SourceDestinationFile> OutputFiles = new List<SourceDestinationFile>();
        #endregion </public>

        #region <Cotructor>
        public Ini() { }

        public Ini(ref RetValues retVal, string filename)
        {
            this.Filename = filename;
            if (!String.IsNullOrEmpty(this.Filename))
                try
                {
                    if (File.Exists(Filename))
                    {
                        Parser = new FileIniDataParser();
                        Data = Parser.ReadFile(filename, Encoding.UTF8);
                        GetAllSectionsParams(ref retVal);
                    }
                }
                catch (Exception ex)
                {
                }
        }
        #endregion </Cotructor>

        #region <Get all parameters for all sections from the ini file and write them to the respective classes>
        public void GetAllSectionsParams(ref RetValues retVal)
        {
            string sectionName = string.Empty;
            bool useBom = true;

            // SysParam is mandatory
            #region <SysParams>
            GetSysParams(ref retVal, out SysParams);
            if (retVal.error)
            {
                return;
            }
            #endregion </SysParams>

            // Document is mandatory
            #region <DocParams>
            GetDocParams(ref retVal, out DocParams);
            if (retVal.error)
            {
                return;
            }
            #endregion </DocParams>

            // Business partner is mandatory
            #region <BpParams>
            GetBpParams(ref retVal, out BpParams);
            if (retVal.error)
            {
                return;
            }
            #endregion </BpParams>

            // If BOMArea is not specified, BOMColumns and BOMParameter will not be evaluated.
            #region <BOMArea>
            GetBOMArea(ref retVal, out BOMArea);
            if (retVal.error)
            {
                if (retVal.code != 1000000)
                {
                    return;
                }
                useBom = false;
                retVal.Reset();
            }
            #endregion </BOMArea>

            // If BOMArea is not specified, BOMColumns will not be evaluated.
            #region <BOMRow>
            if (useBom)
            {
                GetBOMRow(ref retVal, out BOMRow);
                if (retVal.error)
                {
                    return;
                }
            }
            #endregion </BOMRow>

            // If BOMArea is not specified, BOMColumns will not be evaluated.
            #region <BOMColumns>
            if (useBom)
            {
                GetBOMColumns(ref retVal, out BOMColumns);
                if (retVal.error)
                {
                    return;
                }
            }
            #endregion </BOMArea>

            // If BOMArea is not specified, BOMParameter will not be evaluated.
            #region <BOMParams>
            GetBOMParams(ref retVal, out BOMParams);
            if (retVal.error)
            {
                return;
            }
            #endregion </BOMArea>

            // At least one user-defined action must be defined, otherwise an error will be thrown.
            #region <UdfParams>
            GetUdfParams(ref retVal, out UdfParams);
            if (retVal.error)
            {
                return;
            }
            #endregion </UDFParams>

            #region <Calculation>
            GetCalculationParams(ref retVal);
            if (retVal.error)
            {
                return;
            }
            #endregion </Calculation>

            #region <Script>
            GetScriptParams(ref retVal);
            if (retVal.error)
            {
                return;
            }
            #endregion </Script>

            #region <Output>
            GetOutputParams(ref retVal);
            if (retVal.error)
            {
                return;
            }
            #endregion </Output>

        }
        #endregion </Get all parameters for all sections from the ini file and write them to the respective classes>

        #region <Get all parameters to the SysParams>
        public void GetSysParams(ref RetValues retVal, out SysParams SysParams)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = "SysParams";
            SysParams = new SysParams();
            try
            {
                foreach (SectionData section in Data.Sections)
                {
                    if (section.SectionName == sectionName)
                    {
                        SysParams.VarName = section.SectionName;
                        foreach (KeyData key in section.Keys)
                        {
                            PropertyInfo propInfo = SysParams.GetType().GetProperty(key.KeyName);
                            if (propInfo != null)
                            {
                                if (!String.IsNullOrEmpty(key.Value))
                                {
                                    propInfo.SetValue(SysParams, key.Value.Trim());
                                }
                            }
                        }
                        break;
                    }
                }
                if (String.IsNullOrEmpty(SysParams.VarName))
                {
                    helper.SetError(ref retVal, 1000001, method + "Section " + sectionName + " not found.");
                }
                else
                {
                    retVal.Reset();
                }
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000002, method + "Error in section [" + sectionName + "]" + Environment.NewLine + ex.Message);
            }
            if (retVal.error)
            {
                return;
            }
        }
        #endregion </Get all parameters to the SysParams>

        #region <Get all parameters to the DocParams>
        public void GetDocParams(ref RetValues retVal, out DocParams DocParams)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = "DocParams";
            DocParams = new DocParams();
            try
            {
                foreach (SectionData section in Data.Sections)
                {
                    if (section.SectionName == sectionName)
                    {
                        DocParams.VarName = section.SectionName;
                        foreach (KeyData key in section.Keys)
                        {
                            PropertyInfo propInfo = DocParams.GetType().GetProperty(key.KeyName);
                            if (propInfo != null)
                            {
                                if (!String.IsNullOrEmpty(key.Value))
                                {
                                    propInfo.SetValue(DocParams, key.Value.Trim());
                                }
                            }
                        }
                        break;
                    }
                }
                if (String.IsNullOrEmpty(DocParams.VarName))
                {
                    helper.SetError(ref retVal, 1000003, method + "Section " + sectionName + " not found.");
                }
                else
                {
                    retVal.Reset();
                }
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000004, method + "Error in section [" + sectionName + "]" + Environment.NewLine + ex.Message);
            }
            if (retVal.error)
            {
                return;
            }
        }
        #endregion </Get all parameters to the DocParams>

        #region <Get all parameters to the BpParams>
        public void GetBpParams(ref RetValues retVal, out BpParams BpParams)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = "BpParams";
            BpParams = new BpParams();
            try
            {
                foreach (SectionData section in Data.Sections)
                {
                    if (section.SectionName == sectionName)
                    {
                        BpParams.VarName = section.SectionName;
                        foreach (KeyData key in section.Keys)
                        {
                            PropertyInfo propInfo = BpParams.GetType().GetProperty(key.KeyName);
                            if (propInfo != null)
                            {
                                if (!String.IsNullOrEmpty(key.Value))
                                {
                                    propInfo.SetValue(BpParams, key.Value.Trim());

                                }
                            }
                        }
                        break;
                    }
                }
                if (String.IsNullOrEmpty(BpParams.VarName))
                {
                    helper.SetError(ref retVal, 1000005, method + "Section " + sectionName + " not found.");
                }
                else
                {
                    retVal.Reset();
                }
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000006, method + "Error in section [" + sectionName + "]" + Environment.NewLine + ex.Message);
            }
            if (retVal.error)
            {
                return;
            }
        }
        #endregion </Get all parameters to the BpParams>

        #region <Get all parameters to the BOMArea>
        public void GetBOMArea(ref RetValues retVal, out BOMArea BomArea)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = "BOMArea";
            BomArea = new BOMArea();
            try
            {
                foreach (SectionData section in Data.Sections)
                {
                    if (section.SectionName == sectionName)
                    {
                        BomArea.VarName = section.SectionName;
                        foreach (KeyData key in section.Keys)
                        {
                            PropertyInfo propInfo = BomArea.GetType().GetProperty(key.KeyName);
                            if (propInfo != null)
                            {
                                if (!String.IsNullOrEmpty(key.Value))
                                {
                                    propInfo.SetValue(BomArea, key.Value.Trim());
                                }
                            }
                        }
                        break;
                    }
                }
                if (String.IsNullOrEmpty(BomArea.VarName))
                {
                    helper.SetError(ref retVal, 1000007, method + "Section " + sectionName + " not found.");
                }
                else
                {
                    retVal.Reset();
                }
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000008, method + "Error in section [" + sectionName + "]" + Environment.NewLine + ex.Message);
            }
            if (retVal.error)
            {
                return;
            }
        }
        #endregion </Get all parameters to the BOMArea>

        #region <Get all parameters to the BOMRow>
        public void GetBOMRow(ref RetValues retVal, out BOMRow BomRow)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = "BOMRow";
            BomRow = new BOMRow();
            try
            {
                foreach (SectionData section in Data.Sections)
                {
                    if (section.SectionName == sectionName)
                    {
                        BomRow.VarName = section.SectionName;
                        foreach (KeyData key in section.Keys)
                        {
                            PropertyInfo propInfo = BomRow.GetType().GetProperty(key.KeyName);
                            if (propInfo != null)
                            {
                                if (!String.IsNullOrEmpty(key.Value))
                                {
                                    propInfo.SetValue(BomRow, key.Value.Trim());
                                }
                            }
                        }
                        break;
                    }
                }
                if (String.IsNullOrEmpty(BomRow.VarName))
                {
                    helper.SetError(ref retVal, 1000009, method + "Section " + sectionName + " not found.");
                }
                else
                {
                    retVal.Reset();
                }
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000010, method + "Error in section [" + sectionName + "]" + Environment.NewLine + ex.Message);
            }
            if (retVal.error)
            {
                return;
            }
        }
        #endregion </Get all parameters to the BOMArea>

        #region <Get all parameters to the BOMColumns>
        public void GetBOMColumns(ref RetValues retVal, out BOMColumns BomColumns)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = "BOMColumns";
            BomColumns = new BOMColumns();
            try
            {
                foreach (SectionData section in Data.Sections)
                {
                    if (section.SectionName == sectionName)
                    {
                        BomColumns.VarName = section.SectionName;
                        foreach (KeyData key in section.Keys)
                        {
                            PropertyInfo propInfo = BomColumns.GetType().GetProperty(key.KeyName);
                            if (propInfo != null)
                            {
                                if (!String.IsNullOrEmpty(key.Value))
                                {
                                    propInfo.SetValue(BomColumns, key.Value.Trim());
                                }
                            }
                        }
                        break;
                    }
                }
                if (String.IsNullOrEmpty(BomColumns.VarName))
                {
                    helper.SetError(ref retVal, 1000011, method + "Section " + sectionName + " not found.");
                }
                else
                {
                    retVal.Reset();
                }
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000012, method + "Error in section [" + sectionName + "]" + Environment.NewLine + ex.Message);
            }
            if (retVal.error)
            {
                return;
            }
        }
        #endregion </Get all parameters to the BOMColumns>

        #region <Get all parameters to the BOMParams>
        public void GetBOMParams(ref RetValues retVal, out List<BOMParams> BomParams)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = "BOMParams";
            BomParams = new List<BOMParams>();
            try
            {
                foreach (SectionData section in Data.Sections)
                {

                    if (section.SectionName == sectionName)
                    {
                        // Initialise the new parameters list
                        int columns = Convert.ToInt32(BOMColumns.ColumnsCounter);
                        for (int i = 0; i <= columns; i++)
                        {
                            BOMParams bomPos = new BOMParams();
                            BOMParams.Add(bomPos);
                        }

                        foreach (KeyData key in section.Keys)
                        {
                            string[] item = key.KeyName.Split(new string[] { Consts.bomParamsDelimiter }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            int counter = Convert.ToInt32(item[0]);

                            PropertyInfo propInfo = BOMParams[counter].GetType().GetProperty(item[1]);
                            if (propInfo != null)
                            {
                                if (!String.IsNullOrEmpty(key.Value))
                                {
                                    propInfo.SetValue(BOMParams[counter], key.Value.Trim());
                                }
                            }
                        }
                        #region <Split the RegExPattern if replace statements are existing>
                        for (int i = 0; i < BOMParams.Count; i++)
                        {
                            string[] regExPattern = BOMParams[i].RegExPattern.Split(new string[] { BOMParams[i].RegExReplacePatternDelimiter }, StringSplitOptions.TrimEntries);
                            BOMParams[i].RegExPattern = regExPattern[0];
                            if (regExPattern.Length > 1)
                            {
                                string[] tmpArray = new string[regExPattern.Length - 1];
                                for (int j = 1; j < regExPattern.Length; j++)
                                {
                                    tmpArray[j - 1] = regExPattern[j];
                                }
                                BOMParams[i].RegExReplacePattern = tmpArray;
                            }
                        }
                        #endregion </Split the RegExPattern if replace statements are existing>

                        break;
                    }
                }
                if (BomParams.Count == 0)
                {
                    helper.SetError(ref retVal, 1000013, method + "Section " + sectionName + " not found.");
                }
                else
                {
                    retVal.Reset();
                }
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000014, method + "Error in section [" + sectionName + "]" + Environment.NewLine + ex.Message);
            }
            if (retVal.error)
            {
                return;
            }
        }
        #endregion </Get all parameters to the BOMParams>

        #region <Get all parameters to the UDFParams>
        public void GetUdfParams(ref RetValues retVal, out List<UdfParams> UdfParams)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = "UdfParams";
            UdfParams = new List<UdfParams>();
            try
            {
                foreach (SectionData section in Data.Sections)
                {
                    sectionName = ":";
                    if (section.SectionName.StartsWith(sectionName))
                    {
                        UdfParams udfParams = new UdfParams();
                        udfParams.VarName = section.SectionName.Substring(1);

                        foreach (KeyData key in section.Keys)
                        {
                            PropertyInfo propInfo = udfParams.GetType().GetProperty(key.KeyName);
                            if (propInfo != null)
                            {
                                if (!String.IsNullOrEmpty(key.Value))
                                {
                                    propInfo.SetValue(udfParams, key.Value.Trim());
                                }
                            }
                        }

                        this.UdfParams.Add(udfParams);

                        #region <Split the RegExPattern if replace statements are existing>
                        for (int i = 0; i < UdfParams.Count; i++)
                        {
                            string[] regExPattern = UdfParams[i].RegExPattern.Split(new string[] { UdfParams[i].RegExReplacePatternDelimiter }, StringSplitOptions.TrimEntries);
                            UdfParams[i].RegExPattern = regExPattern[0];
                            if (regExPattern.Length > 1)
                            {
                                string[] tmpArray = new string[regExPattern.Length - 1];
                                for (int j = 1; j < regExPattern.Length; j++)
                                {
                                    tmpArray[j - 1] = regExPattern[j];
                                }
                                UdfParams[i].RegExReplacePattern = tmpArray;
                            }
                        }
                        #endregion </Split the RegExPattern if replace statements are existing>
                    }
                }
                if (UdfParams.Count == 0)
                {
                    helper.SetError(ref retVal, 1000015, method + "Section " + sectionName + " not found.");
                }
                else
                {
                    retVal.Reset();
                }
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000016, method + "Error in section [" + sectionName + "]" + Environment.NewLine + ex.Message);
            }
            if (retVal.error)
            {
                return;
            }
        }
        #endregion </Get all parameters to the UDFParams>

        #region <Get all parameters to the ~CalculationParams>
        private void GetCalculationParams(ref RetValues retVal)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = Consts.calculationBomSumSection;

            try
            {
                foreach (var section in Data.Sections)
                {
                    if (section.SectionName == sectionName)
                    {

                        foreach (var key in section.Keys)
                        {
                            ActionParams actionParams = new ActionParams();

                            actionParams.Name = key.KeyName;
                            actionParams.Value = key.Value;

                            CalculationParams.Add(actionParams);
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000017, method + ex.Message);
            }
        }
        #endregion </Get all parameters to the ~CalculationParams>

        #region <Get all parameters to the ~ScriptParams>
        private void GetScriptParams(ref RetValues retVal)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = Consts.ScriptSection;

            try
            {
                foreach (var section in Data.Sections)
                {
                    if (section.SectionName == sectionName)
                    {

                        foreach (var key in section.Keys)
                        {
                            ActionParams actionParams = new ActionParams();

                            actionParams.Name = key.KeyName;
                            actionParams.Value = key.Value;

                            ScriptParams.Add(actionParams);
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000018, method + ex.Message);
            }
        }
        #endregion </Get all parameters to the ~ScriptParams>

        #region <Get all parameters to the ~OutputParams>
        private void GetOutputParams(ref RetValues retVal)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            string sectionName = Consts.ScriptSection;

            try
            {
                OutputParams.outputFileNames = Data[Consts.OutputSection][Consts.outputFileNamesKey];
                string[] fileNames = OutputParams.outputFileNames.Split(new string[] { Consts.FilesDelimiter }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string fileNamePair in fileNames)
                {
                    SourceDestinationFile filePair = new SourceDestinationFile();
                    string[] files = fileNamePair.Split(new string[] { Consts.FileNamesDelimiter }, StringSplitOptions.RemoveEmptyEntries);

                    if (files.Length < 2)
                    {
                        helper.SetError(ref retVal, 1000019, method + @"Wrong output file pair given.");
                        return;
                    }

                    if (string.IsNullOrEmpty(files[0]))
                    {
                        helper.SetError(ref retVal, 1000020, method + @"No output template given.");
                        return;
                    }

                    if (!File.Exists(files[0]))
                    {
                        helper.SetError(ref retVal, 1000021, method + @"'" + files[0] + "' not found.");
                        return;
                    }

                    filePair.Source = files[0];
                    filePair.Destination = files[1];
                    OutputFiles.Add(filePair);
                }
                Program.OutputFiles = OutputFiles;
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1000022, method + ex.Message);
            }
        }
        #endregion </Get all parameters to the ~OutputParams>

    }
}

