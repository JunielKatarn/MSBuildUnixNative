#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Microsoft.Build.Framework;

#endregion

namespace LLVM.Build.Tasks
{
	/// <summary>
	/// This is an MSBuild Task that allows executing the clang compiler, up to the 'Compile' stage.
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
			argPriorities["SystemRoot"] = i++;
			argPriorities["Language"]   = i++;
			argPriorities["Verbose"]    = i++;
			//argPriorities["InputFiles"] = i++;
		}

		#endregion // static members

		public CompileTask()
		{
			argValues = new Dictionary<string, object>();
			argFuncs = new SortedDictionary<string, Func<object, string>>(Comparer<string>.Create((a, b) => { return argPriorities[a].CompareTo(argPriorities[b]); }));
			argCount = 0;

			// Defaults
			//TODO: Move into props?
			Stage = "Compile";
			Language = "c++";
		}

		#region private members

		private string[] ToArgArray(ITaskItem item)
		{
			string[] itemArgs = new string[]
			{
				$"-o {IntDir}{item.ItemSpec.Substring(0, item.ItemSpec.LastIndexOf('.'))}.o",
				item.GetMetadata("FullPath")
			};

			string[] result = new string[argCount + itemArgs.Length];
			int index = 0;
			foreach (var entry in argFuncs)
			{
				if (argValues[entry.Key].GetType().IsArray)
				{
					foreach (var value in (Array)argValues[entry.Key])
					{
						result[index++] = entry.Value(value);
					}
				}
				else
				{
					result[index++] = entry.Value(argValues[entry.Key]);
				}
			}
			itemArgs.CopyTo(result, index);

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

		#region Properties

		[Output]
		public string[] ObjectFiles
		{
			get
			{
				string[] result = new string[InputFiles.Length];

				for (int i = 0; i < result.Length; i++)
				{
					result[i] = $"{IntDir}{InputFiles[i].ItemSpec.Substring(0, InputFiles[i].ItemSpec.LastIndexOf('.'))}.o";
				}

				return result;
			}
		}

		#endregion // Properties

		#region Command line arguments

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

		public string SystemRoot
		{
			get
			{
				return argValues["SystemRoot"] as string;
			}

			set
			{
				SetArgumentProperty("SystemRoot", value, $"-isysroot {value}");
			}
		}

		//TODO: Make non-arg? If not provided, infer/set per taskitem (InputFiles).
		public string Language
		{
			get
			{
				return argValues["Language"] as string;
			}

			set
			{
				SetArgumentProperty("Language", value, $"-x {value}");
			}
		}

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

		#endregion // Command line arguments

		#region Task members

		public override bool Execute()
		{
			bool result = true;

			foreach (var item in InputFiles)
			{
				result &= InvokeProcess(item) == 0;
			}

			return result;
		}

		#endregion // Task members
	}
}