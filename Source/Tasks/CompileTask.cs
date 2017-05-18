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
			argPriorities["InputFiles"] = i++;
			argPriorities["Verbose"]    = i++;
		}
		private static ArgComparer GetArgComparer()
		{
			return new ArgComparer();
		}

		#endregion

		#region Private members

		private class ArgComparer : IComparer<string>
		{
			public int Compare(string x, string y)
			{
				return argPriorities[x].CompareTo(argPriorities[y]);
			}
		}

		private IDictionary<string, object> argValues = new Dictionary<string, object>();

		private IDictionary<string, string> argStrings = new SortedDictionary<string, string>(GetArgComparer());

		private string[] ToArgArray()
		{
			string[] result = new string[argStrings.Count];
			argStrings.Values.CopyTo(result, 0);

			return result;
		}

		//TODO: Move SetArgumentProperty into a base or utility class.
		private void SetArgumentProperty(string key, bool value, string stringValue)
		{
			argValues[key] = value;
			if (value)
				argStrings[key] = stringValue;
		}

		private void SetArgumentProperty(string key, string value, string stringValue)
		{
			if (!string.IsNullOrEmpty(value))
			{
				argValues[key] = value;
				argStrings[key] = stringValue;
			}
			else if (argValues.ContainsKey(key))
			{
				argValues.Remove(key);
				argStrings.Remove(key);
			}
		}

		private void SetArgumentProperty(string key, string[] value, string separator)
		{
			if (value != null && value.Length > 0)
			{
				argValues[key] = value;
				string prefix = string.IsNullOrWhiteSpace(separator) ? "" : separator;
				argStrings[key] = prefix + string.Join(separator, value);
			}
			else if (argValues.ContainsKey(key))
			{
				argValues.Remove(key);
				argStrings.Remove(key);
			}
		}

		private void SetArgumentProperty(string key, ITaskItem value, string stringValue)
		{
			if (value != null)
			{
				argValues[key] = value;
				argStrings[key] = stringValue;
			}
			else if (argValues.ContainsKey(key))
			{
				argValues.Remove(key);
				argStrings.Remove(key);
			}
		}

		private void SetArgumentProperty(string key, ITaskItem[] value, string separator)
		{
			if (value != null && value.Length > 0)
			{
				argValues[key] = value;
				string prefix = string.IsNullOrWhiteSpace(separator) ? "" : separator;
				argStrings[key] = prefix + string.Join<ITaskItem>(separator, value);
			}
			else if (argValues.ContainsKey(key))
			{
				argValues.Remove(key);
				argStrings.Remove(key);
			}
		}

		private string[] ToArgArray(ITaskItem item)
		{
			return new string[]
			{
				Stages[Stage],
				$"-o {IntDir}{item.ItemSpec.Substring(0, item.ItemSpec.LastIndexOf('.'))}.o",
				item.GetMetadata("FullPath")
			};
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

		#endregion

		#region Compiler options

		[Required]
		public string ClangExecutable { get; set; }

		[Required]
		public ITaskItem[] InputFiles
		{
			get
			{
				return argValues["InputFiles"] as ITaskItem[];
			}

			set
			{
				SetArgumentProperty("InputFiles", value, " ");
			}
		}

		public string Stage
		{
			get
			{
				if (!argValues.ContainsKey("Stage"))
					return "Compile";//TODO: Move default into props.

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