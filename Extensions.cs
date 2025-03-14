/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

namespace ADEcore
{
    public static class Extensions
    {
        #region <Strings>
        public static string StringReverse(this string input)
        {
            return new string(input.StringReverse().ToArray());
        }
        #endregion </Strings>


        #region <Console>
        public static void ConsoleWriteAt(int line, string text)
        {
            Console.SetCursorPosition(0, line);
            Console.Write(text);
        }

        public static void ConsoleWriteLineAt(int line, string text)
        {
            Console.SetCursorPosition(0, line);
            Console.WriteLine(text);
        }

        public static void ResetConsole(string text)
        {
            ClearConsole();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(text);
        }

        // Clears the entire console
        public static void ClearConsole()
        {
            Console.Clear();
        }

    }
    #endregion </Console>

}
