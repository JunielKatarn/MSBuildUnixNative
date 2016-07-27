#region Using directives

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace LLVM.Build.Tasks
{
    /// <summary>
    /// This is an MSBuild Task that allows deleting files that match the 
    /// specified list of extensions inside the list of specified directories.
    /// </summary>
    public class CleanTask : Task
    {
        #region Private members

        private const string DefaultExtension = "*";
        private readonly char[] argumentSeparators = { ';' };

        private string _directories = String.Empty;
        private string _extensions  = String.Empty;

        private List<string> directoriesList = new List<string>();
        private List<string> extensionsList  = new List<string>();

        private bool usingDefaultExtension = false;

        #endregion
        
        #region Public properties

        /// <summary>
        /// Public property to indicate the list of directories where we want to find files that need to be cleaned.
        /// This is a required property, so it cannot be empty.
        /// Multiple directories must be separated by semicolon ";".
        /// </summary>
        [Required]
        public string Directories
        {
            get { return _directories; }
            set
            {
                _directories = value;
                directoriesList.Clear();
                directoriesList.AddRange(_directories.Split(argumentSeparators, StringSplitOptions.RemoveEmptyEntries));

                if (directoriesList.Count <= 0)
                {
                    throw new ArgumentNullException("DirectoriesToClean must not be empty.");
                }
            }
        }

        /// <summary>
        /// Public property to indicate the list of extensions that need to be cleaned.
        /// This is a required property, so it cannot be empty.
        /// If it is desired to delete all the files, use "*".
        /// Multiple extensions must be separated by semicolon ";".
        /// </summary>
        [Required]
        public string Extensions
        {
            get { return _extensions; }
            set
            {
                _extensions = value;
                extensionsList.Clear();
                extensionsList.AddRange(_extensions.Split(argumentSeparators, StringSplitOptions.RemoveEmptyEntries));

                if (extensionsList.Count <= 0)
                {
                    throw new ArgumentNullException("ExtensionsToClean must not be empty.");
                }

                usingDefaultExtension = extensionsList.Contains(DefaultExtension);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Deletes all the files that match the specified extension inside the desired directory.
        /// </summary>
        /// <param name="directory">The directory where the files will be searched and deleted.</param>
        /// <param name="extension">The extension of the files that need to be deleted, which needs to be provided in the format "*.ext". If all files are desired, use DefaultExtension="*".</param>
        /// <returns>True if the task was successful, false otherwise.</returns>
        private bool DeleteFilesInDirectoryByExtension(string directory, string extension)
        {
            try
            {
                string[] filePaths = Directory.GetFiles(directory, extension, SearchOption.TopDirectoryOnly);

                foreach (string filePath in filePaths)
                {
                    string trimmedFilePath = filePath.Trim();
                    Log.LogMessage("Cleaning file: {0}", trimmedFilePath);
                    File.Delete(trimmedFilePath);
                }
                
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }

            return true;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// The public execution method required by MSBuild.
        /// It will delete all the files that match the selected extensions
        /// located inside the specified directories.
        /// </summary>
        /// <returns>True if the task was successful, false otherwise.</returns>
        public override bool Execute()
        {
            foreach (string directory in directoriesList)
            {
                if (!Directory.Exists(directory))
                {
                    Log.LogErrorFromException(new DirectoryNotFoundException("Directory does not exist: " + directory));
                    return false;
                }

                if (usingDefaultExtension)
                {
                    Log.LogWarning("You selected * as extension, which will delete all the files in the selected directories.");
                    if (!DeleteFilesInDirectoryByExtension(directory, DefaultExtension))
                    {
                        return false;
                    }
                }

                else
                {
                    foreach (string extension in extensionsList)
                    {
                        string completeExtension = extension;
                        if(!extension.StartsWith("*."))
                        {
                            completeExtension = "*." + extension;
                        }
                        if (!DeleteFilesInDirectoryByExtension(directory, completeExtension))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
