using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Microsoft.Build.Framework;

namespace LLVM.Build.Tasks
{
	public class LinkTask : CommandLineTask
	{
		#region static members

		static LinkTask()
		{
			// Init arg priorities.
			uint i = 0;
			argPriorities["LinkerOptionExtensions"]	= i++;
			argPriorities["HashStyle"]				= i++;
			argPriorities["EhFrameHeader"]			= i++;
			argPriorities["TargetEmulation"]		= i++;
			argPriorities["Shared"]					= i++;
			argPriorities["DynamicLinker"]			= i++;
			argPriorities["HeaderInputs"]			= i++;
			argPriorities["LibrarySearchPath"]		= i++;
			argPriorities["InputFiles"]				= i++;
			argPriorities["LibraryNames"]			= i++;
			argPriorities["FooterInputs"]			= i++;
			argPriorities["OutputFile"]				= i++;
		}

		private static IDictionary<string, uint> argPriorities = new SortedDictionary<string, uint>();

		#endregion // static members

		public LinkTask()
		{
			argValues = new Dictionary<string, object>();
			argFuncs = new SortedDictionary<string, Func<object, string>>(Comparer<string>.Create((a, b) => { return argPriorities[a].CompareTo(argPriorities[b]); }));
			argCount = 0;
		}

		#region private members

		private string[] ToArgArray()
		{
			string[] result = new string[argCount];
			uint index = 0;
			foreach(var entry in argFuncs)
			{
				if (argValues[entry.Key].GetType().IsArray)
				{
					foreach(var item in (Array)argValues[entry.Key])
					{
						result[index++] = entry.Value(item);
					}
				}
				else
				{
					result[index++] = entry.Value(argValues[entry.Key]);
				}
			}

			return result;
		}

		private int InvokeProcess(string[] args)
		{
			int exitCode = -1;
			Process process = null;
			string toolPath = GenerateFullPathToTool();

			Log.LogMessage("Executing command:");
			Log.LogCommandLine($"{toolPath}\n\t{string.Join("\n\t", args)}\n");

			if (PrintOnly)
			{
				Log.LogWarning("PrintOnly was set. Ignoring this task.");
				return 0;
			}

			try
			{
				process = new Process();
				process.StartInfo.FileName = toolPath;
				process.StartInfo.Arguments = string.Join(" ", args);
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

				// If the path to OutputFile does not exist, lld won't create it by itself.
				if (!string.IsNullOrEmpty(OutDir) && !Directory.Exists(OutDir))
					Directory.CreateDirectory(OutDir);

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

		public string OutDir { get; set; }

		public bool PrintOnly { get; set; } = false;

		#endregion

		#region Command line arguments

		[Required]
		public ITaskItem[] InputFiles
		{
			get
			{
				return argValues["InputFiles"] as ITaskItem[];
			}

			set
			{
				SetArgumentProperty("InputFiles", value);
			}
		}

		/// <summary>
		/// --hash-style
		/// Specify hash style (sysv, gnu or both)
		/// </summary>
		public string HashStyle
		{
			get
			{
				return argValues["HashStyle"] as string;
			}

			set
			{
				SetArgumentProperty("HashStyle", value, $"--hash-style={value}");
			}
		}

		/// <summary>
		/// --eh-frame-hdr
		/// Request creation of .eh_frame_hdr section and PT_GNU_EH_FRAME segment header
		/// </summary>
		public bool EhFrameHeader
		{
			get
			{
				if (argValues.ContainsKey("EhFrameHeader"))
				{
					return (bool)argValues["EhFrameHeader"];
				}

				return false;
			}

			set
			{
				SetArgumentProperty("EhFrameHeader", value, "--eh-frame-hdr");
			}
		}

		/// <summary>
		/// -m
		/// Target emulation
		/// </summary>
		public string TargetEmulation
		{
			get
			{
				return argValues["TargetEmulation"] as string;
			}

			set
			{
				SetArgumentProperty("TargetEmulation", value, $"-m {value}");
			}
		}

		/// <summary>
		/// Strictly optional.
		/// Provided by property sheets.
		/// </summary>
		public ITaskItem[] HeaderInputs
		{
			get
			{
				return argValues["HeaderInputs"] as ITaskItem[];
			}

			set
			{
				SetArgumentProperty("HeaderInputs", value);
			}
		}

		/// <summary>
		/// -shared
		/// Build a shared object.
		/// TODO: Is this mutuially exclusive with DynamicLinker?
		/// TODO: .so only?
		/// </summary>
		public bool Shared
		{
			get
			{
				if (!argValues.ContainsKey("Shared"))
					return false;

				return (bool)argValues["Shared"];
			}

			set
			{
				SetArgumentProperty("Shared", value, "-shared");
			}
		}

		/// <summary>
		/// -dynamic-linker
		/// TODO: EXE-only?
		/// </summary>
		public ITaskItem DynamicLinker
		{
			get
			{
				return argValues["DynamicLinker"] as ITaskItem;
			}

			set
			{
				SetArgumentProperty("DynamicLinker", value, $"-dynamic-linker {value}");
			}
		}

		/// <summary>
		/// -z
		/// </summary>
		public string LinkerOptionExtensions
		{
			get
			{
				return argValues["LinkerOptionExtensions"] as string;
			}

			set
			{
				SetArgumentProperty("LinkerOptionExtensions", value, $"-z {value}");
			}
		}

		/// <summary>
		/// -L
		/// </summary>
		public ITaskItem[] LibrarySearchPath
		{
			get
			{
				return argValues["LibrarySearchPath"] as ITaskItem[];
			}

			set
			{
				SetArgumentProperty("LibrarySearchPath", value, "-L");
			}
		}

		/// <summary>
		/// -l
		/// Root name of library to use
		/// </summary>
		public string[] LibraryNames
		{
			get
			{
				return argValues["LibraryNames"] as string[];
			}

			set
			{
				SetArgumentProperty("LibraryNames", value, "-l");
			}
		}

		/// <summary>
		/// Strictly optional.
		/// Provided by property sheets.
		/// </summary>
		public ITaskItem[] FooterInputs
		{
			get
			{
				return argValues["FooterInputs"] as ITaskItem[];
			}

			set
			{
				SetArgumentProperty("FooterInputs", value);
			}
		}

		/// <summary>
		/// -o
		/// </summary>
		public ITaskItem OutputFile
		{
			get
			{
				return argValues["OutputFile"] as ITaskItem;
			}

			set
			{
				SetArgumentProperty("OutputFile", value, $"-o {value}");
			}
		}

		#endregion // Command line arguments

		#region Task members

		public override bool Execute()
		{
			try
			{
				return InvokeProcess(ToArgArray()) == 0;
			}
			catch(Exception e)
			{
				Log.LogError(e.StackTrace);

				return false;
			}
		}

		#endregion // Task members

		#region ToolTask members

		protected override string ToolName => "ld.lld";

		#endregion
	}
}
