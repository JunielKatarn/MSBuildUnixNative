using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace LLVM.Build.Tasks
{
	class ArchiveTask : CommandLineTask
	{
		#region Static members

		#endregion // Static members

		#region Private members

		#endregion // Private members

		#region Properties

		#endregion // Properties

		#region Archiver options

		#endregion // Linker options

		#region Task members

		public override bool Execute()
		{
			throw new NotImplementedException();
		}

		#endregion // Task members

		#region ToolTask members

		protected override string ToolName => "llvm-ar";

		#endregion // ToolTask members
	}
}
