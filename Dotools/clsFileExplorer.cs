using System;
using ExcelDataReader;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotools
{
    /// <summary>
    /// Provides static methods for file operations such as writing, appending, renaming, deleting, moving, and importing Excel files.
    /// </summary>
    public static class clsFileExplorer
    {
        /// <summary>
        /// Writes content to a file at the specified path using the given file mode.
        /// </summary>
        /// <param name="Path">The file path.</param>
        /// <param name="Content">The content to write.</param>
        /// <param name="Mode">The file mode (e.g., Create, Append).</param>
        /// <returns>True if the operation succeeds; otherwise, false.</returns>
        public static bool Write(string Path, string Content, FileMode Mode)
        {
            try
            {
                using (FileStream fs = new FileStream(Path, Mode))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(Content);

                    fs.Write(buffer, 0, buffer.Length);
                }

                return true;
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Writes content to a file at the specified path, overwriting any existing content.
        /// </summary>
        /// <param name="Path">The file path.</param>
        /// <param name="Content">The content to write.</param>
        /// <returns>True if the operation succeeds; otherwise, false.</returns>
        public static bool Write(string Path, string Content)
        {
            try
            {
                File.WriteAllText(Path, Content, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Appends content to the end of the specified file.
        /// </summary>
        /// <param name="Path">The file path.</param>
        /// <param name="Content">The content to append.</param>
        /// <returns>True if the operation succeeds; otherwise, false.</returns>
        public static bool Append(string Path, string Content)
        {
            try
            {
                File.AppendAllText(Path, Content, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Renames a file to the specified new name (without extension) in the same directory.
        /// </summary>
        /// <param name="Path">The original file path.</param>
        /// <param name="NewFileNameWithoutExtension">The new file name without extension.</param>
        /// <returns>The new file path if successful; otherwise, null.</returns>
        public static string Rename(string Path, string NewFileNameWithoutExtension)
        {
            try
            {
                string NewPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), NewFileNameWithoutExtension) + System.IO.Path.GetExtension(Path);

                if (!File.Exists(NewPath))
                {
                    File.Move(Path, NewPath);
                }
                else
                {
                    return null;
                }

                return NewPath;
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return null;
            }
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="Path">The file path.</param>
        /// <returns>True if the file is deleted; otherwise, false.</returns>
        public static bool Delete(string Path)
        {
            try
            {
                File.Delete(Path);
                return true;
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Searches for a file by name. (Currently always returns true.)
        /// </summary>
        /// <param name="FileName">The name of the file to search for.</param>
        /// <returns>True if the operation succeeds; otherwise, false.</returns>
        public static bool Search(string FileName)
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Moves a file from the source path to the destination path.
        /// </summary>
        /// <param name="SourceFileName">The source file path.</param>
        /// <param name="DestinationFileName">The destination file path.</param>
        /// <returns>True if the operation succeeds; otherwise, false.</returns>
        public static bool Move(string SourceFileName, string DestinationFileName)
        {
            try
            {
                File.Move(SourceFileName, DestinationFileName);
                return true;
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Moves and renames a file to the specified destination and new name.
        /// </summary>
        /// <param name="SourceFileName">The source file path.</param>
        /// <param name="DestinationFileName">The destination file path.</param>
        /// <param name="Name">The new file name (without extension).</param>
        /// <returns>True if the operation succeeds; otherwise, false.</returns>
        public static bool MoveAndRename(string SourceFileName, string DestinationFileName, string Name)
        {
            try
            {
                string NewDestinationFileName = Path.Combine(Path.GetDirectoryName(DestinationFileName), Name) + Path.GetExtension(DestinationFileName);

                File.Move(SourceFileName, NewDestinationFileName);

                return true;
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Moves and renames a file to the destination path using a new GUID as the file name.
        /// </summary>
        /// <param name="SourceFileName">The source file path.</param>
        /// <param name="DestinationFileName">The destination file path.</param>
        /// <returns>True if the operation succeeds; otherwise, false.</returns>
        public static bool MoveAndRenameWithGiud(string SourceFileName, string DestinationFileName)
        {
            return MoveAndRename(SourceFileName, DestinationFileName, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Imports the first worksheet of an Excel file into a DataTable.
        /// </summary>
        /// <param name="ExelFilePath">The path to the Excel file.</param>
        /// <returns>A DataTable containing the data from the first worksheet.</returns>
        public static DataTable ImportExelToDataTable(string ExelFilePath)
        {
            DataTable dt = null;

            using (FileStream stream = File.Open(ExelFilePath, FileMode.Open, FileAccess.Read))
            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
            {
                DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                dt = result.Tables[0];
            }

            return dt;
        }
    }
}
