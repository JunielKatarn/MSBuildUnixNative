#region Using directives

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Diagnostics;
using System.Threading;

using System.IO;

#endregion

namespace LLVM.Build.Tasks
{
	/// <summary>
	/// This is an MSBuild Task that allows executing the current operating
	/// system's version of clang for the compilation process.
	/// </summary>
	public class CompileTask : Task
	{
		#region Private members

		private string _clangExecutable = String.Empty;
		private string _inputFiles = String.Empty; 
		private string _option_o = String.Empty;
		
		#endregion

		#region Public properties

		/// <summary>
		/// Full path to the clang executable.
		/// </summary>
		[Required]
		public string ClangExecutable
		{
			get { return _clangExecutable; }
			set { _clangExecutable = value; }
		}

		/// <summary>
		/// The C++ files to compile.
		/// </summary>
		[Required]
		public string InputFiles
		{
			get { return _inputFiles; }
			set { _inputFiles = value; }
		}

		#region  Clang options and arguments

		/// <summary>
		/// -o
		/// Writes output to file.
		/// </summary>
		[Required]
		public string Option_o
		{
			get { return _option_o; }
			set { _option_o = value; }
		}

		#endregion

		#endregion

		#region Private properties

		/// <summary>
		/// Constructs a string with all the arguments for the clang executable
		/// using the provided values in the public properties.
		/// </summary>
		private string Arguments
		{
			get {
				return String.Format(
					"{0} -o {1} ",
					InputFiles,
					Option_o
				);
			}
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Executes clang.
		/// </summary>
		/// <returns>True if the return value of clang was 0.</returns>
		public override bool Execute()
		{
			return RunProgram() == 0;
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Creates a Process for the clang executable with all the necessary arguments,
		/// and captures the standard output and error strings to log them.
		/// </summary>
		/// <returns>The integer return value of the process.</returns>
		private int RunProgram()
		{
			int exitCode = -1;

			Process process = null;

			try
			{
				process = new Process();

				process.StartInfo.FileName = ClangExecutable;
				process.StartInfo.Arguments = Arguments;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;

				process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => {
					if (!String.IsNullOrEmpty(e.Data))
					{
						Log.LogMessage(e.Data);
					}
				});

				process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => {
					if (!String.IsNullOrEmpty(e.Data))
					{
						Log.LogError(e.Data);
					}
				});

				//TODO: Create ProcessOptions method.
				// If the path to _option_o does not exist, clang won't create it by itself.
				string dir = Path.GetDirectoryName(_option_o);
				if (! string.IsNullOrEmpty(dir))
				{
					Directory.CreateDirectory(dir);
				}

				Console.WriteLine("Command to execute: " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);

				process.Start();

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				while (!process.HasExited)
				{
					process.WaitForExit(1);
					Thread.Sleep(TimeSpan.FromSeconds(1));
				}

				exitCode = process.ExitCode;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			finally
			{
				if (process != null)
				{
					process.Dispose();
				}
			}

			return exitCode;
		}

		#endregion
	}
}