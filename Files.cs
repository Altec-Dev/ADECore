/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

using System.Reflection;
using System.Text;

namespace ADEcore
{
    public class Files
    {
        #region <private>
        Helpers helper = new Helpers();
        #endregion </private>

        public Files()
        {
        }

        #region <Expand the path wit \ if necessary>
        public void ExpandPath(ref string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                string tmpPath = System.IO.Path.GetFullPath(path);
                if (!tmpPath.EndsWith(@"\"))
                {
                    path = tmpPath + @"\";
                }
            }
        }
        #endregion </Expand the path wit \ if necessary>

        #region <Get all file names from given directory>
        // Get a list off all file names at one time 
        public List<string> GetFiles(string path, string type, bool recursiv = false)
        {
            List<string> fileList = new List<string>();
            SearchOption searchOption = new SearchOption();

            try
            {
                searchOption = SearchOption.TopDirectoryOnly;
                if (recursiv)
                {
                    searchOption = SearchOption.AllDirectories;
                }
                fileList.AddRange(Directory.EnumerateFiles(path, type, searchOption));
            }
            catch (Exception ex)
            {
                throw;
            }

            return fileList;
        }

        // Get a list off all file names file by file 
        public IEnumerable<string> GetFilesLazy(string path, string type, bool recursiv = false)
        {
            SearchOption searchOption = new SearchOption();
            searchOption = SearchOption.TopDirectoryOnly;
            if (recursiv)
            {
                searchOption = SearchOption.AllDirectories;
            }

            var dateien = Directory.EnumerateFiles(path, type, searchOption);
            foreach (var datei in dateien)
            {
                yield return datei; // Return file by file
            }
        }
        #endregion </Get all file names from given directory>

        #region <Read text file into array in UTF-8>
        public void ReadTxtFileIntoArray(ref RetValues retVal, ref string[] array, string filename)
        {
            string method = MethodBase.GetCurrentMethod().Name + @" / ";
            try
            {
                if (!File.Exists(filename))
                {
                    helper.SetError(ref retVal, 1001000, method + "File " + filename + " not exists");
                    return;
                }
                array = File.ReadAllLines(filename, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                helper.SetError(ref retVal, 1001001, method + ex.Message);
            }
        }
        #endregion </Read text file into array in UTF-8>

    }
}
