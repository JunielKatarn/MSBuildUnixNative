#region Using directives

using System;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

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

		/// <summary>
		/// Deletes all the files that match the specified extension inside the desired directory.
		/// </summary>
		/// <param name="directory">The directory where the files will be searched and deleted.</param>
		/// <param name="extension">
		///	 The extension of the files that need to be deleted, which needs to be provided in the format
		///	 "*.ext". If all files are desired, use DefaultExtension="*".
		/// </param>
		/// <returns>True if the task was successful, false otherwise.</returns>
		private bool DeleteFilesInDirectoryByExtension(string directory, string extension)
		{
			try
			{
				string[] filePaths = Directory.GetFiles(directory, extension, SearchOption.TopDirectoryOnly);

				foreach (string filePath in filePaths)
				{
					string trimmedFilePath = filePath.Trim();
					if (PrintOnly)
					{
						Log.LogWarning($"PrintOnly mode. Skiping: {trimmedFilePath}");
						continue;
					}
					Log.LogMessage("Deleting file: {0}", trimmedFilePath);
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

		#region Public properties

		/// <summary>
		/// Public property to indicate the list of directories where we want to find files that need to be cleaned.
		/// This is a required property, so it cannot be empty.
		/// Multiple directories must be separated by semicolon ";".
		/// </summary>
		[Required]
		public ITaskItem[] Directories { get; set; }

		/// <summary>
		/// Public property to indicate the list of extensions that need to be cleaned.
		/// This is a required property, so it cannot be empty.
		/// If it is desired to delete all the files, use "*".
		/// Multiple extensions must be separated by semicolon ";".
		/// </summary>
		[Required]
		public ITaskItem[] Extensions { get; set; }

		public bool PrintOnly { get; set; } = false;

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
			foreach(var directory in Directories)
			{
				if (!Directory.Exists(directory.GetMetadata("FullPath")))
					continue;

				foreach (var extension in Extensions)
				{
					//TODO: preprocess at earlier stage.
					string pattern;
					if (extension.ItemSpec.Contains("."))
						pattern = "*" + extension.ItemSpec;
					else
						pattern = "*." + extension.ItemSpec;

					return DeleteFilesInDirectoryByExtension(directory.GetMetadata("FullPath"), pattern);
				}
			}

			return true;
		}

		#endregion
	}
}