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
	public class CompileTask : CommandLineTask
	{
		#region static members

		private static readonly Dictionary<string, string> Stages = new Dictionary<string, string>
		{
			["Preprocess"] = "-E",
			["Parse"]      = "-fsyntax-only",
			["Assemble"]   = "-S",
			["Compile"]    = "-c"
		};

		private static IDictionary<string, uint> argPriorities = new SortedDictionary<string, uint>();

		static CompileTask()
		{
			uint i = 0;
			argPriorities["Stage"]      = i++;
			argPriorities["Verbose"]    = i++;
		}

		#endregion // static members

		public CompileTask()
		{
			argValues = new Dictionary<string, object>();
			argStrings = new SortedDictionary<string, string>(CommandLineTask.GetArgComparer(argPriorities));

			// Defaults
			Stage = "Compile";//TODO: Move default into props.
		}

		#region private members

		private string[] ToArgArray(ITaskItem item)
		{
			string[] itemArgs = new string[]
			{
				$"-o {IntDir}{item.ItemSpec.Substring(0, item.ItemSpec.LastIndexOf('.'))}.o",
				item.GetMetadata("FullPath")
			};

			string[] result = new string[argStrings.Count + itemArgs.Length];
			argStrings.Values.CopyTo(result, 0);
			itemArgs.CopyTo(result, argStrings.Count);

			return result;
		}

		private int InvokeProcess(ITaskItem item)
		{
			int exitCode = -1;
			Process process = null;
			string[] arguments = ToArgArray(item);

			Log.LogMessage("Executing command:");
			Log.LogCommandLine($"{ClangExecutable}\n\t{string.Join("\n\t", arguments)}\n");

			if (PrintOnly)
			{
				Log.LogWarning("PrintOnly was set. Ignoring this task.");
				return 0;
			}

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

		#endregion // private members

		#region Compiler options

		[Required]
		public string ClangExecutable { get; set; }

		[Required]
		public ITaskItem[] InputFiles { get; set; }

		public string Stage
		{
			get
			{
				return argValues["Stage"] as string;
			}

			set
			{
				SetArgumentProperty("Stage", value, Stages[value]);
			}
		}

		public string IntDir { get; set; }

		/// <summary>
		/// Only applies when the length of InputFiles is 1.
		/// TODO: Either find usage, or deprecate.
		/// </summary>
		public string ObjectFileName { get; set; }

		/// <summary>
		/// -v
		/// </summary>
		public bool Verbose
		{
			get
			{
				if (argValues.ContainsKey("Verbose"))
					return (bool)argValues["Verbose"];

				return false;
			}

			set
			{
				SetArgumentProperty("Verbose", value, "-v");
			}
		}

		public bool PrintOnly { get; set; } = false;

		#endregion // Compiler options

		#region Task members

		public override bool Execute()
		{
			foreach (var item in InputFiles)
			{
				InvokeProcess(item);
			}

			return true;
		}

		#endregion // Task members
	}
}