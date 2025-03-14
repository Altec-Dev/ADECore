/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

using System.Text;
using System.Text.RegularExpressions;

namespace ADEcore
{
    public class Helpers
    {
        public string ErrorFileName { get; set; }
        public string FileNameDir { get; set; }
        readonly string strMsgDel = @"-----";

        // Parser mode
        public void SetError(ref RetValues retVal, int code, string message)
        {
            retVal.error = true;
            retVal.code = code;
            retVal.msg = strMsgDel + Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                + Environment.NewLine + "Error: " + code
                + Environment.NewLine + "Message: " + message;
            if (ErrorFileName != string.Empty)
            {
                retVal.msg = retVal.msg + Environment.NewLine + "File: " + ErrorFileName;
            }
        }

        // Server mode
        public void SetError(int code, string message, ref FileStream fileStream, CommandLineOptions.ServerModeOptions opts, string recipient)
        {
            SMTP smtp = new();

            //string msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t" + code + "\t" + message + Environment.NewLine;
            if (message.StartsWith(Environment.NewLine))
            {
                message = message.TrimStart(Environment.NewLine.ToCharArray());
            }
            string msg = strMsgDel + Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                + Environment.NewLine + "Error: " + code
                + Environment.NewLine + "Message: " + message;
            if (ErrorFileName != string.Empty)
            {
                msg = msg + Environment.NewLine + "File: " + ErrorFileName;
            }
            msg = msg + Environment.NewLine;

            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Flush();

            //Console.WriteLine(msg);

            if (!string.IsNullOrEmpty(recipient) && opts.EmailMode)
            {
                smtp.SendEMailViaSmtp(recipient, msg, ref opts);
            }

        }

        public void ExitWithErrorCode(RetValues retVal)
        {
            Console.Write(retVal.msg);

            Environment.Exit(retVal.code);
        }

        // Calculate the rounding accuracy from a formula
        public int GetRoundingAccuracyFromFormula(ref RetValues retval, string formula)
        {
            int ret = 0;

            var matches = Regex.Matches(formula, Consts.regExDecimal);

            if (matches.Count > 0)
            {
                int maxDecimalPlaces = matches.Cast<Match>()
                    .Select(m => m.Value.Split('.')[1].Length)
                    .Max();

                ret = maxDecimalPlaces;
            }

            return ret;
        }
    }
}

