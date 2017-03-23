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
	public class WhatIfCompileTask : Task
	{
		#region Static members

		static readonly Dictionary<string, string> Stages = new Dictionary<string, string>
		{
			["Preprocess"]	= "-E",
			["Parse"]		= "-fsyntax-only",
			["Assemble"]	= "-S",
			["Compile"]		= "-c"
		};

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
			foreach(var item in InputFiles)
			{
				Console.WriteLine(RenderCommand(item));
			}

			//InvokeProcess(null);

			return true;
		}

		#endregion

		#region Private members

		private int InvokeProcess(ITaskItem item)
		{
			int exitCode = -1;
			Process process = null;

			try
			{
				process = new Process();
				process.StartInfo.FileName = ClangExecutable;
				process.StartInfo.Arguments = "--version";
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

				// If the path to _option_o does not exist, clang won't create it by itself.
				string dir = Path.GetDirectoryName(item.ToString());
				if (!string.IsNullOrEmpty(dir))
					Directory.CreateDirectory(dir);

				//Console.WriteLine("Command to execute: " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);

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
			catch(Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			finally
			{
				process?.Dispose();
			}

			return exitCode;
		}

		private string RenderCommand(ITaskItem item)
		{
			string name = item.ToString();

			return $"{ClangExecutable}\n" +
					$"\t{Stages[Stage]}\n" +
					$"\t-o {IntDir}{name.Substring(0, name.LastIndexOf('.'))}.o\n" +
					$"\t{item}\n";
		}

		#endregion
	}
}
