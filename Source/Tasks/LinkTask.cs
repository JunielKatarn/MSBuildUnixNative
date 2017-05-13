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

		#endregion // Static members

		#region Private members

		#endregion // Private members

		#region Linker options

		[Required]
		public string LLDExecutable { get; set; }

		[Required]
		public ITaskItem[] InputFiles { get; set; }

		/// <summary>
		/// --hash-style
		/// Specify hash style (sysv, gnu or both)
		/// </summary>
		public string HashStile { get; set; }

		/// <summary>
		/// --eh-frame-hdr
		/// Request creation of .eh_frame_hdr section and PT_GNU_EH_FRAME segment header
		/// </summary>
		public bool EhFrameHeader { get; set; }

		/// <summary>
		/// -m
		/// Target emulation
		/// </summary>
		public string TargetEmulation { get; set; }

		/// <summary>
		/// -dynamic-linker
		/// </summary>
		public string DynamicLinker { get; set; }

		/// <summary>
		/// -z
		/// </summary>
		public string LinkerOptionExtensions { get; set; }

		/// <summary>
		/// -L
		/// </summary>
		public ITaskItem[] LibrarySearchPath { get; set; }

		/// <summary>
		/// -l
		/// Root name of library to use
		/// </summary>
		public ITaskItem[] LibraryNames { get; set; }

		/// <summary>
		/// -o
		/// </summary>
		public ITaskItem OutputFile { get; set; }

		#endregion // Linker options

		#region Task members

		public override bool Execute()
		{
			throw new NotImplementedException();
		}

		#endregion // Task members
	}
}
