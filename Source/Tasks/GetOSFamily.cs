using System.Runtime.InteropServices;

using Microsoft.Build.Utilities;

namespace LLVM.Build.Tasks
{
	public class GetOSFamily : Task
	{

		public override bool Execute()
		{
			OSPlatform p;
			if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				p = OSPlatform.Windows;
			else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				p = OSPlatform.Linux;
			else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				p = OSPlatform.OSX;

			Log.LogWarning(p.ToString());
			Log.LogWarning($"W: {OSPlatform.Windows.ToString()}, L: {OSPlatform.Linux.ToString()}, M: {OSPlatform.OSX.ToString()}");

			return true;
		}
	}
}
