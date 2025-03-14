/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

namespace ADEcore
{
    public class RetValues
    {
        public bool error { get; set; }
        public int code { get; set; }
        public string msg { get; set; }

        public RetValues()
        {
            Reset();
        }
        public void Reset()
        {
            this.error = false;
            this.code = 0;
            this.msg = string.Empty;
        }
    }
}
