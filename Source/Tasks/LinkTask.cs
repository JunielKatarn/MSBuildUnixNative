﻿using System;
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

		private IDictionary<string, object> argValues = new Dictionary<string, object>();

		private IDictionary<string, string> argStrings = new SortedDictionary<string, string>(GetArgComparer());

		private string[] ToArgArray()
		{
			string[] result = new string[argStrings.Count];
			argStrings.Values.CopyTo(result, 0);

			return result;
		}

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

		private int InvokeProcess(string[] args)
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
				return argValues["InputFiles"] as ITaskItem[];
			}

			set
			{
				SetArgumentProperty("InputFiles", value, " ");
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
				SetArgumentProperty("HeaderInputs", value, " ");
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
				SetArgumentProperty("LibrarySearchPath", value, " -L");
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
				SetArgumentProperty("LibraryNames", value, " l");
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
				SetArgumentProperty("FooterInputs", value, " ");
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

		#endregion // Linker options

		#region Task members

		public override bool Execute()
		{
			try
			{
				return InvokeProcess(ToArgArray()) == 0;
			}
			catch
			{
				return false;
			}
		}

		#endregion // Task members
	}
}
