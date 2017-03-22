#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#endregion

namespace LLVM.Build.Tasks
{
	public class WhatIfCompileTask : Task
	{
		#region Private members

		private void LogHigh(string message)
		{
			Log.LogCommandLine(MessageImportance.High, message);
		}

		#endregion

		#region Public properties

		[Required]
		public string ClangExecutable { get; set; }

		public string InputFiles { get; set; }

		public string ObjectFileName
		{
			get; set;
		}


		#endregion

		#region Task members

		public override bool Execute()
		{
			LogHigh($"{ClangExecutable}");
			LogHigh($"\t-o {ObjectFileName}.obj");
			LogHigh($"\t{InputFiles}");

			return true;
		}

		#endregion
	}
}
