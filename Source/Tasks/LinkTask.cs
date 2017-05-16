using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace LLVM.Build.Tasks
{
	public class LinkTask : Task
	{
		#region Static members

		static LinkTask()
		{
			// Init arg priorities.
			uint i = 0;
			argPriorities["LinkerOptionExtensions"]	= i++;
			argPriorities["HashStyle"]				= i++;
			argPriorities["EhFrameHeader"]			= i++;
			argPriorities["TargetEmulation"]		= i++;
			argPriorities["DynamicLinker"]			= i++;
			argPriorities["HeaderInputs"]			= i++;
			argPriorities["LibrarySearchPath"]		= i++;
			argPriorities["InputFiles"]				= i++;
			argPriorities["LibraryNames"]			= i++;
			argPriorities["FooterInputs"]			= i++;
			argPriorities["OutputFile"]				= i++;
		}

		private static IDictionary<string, uint> argPriorities = new SortedDictionary<string, uint>();

		private static ArgComparer GetArgComparer()
		{
			return new ArgComparer();
		}

		#endregion // Static members

		#region Private members

		private class ArgComparer : IComparer<string>
		{
			public int Compare(string x, string y)
			{
				return argPriorities[x].CompareTo(argPriorities[y]);
			}
		}

		private IDictionary<string, object> arguments = new Dictionary<string, object>();
		private IDictionary<string, string> argumentValues = new SortedDictionary<string, string>(GetArgComparer());

		private int InvokeProcess(ICollection<string> args)
		{
			int exitCode = -1;

			Log.LogMessage(LLDExecutable);
			Log.LogMessage("\t" + string.Join("\n\t", args));

			return exitCode;
		}

		#endregion // Private members

		#region Linker options

		[Required]
		public string LLDExecutable { get; set; }

		[Required]
		public ITaskItem[] InputFiles
		{
			get
			{
				return arguments["InputFiles"] as ITaskItem[];
			}

			set
			{
				if (value.Length < 1 || value == null)
					throw new Exception();

				arguments["InputFiles"] = value;
				argumentValues["InputFiles"] = string.Join<ITaskItem>(" ", value);
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
				return arguments["HashStyle"] as string;
			}

			set
			{
				if (string.IsNullOrEmpty(value))
				{
					arguments.Remove("HashStyle");
					argumentValues.Remove("HashStyle");
				}
				else
				{
					arguments["HashStyle"] = value;
					argumentValues["HashStyle"] = $"--hash-style={value}";
				}
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
				if (arguments.ContainsKey("EhFrameHeader"))
				{
					return (bool)arguments["EhFrameHeader"];
				}

				return false;
			}

			set
			{
				arguments["EhFrameHeader"] = value;
				if (value)
					argumentValues["EhFrameHeader"] = "--eh-frame-hdr";
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
				return arguments["TargetEmulation"] as string;
			}

			set
			{
				arguments["TargetEmulation"] = value;
				argumentValues["TargetEmulation"] = $"-m {value}";
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
				return arguments["HeaderInputs"] as ITaskItem[];
			}

			set
			{
				arguments["HeaderInputs"] = value;
				argumentValues["HeaderInputs"] = string.Join<ITaskItem>(" ", value);
			}
		}

		/// <summary>
		/// -dynamic-linker
		/// TODO: EXE-only?
		/// </summary>
		public string DynamicLinker
		{
			get
			{
				return arguments["DynamicLinker"] as string;
			}

			set
			{
				arguments["DynamicLinker"] = value;
				argumentValues["DynamicLinker"] = $"-dynamic-linker {value}";
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
				if (!arguments.ContainsKey("Shared"))
					return false;

				return (bool)arguments["Shared"];
			}

			set
			{
				arguments["Shared"] = value;
				if (value)
					argumentValues["Shared"] = "-shared";
			}
		}

		/// <summary>
		/// -z
		/// </summary>
		public string LinkerOptionExtensions
		{
			get
			{
				return arguments["LinkerOptionExtensions"] as string;
			}

			set
			{
				arguments["LinkerOptionExtensions"] = value;
				argumentValues["LinkerOptionExtensions"] = $"-z {value}";
			}
		}

		/// <summary>
		/// -L
		/// </summary>
		public ITaskItem[] LibrarySearchPath
		{
			get
			{
				return arguments["LibrarySearchPath"] as ITaskItem[];
			}

			set
			{
				arguments["LibrarySearchPath"] = value;
				argumentValues["LibrarySearchPath"] = "-L" +  string.Join<ITaskItem>(" -L", value);
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
				return arguments["LibraryNames"] as string[];
			}

			set
			{
				arguments["LibraryNames"] = value;
				argumentValues["LibraryNames"] = "-l" + string.Join(" -l", value);
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
				return arguments["FooterInputs"] as ITaskItem[];
			}

			set
			{
				arguments["FooterInputs"] = value;
				argumentValues["FooterInputs"] = string.Join<ITaskItem>(" ", value);
			}
		}

		/// <summary>
		/// -o
		/// </summary>
		public ITaskItem OutputFile
		{
			get
			{
				return arguments["OutputFile"] as ITaskItem;
			}

			set
			{
				arguments["OutputFile"] = value;
				argumentValues["OutputFile"] = $"-o {value}";
			}
		}

		#endregion // Linker options

		#region Task members

		public override bool Execute()
		{
			try
			{
				return InvokeProcess(argumentValues.Values) == 0;
			}
			catch
			{
				return false;
			}
		}

		#endregion // Task members
	}
}
