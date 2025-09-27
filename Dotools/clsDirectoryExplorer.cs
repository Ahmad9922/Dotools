using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotools
{
    /// <summary>
    /// Provides static methods for exploring and manipulating directories.
    /// </summary>
    public static class clsDirectoryExplorer
    {
        /// <summary>
        /// Checks whether the specified directory exists.
        /// </summary>
        /// <param name="path">The path of the directory to check.</param>
        /// <returns>True if the directory exists; otherwise, false.</returns>
        public static bool IsDirectoryExists(string path)
        {
            try
            {
                return Directory.Exists(path);
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Creates a directory at the specified path if it does not already exist.
        /// </summary>
        /// <param name="Path">The path where the directory should be created.</param>
        /// <returns>
        /// A DirectoryInfo object representing the created directory, or null if the directory already exists or an error occurs.
        /// </returns>
        public static DirectoryInfo CreateDirectory(string Path)
        {
            try
            {
                if (!Directory.Exists(Path))
                {
                    return Directory.CreateDirectory(Path);
                }
            }
            catch (Exception ex) 
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }

            return null;
        }

        /// <summary>
        /// Extracts the folder name from a full directory path.
        /// </summary>
        /// <param name="Path">The full path of the directory.</param>
        /// <returns>The name of the folder.</returns>
        public static string ExtractFolderName(string Path)
        {
            return Path.Substring(Path.LastIndexOf('\\') + 1);
        }
    }
}
