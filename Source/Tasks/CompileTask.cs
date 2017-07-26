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

		/// <summary>
		/// Mappings from property sheet compiling stage names to actual command line flags.
		/// </summary>
		private static readonly Dictionary<string, string> Stages = new Dictionary<string, string>
		{
			["Preprocess"] = "-E",
			["Parse"]      = "-fsyntax-only",
			["Assemble"]   = "-S",
			["Compile"]    = "-c"
		};

		/// <summary>
		/// Argument-priority map.
		/// </summary>
		private static IDictionary<string, uint> argPriorities = new SortedDictionary<string, uint>();

		/// <summary>
		/// Initializes static values for this class.
		/// </summary>
		static CompileTask()
		{
			uint i = 0;
			argPriorities["Stage"]                   = i++;
			argPriorities["SystemRoot"]              = i++;
			argPriorities["Language"]                = i++;
			argPriorities["StandardLibrary"]         = i++;

			// Compilation flags
			argPriorities["LanguageStandard"]        = i++;

			argPriorities["PositionIndependentCode"] = i++;
			argPriorities["ObjectFileName"]          = i++;
			argPriorities["Verbose"]                 = i++;
		}

		#endregion // static members

		/// <summary>
		/// Initializes an instance of this class.
		/// </summary>
		public CompileTask()
		{
			argValues = new Dictionary<string, object>();
			argFuncs = new SortedDictionary<string, Func<object, string>>(Comparer<string>.Create((a, b) => { return argPriorities[a].CompareTo(argPriorities[b]); }));
			argCount = 0;
		}

		#region private members

		/// <summary>
		/// Serializes this task's argument values.
		/// </summary>
		/// <param name="item">Item to compile</param>
		/// <returns>String array with the ordered command line arguments</returns>
		private string[] ToArgArray(ITaskItem item)
		{
			string[] itemArgs = new string[]
			{
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

		/// <summary>
		/// Executes the underlying compiler tool.
		/// </summary>
		/// <param name="item">Compilation unit</param>
		/// <returns>OS-provided process exit status.</returns>
		private int InvokeProcess(ITaskItem item)
		{
			int exitCode = -1;
			Process process = null;
			string[] arguments = ToArgArray(item);
			string toolPath = GenerateFullPathToTool();

			Log.LogMessage("Executing command:");
			Log.LogCommandLine($"{toolPath}\n\t{string.Join("\n\t", arguments)}\n");

			if (Fake)
			{
				Log.LogWarning("Fake was set. Ignoring this task.");
				return 0;
			}

			try
			{
				process = new Process();
				process.StartInfo.FileName = toolPath;
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

		/// <summary>
		/// Intermediate output directory.
		/// </summary>
		public string IntDir { get; set; }

		[Output]
		public string[] ObjectFiles { get; set; }

		#endregion // Properties

		#region Command line arguments

		/// <summary>
		/// Not treated as a regular command line argument.
		/// There will be a clang process for each source file in this array.
		/// </summary>
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

		public string ObjectFileName
		{
			get
			{
				return argValues["ObjectFileName"] as string;
			}

			set
			{
				SetArgumentProperty("ObjectFileName", value, $"-o {value}");
			}
		}

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
		/// <summary>
		/// Common values:TODO
		/// </summary>
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
		/// Common values:TODO
		/// </summary>
		public string StandardLibrary
		{
			get
			{
				return argValues["StandardLibrary"] as string;
			}

			set
			{
				SetArgumentProperty("StandardLibrary", value, $"-stdlib={value}");
			}
		}

		/// <summary>
		/// Common values:TODO
		/// </summary>
		public string LanguageStandard
		{
			get
			{
				return argValues["LanguageStandard"] as string;
			}

			set
			{
				SetArgumentProperty("LanguageStandard", value, $"-std={value}");
			}
		}

		public bool PositionIndependentCode
		{
			get
			{
				if (argValues.ContainsKey("PositionIndependentCode"))
					return (bool)argValues["PositionIndependentCode"];

				return false;
			}

			set
			{
				SetArgumentProperty("PositionIndependentCode", value, "-fPIC");
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

		#endregion // Command line arguments

		#region Task members

		/// <summary>
		/// Executes this task.
		/// </summary>
		/// <returns>
		/// True - if the task succeeded and exited without errors. False otherwise.
		/// </returns>
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

		#region ToolTask members

		protected override string ToolName => "clang";

		#endregion
	}
}