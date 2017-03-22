#region Using directives

using System;
using System.Collections.Generic;

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

		#region Private members

		private string RenderCommand(ITaskItem item)
		{
			string name = item.ToString();

			return $"{ClangExecutable}\n" +
					$"\t{Stages[Stage]}\n" +
					$"\t-o {IntDir}{name.Substring(0, name.LastIndexOf('.'))}.o\n" +
					$"\t{item}\n";
		}

		#endregion

		#region Compiler options

		[Required]
		public string ClangExecutable { get; set; }

		[Required]
		public ITaskItem[] InputFiles { get; set; }

		public string Stage { get; set; } = "Compile";

		public string IntDir { get; set; }

		#endregion

		#region Task members

		public override bool Execute()
		{
			foreach(var item in InputFiles)
			{
				Log.LogCommandLine(RenderCommand(item));
			}

			return true;
		}

		#endregion
	}
}
