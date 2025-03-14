/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PdfToText
{
	class FSW
	{
		object _lock = new object();
		//
		FileSystemWatcher fsw = new FileSystemWatcher();

		/*
		 * 
		 * 
		 */
		public FSW(string dir, string filter)
		{
			// All files in this directory ony are monitored by default.
			fsw.Path = dir;
			fsw.Filter = @"*.*";
			// 
			fsw.NotifyFilter = NotifyFilters.CreationTime
												| NotifyFilters.DirectoryName
												| NotifyFilters.FileName
												| NotifyFilters.LastWrite;
			//
			fsw.IncludeSubdirectories = false;
			fsw.Created += new FileSystemEventHandler(FswCreated);
			fsw.EnableRaisingEvents = true;

		}

		~FSW()
		{
			fsw.EnableRaisingEvents = false;
		}

		/*
		 *  Works, when a new file has been created
		 */
		private void FswCreated(object sender, FileSystemEventArgs e)
		{
			bool fileInUse = false;
			try
			{
				FileInfo fInfo = new FileInfo(e.FullPath);
				do
				{
					fileInUse = IsFileinUse(fInfo);
					if (fileInUse)
					{
						WaitNMSeconds(Consts.fswWaitNMseconds);
					}
				} while (fileInUse);
				//

				MessageBox.Show("File created: " + e.FullPath);

				// filter file types 
				if (Regex.IsMatch(e.FullPath, PdfToText.Consts.moitoredFiles, RegexOptions.IgnoreCase))
				{
					if (Path.GetExtension(e.FullPath).ToLower() == @"pdf")
					{

					}
					else if (Path.GetExtension(e.FullPath).ToLower() == @"txt")
					{

					}
				}
				//<your code here>
			}
			catch (Exception ex)
			{
			}
		}

		/*
		 * Checks if the file is in use
		 */
		protected virtual bool IsFileinUse(FileInfo file)
		{
			FileStream stream = null;
			try
			{
				stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
				}
			}
			return false;
		}

		/*
		 * Waits n milliseconds
		 */
		public virtual void WaitNMSeconds(int mSec)
		{
			DateTime current = DateTime.Now;
			do
			{
				Application.DoEvents();
			} while (current.AddMilliseconds(mSec) > DateTime.Now);
		}

	}
}
