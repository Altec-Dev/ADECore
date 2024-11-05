# ADECore

**A**utomatic **D**ocument **E**ntry **Core** is the core of several tools that enable automatic document entry. It extracts data from text files using regular expressions to process them with third-party tools.

The programme is used in the company to automatically import business documents, mainly in PDF format, into the ERP system or to prepare them for further processing in Excel.

It is a further development of [**BiI Automatic Document Entry**](https://bii.erppdm.com/automaticdocumententry.html)

# Features

**ADECore** is a console application that can run as both a server and a parser.

In server mode, the programme monitors a specified directory for new PDF files at a defined interval. New files are prepared for data extraction, and the programme is started as a parser to extract and store the data.

The data preparation is done by the Linux tool **Poppler**. It converts the PDF file into a text file so that ADECore can extract the required information and save it in a file. This data can then be used with other programmes for further processing.

A mathematical parser is integrated to allow simple calculations to be carried out with the values obtained. This ensures that all items have been found. This is done, for example, by calculating the sum of the individual prices of the items and comparing them with the total of the document.

If functions are needed that go beyond the functions provided in the framework, additional  functions can be created in C# scripts. 

# Prerequisites

**.NET CORE 8** \
AutoMapper (https://automapper.org/) \
commandlineparser (https://github.com/commandlineparser/commandline) \
ini-parser (https://github.com/rickyah/ini-parser) \
MathParserTK (https://github.com/kirnbas/MathParserTK)

pdftotext (https://poppler.freedesktop.org/) \
inotifywait (sudo apt install inotify-tools) \
WSL 2 (optional if no Linux server is availabl) 

# Command line parameters

### Server mode

- `‘m’`, `‘monitoredDirectory’`, Required = true, HelpText = ‘Directory that is monitored for new PDF files’
- `‘p’`, `‘popplerDirectory’`, Required = true, HelpText = ‘Directory that can be used by the PDF tool Poppler’
- `‘b’`, `‘bpDirectory’`, Required = true, HelpText = ‘Directory with the INI files of the business partners’
- `‘i’`, `‘checkIntervall’`, Required = true, HelpText = ‘Check interval in seconds’
- `‘e’`, `‘emailMode’`, Required = false, HelpText = ‘Emails are automatically sent to this address based on the last directory name’
- `‘d’`, `‘Domain’`, Required = false, HelpText = ‘Domain to which the emails should be sent. Requires emailMode’
- `‘s’`, `‘smtpServer’`, Required = false, HelpText = ‘SMTP server. Requires emailMode’
- `‘o’`, `‘smtpPort’`, Required = false, HelpText = ‘SMTP port. Requires emailMode’
- `‘r’`, `‘smtpSender’`, Required = false, HelpText = ‘SMTP sender. Requires emailMode’
- `‘u’`, `‘smtpUser’`, Required = false, HelpText = ‘SMTP user. Requires emailMode’
- `‘w’`, `‘smtpPassword’`, Required = false, HelpText = ‘SMTP password. Requires emailMode’
- `‘j’`, `‘smtpSubject’`, Required = false, HelpText = ‘E-mail subject. Requires emailMode’
- `‘l’`, `‘smtpEnableSSL’`, Required = false, HelpText = ‘Activate SSL for SMTP. Requires emailMode’
- `‘a’`, `‘smtpAdmin’`, Required = false, HelpText = ‘Administrator email address. Requires emailMode’
- `‘f’`, `‘LogFile’`, Required = true, HelpText = ‘File name of the log file in which the events are logged’
- `‘t’`, `‘timePerFile’`, Required = true, HelpText = ‘Maximum waiting time in seconds for Poppler before an error is triggered’

### Example

**Without e-mail notification** 

```
ADECore.exe server -m ‘C:\ADECore\MonitoredDirectory’ -p ‘\\wsl.localhost\Ubuntu\home\linux\poppler’ -b ‘C:\ADECore\BpDir’ -f ‘C:\ADECore\Logs\ADECore.log’ -i 5 -t 20
```

**With e-mail notification** 

To send e-mails, PDF files must be stored in a directory named with the recipient's address. The directory name for the recipient `m.mustermann@domain` must then be `m.mustermann`.

```
ADECore.exe server -m ‘C:\ADECore\MonitoredDirectory’ -p ‘\\wsl.localhost\Ubuntu\home\linux\poppler’ -b ‘C:\ADECore\BpDir’ -f ‘C:\ADECore\Log s\ADECore.log’ -i 5 -e -d “domain” -s “SMTP Server” -o 25 -r “noreply@domain” -j “ADEcore” -l -a “admin@domain” -t 20
- ‘f’, ‘file’, Required = true, HelpText = ‘Text file to parse
```

### Parser mode

- `'d'`, `"directory`", Required = true, HelpText = "Directory with the business partners INI files"
- `'f'`, `"file"`, Required = true, HelpText = "Text file to parse"

### Example:

```
`ADECore.exe parse -d “C:\ADECore\BpDir” -f “C:\ADECore\Parse\GP\Dokument.txt”`
```

# Workflow
![Image](https://github.com/user-attachments/assets/98ef1d29-2bf9-43e6-89f5-b99344fd6ddf)

# Documentation of the INI file

### General notes
The entire programme configuration is stored in the INI file. A separate INI file is created for each business partner and each of their documents. This file contains the variables and ECMAScript regex patterns for value extraction. There are two operational modes: one for document information and one for document items (BoM).
- **Document items:** Detected lines are stored individually in a list, and each line is processed separately. Multi-line search patterns are not supported.
- **Document information:** Multi-line RegEx patterns are supported.

To minimise the complexity of RegEx patterns, values can be further processed via RegEx. Unnecessary patterns can be deleted or replaced.

### Tips
- **Case sensitivity:** Variable names are case-sensitive; for instance, `regExPattern` is not equivalent to `RegExPattern`.
- **Value trimming:** Any value returned from a RegEx search is automatically trimmed of leading and trailing spaces.
- **RegEx:** Line breaks can be matched using `\r?\n` or `([\s\S]*?)`. Examples of amounts matching the pattern `(\ *\d{1,3}(?:\.\d{3})*(?:,\d+))` include `1,02`, `100,02`, and `1.000,02`.
- **Character encoding:** UTF-8

## System variables

**Section:** `[SysParams]`

These variables are valid globally for the entire document.

- `SysDecimalSeparator`: system's decimal separator (default: `.`).
- `SysThousandsSeparators`: system's thousands separator (default: `,`).
- `DocDecimalSeparator`: document's decimal separator (default: `,`).
- `DocThousandsSeparators`: thousands separator in the document (default: `.`).
- `DefValueString`: default value for string variables (default: `string.Empty`).
- `DefValueDouble`: default value for numeric variables (default: `0`).
- `ScriptLanguage`: script language for user-defined scripts (currently only C#).

## Business Partner Information

**Section:** `[DocParams]`

Main parameters for classifying the document and the business partner. These can be, for example, information from the ERP system for later import.

- `Name`: Unique name of the business partner.
- `Id`: Unique identification ID of the business partner in the entire system.
- `Type`: Description of the document, must be unique within the business partner.

**Section:** `[BpParams]`

Parameters for identifying the business partner and the document category using regular expressions.

### Mandatory

- `Name`: RegEx pattern to identify the business partner (e.g. `(\ *business partner\ +)`).
- `NameRelativeRow`: Relative row to search for the value (0 = same row, negative values for previous rows, positive for subsequent ones).
- `NameAbsolutRow`: Absolute row in the document for navigation (will be ignored if `NameRelativeRow` is equal to 0).

- `Type`: RegEx pattern to identify the document type (e.g. `(\ *Offer\ +)(\d+)`).
- `TypeRelativeRow`: Relative row to search for the value.
- `TypeAbsolutRow`: Absolute row in the document for navigation (will be ignored if `TypeRelativeRow` is equal to 0).

### Ooptional

- `Id`: RegEx pattern to identify the business partner ID. If empty, it is ignored.
- `IdRelativeRow`: Relative row to search for the value.
- `IdAbsolutRow`: Absolute row in the document for navigation (ignored if `IdRelativeRow` is equal to 0).

## Document Items (BOM)

### BOM Area

**Section:** `[BOMArea]`

Defines the area in which the parts list items are found in the document.

- `Start`: RegEx pattern that identifies the beginning of the parts list.
- `StartFromTop`: Determines whether the search starts from the top (1) or bottom (0) (default: 1).
- `End`: RegEx pattern that identifies the end of the piece list.
- `EndFromTop`: Determines whether the search starts from the top (0) or bottom (1) (default: 0).

### BOM rows

**Section:** `[BOMRow]`

Defines how each individual BOM item is recognised.

- `Start`: RegEx pattern that identifies the start of a BOM item.
- `End`: RegEx pattern that identifies the end of a BOM item. If empty, it is assumed that all information is on one line.

### BOM Columns

**Section:** `[BOMColumns]`

- `ColumnsCounter`: Number of columns in each BOM line (starting with 0).

### BOM parameters

**section:** `[BOMParams]`

Specific parameters are defined for each column. The first column (`0_...`) is explained here as an example. For further columns, simply increase the number (e.g. `1_...`, `2_...`).

- `0_RelativeRow`: Relative row within the BOM item (default: 0).
- `0_HitNumber`: Which match found should be used (default: -1, meaning that all groups are merged).
- `0_IsMandatory`: Indicates whether the value is mandatory (1 for Yes, 0 for No).
- `0_IsNumber`: Indicates whether the value is numeric. If yes, the decimal and thousand separator is adjusted (1 for yes, 0 for no).
- `0_RegExReplacePatternDelimiter`: Delimiter to separate search and replace patterns (default: `<@>`).
- `0_RegExReplaceValueDelimiter`: Delimiter for separating replacement patterns (default: `<|>`).
- `0_RegExPattern`: RegEx pattern for searching the value.
- `0_VarName`: Unique variable name for later use.

**Note:**
If the default value of a parameter is not adjusted, it does not have to be listed in the INI file.

## User-defined variables

**Section:** `[<variable name>]`

Defines variables that can be used for user-defined actions.

Example for the variable `QuotationNr`:

```ini
[:QuotationNr]
RegExPattern=(\ *Offer\ +)(\d+)
IsMandatory=1
RegExReplacePatternDelimiter=<@>
RegExReplaceValueDelimiter=<|>
RemoveEmptyLines=0
IgnoreFirstLine=0
IgnoreLastLine=0
HitNumber=1
IsNumber=0
```

**Parameter description:**

- `RegExPattern`: RegEx pattern for extracting the value.
- `IsMandatory`: Indicates whether the value is mandatory (1 for yes, 0 for no).
- `RegExReplacePatternDelimiter`: Delimiter for RegEx replacements (default: `<@>`).
- `RegExReplaceValueDelimiter`: Delimiter for replacement values (default: `<|>`).
- `RemoveEmptyLines`: Removes empty lines after applying the rules (1 for yes, 0 for no).
- `IgnoreFirstLine`: Ignores the first line of the found block (1 for yes, 0 for no).
- `IgnoreLastLine`: Ignore the last line of the block found (1 for Yes, 0 for No).
- `HitNumber`: Which group to use in the match found (default: -1).
- `IsNumber`: Indicates whether the value is numeric (1 for Yes, 0 for No).

**Note:**
If the default value of a parameter is not adjusted, it does not need to be listed in the INI file.

## User-defined calculations

**Section:** `[~Calculation]`

Enables the definition of calculations and validations.

- `CalculationBomSum`: System variable for calculating the sum of the values from the BOM items and comparing them with a reference value.

Example:

```ini
CalculationBomSum=<bomRowTotalPrice> equal <QuotationTotal>
```

- To disable the calculation, use:

```ini
CalculationBomSum=NoCalculationBomSum
```

## User-defined scripts

**Section:** `[~Script]`

User-defined scripts can be inserted in the specified script language.

## Output

**section:** `[~Output]`

Defines how and where the results are output.

- `FileNames`: system variable for specifying the source and destination files.
- Format: `source1;destination1::source2;destination2`
- Example:

```
FileNames=C:\ADECore\BpDir\GP\GP_Angebot.template;C:\ADECore\OutputDir\<Name>_<Type>_<QuotationNr>.txt
```

`<Name>`, `<Type>`, and `<QuotationNr>` are placeholders that will be replaced by the corresponding values.

## Appendix: Tips and notes

- **RegEx tests:** Use tools such as [regex101.com](https://regex101.com/) to validate your RegEx patterns.
- **RegExReplacePatternDelimiter** and **RegExReplacePatternDelimiter:** These can be used as often as you like in succession to replace patterns in the found string. After each replacement, any spaces at the beginning and end are truncated. The replacement is carried out from left to right and always reworks the previous result.
- **Line breaks in RegEx:** Note that `\r\n`, `\r` and `\n` are platform-dependent. Use patterns such as `\r?\n` and `([\s\S]*?)` for platform-independent line breaks.
- **Placeholders in filenames:** To avoid errors, make sure that the placeholders used in `FileNames` are actually defined.
- **Script languages:** Currently, only C# is supported as a script language. Other languages such as VB may be added in future versions.

Definition of the class provided for scripts that can be read or modified
```csharp
public class Variable
{
    public int Row { get; set; } = -1;
    public string Name { get; set; } = string.Empty;
    public Enums.VariableType Type { get; set; } = Enums.VariableType.unknown;
    public Enums.VariableAssignedTo WhereTo { get; set; } = Enums.VariableAssignedTo.unknown;
    public string Value { get; set; } = string.Empty;
}
```

This script swaps values for a pair of variables with the number of factors of two.

```csharp
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

const RegexOptions regExOptions = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.ECMAScript | RegexOptions.Compiled;

string unitPair = @"Paar";
string newUnitPair = @"Stk.";
string quantityUnit = @"bomRowQtyUnit";
string quantity = @"bomRowQty";

for (int i = 0; i < vars.Count; i++)
{
    if (vars[i].Name == quantityUnit)
    {
        if (vars[i].Value == unitPair)
        {
            for (int j = 0; j < vars.Count; j++)
            {
                if (vars[j].Name == quantity && vars[j].Row == vars[i].Row)
                {
                    vars[j].Value = (Convert.ToInt32(vars[j].Value) * 2).ToString();
                    break;
                }
            }
            vars[i].Value = newUnitPair;
        }
    }
}

return true;
```

# Note
This documentation serves as a guide for configuring the INI file. Make sure that all RegEx patterns and paths are adapted to your specific requirements.

---

# Linux

There are many ways to achieve the result shown below. This is how we solved it.

Create the file poppler.sh with this content and set it as executable.
```
#!/bin/bash
SCRIPT_NAME=‘poppler.sh’
if pgrep -x ‘$SCRIPT_NAME’ > /dev/null && [ $$ -ne $(pgrep -xo ‘$SCRIPT_NAME’) ]; then
    exit 1
fi
WATCHED_DIR=‘/home/linux/poppler/’
inotifywait -m -e close_write --format ‘%w%f’ ‘${WATCHED_DIR}’ | while read FILE
do
  pdftotext -q -layout -eol dos ${FILE} ${FILE}.txt
done
```
At the end of the file `.profile`, insert the path to the file `./poppler.sh &`. The `&` is used to execute the script in the background.

