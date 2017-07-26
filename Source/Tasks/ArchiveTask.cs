using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace LLVM.Build.Tasks
{
	public class ArchiveTask : CommandLineTask
	{
		#region Static members

		private static IDictionary<string, int> argPriorities = new SortedDictionary<string, int>();

		static ArchiveTask()
		{
			// Init arg priorities
			argPriorities["OutputFile"] = argPriorities.Count;
			argPriorities["InputFiles"] = argPriorities.Count;
		}

		#endregion // Static members

		public ArchiveTask()
		{
			argValues = new Dictionary<string, object>();
			argFuncs = new SortedDictionary<string, Func<object, string>>(Comparer<string>.Create((a, b) => { return argPriorities[a].CompareTo(argPriorities[b]); }));
			argCount = 0;
		}

		#region Private members

		#endregion // Private members

		#region Properties

		#endregion // Properties

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
		/// 
		/// </summary>
		public ITaskItem OutputFile
		{
			get
			{
				return argValues["OutputFile"] as ITaskItem;
			}

			set
			{
				SetArgumentProperty("OutputFile", value, value.ToString());
			}
		}

		#endregion // Command line arguments

		#region Task members

		#endregion // Task members

		#region ToolTask members

		protected override string ToolName => "llvm-ar";

		protected override string GenerateCommandLineCommands()
		{
			CommandLineBuilder builder = new CommandLineBuilder();

			builder.AppendSwitch("rc"); // modifiers
			builder.AppendFileNameIfNotNull(OutputFile);
			builder.AppendFileNamesIfNotNull(InputFiles, " ");

			//TODO: Move elsewhere?
			// Ensure OutputFile's directory exists.
			if (!Directory.Exists(Path.GetDirectoryName(OutputFile.GetMetadata("FullPath"))))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(OutputFile.GetMetadata("FullPath")));
			}

			return builder.ToString();
		}

		#endregion // ToolTask members
	}
}
