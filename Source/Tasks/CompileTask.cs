#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#endregion

namespace LLVM.Build.Tasks
{
	/// <summary>
	/// This is an MSBuild Task that allows executing the current operating
	/// system's version of clang for the compilation process.
	/// </summary>
	public class CompileTask : Task
	{
		#region Static members

		static readonly Dictionary<string, string> Stages = new Dictionary<string, string>
		{
			["Preprocess"] = "-E",
			["Parse"] = "-fsyntax-only",
			["Assemble"] = "-S",
			["Compile"] = "-c"
		};

		#endregion

		#region Private members

		private int InvokeProcess(ITaskItem item)
		{
			int exitCode = -1;
			Process process = null;
			string[] arguments = GetArguments(item);

			Log.LogMessage("Executing command:");
			Log.LogCommandLine($"{ClangExecutable}\n\t{string.Join("\n\t", arguments)}");

			try
			{
				process = new Process();
				process.StartInfo.FileName = ClangExecutable;
				process.StartInfo.Arguments = string.Join(" ", arguments);
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;

				process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
						Log.LogMessage(e.Data);
				});

				process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
						Log.LogError(e.Data);
				});

				// If the path to ObjectFile does not exist, clang won't create it by itself.
				if (!string.IsNullOrEmpty(IntDir) && !Directory.Exists(IntDir))
					Directory.CreateDirectory(IntDir);

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
				process?.Dispose();
			}

			return exitCode;
		}

		private string[] GetArguments(ITaskItem item)
		{
			return new string[]
			{
				Stages[Stage],
				$"-o {IntDir}{item.ItemSpec.Substring(0, item.ItemSpec.LastIndexOf('.'))}.o",
				item.GetMetadata("FullPath")
			};
		}

		#endregion

		#region Compiler options

		[Required]
		public string ClangExecutable { get; set; }

		[Required]
		public ITaskItem[] InputFiles { get; set; }

		public string Stage { get; set; } = "Compile";

		public string IntDir { get; set; }

		public string ObjectFileName { get; set; }

		#endregion

		#region Task members

		public override bool Execute()
		{
			foreach (var item in InputFiles)
			{
				InvokeProcess(item);
			}

			return true;
		}

		#endregion
	}
}