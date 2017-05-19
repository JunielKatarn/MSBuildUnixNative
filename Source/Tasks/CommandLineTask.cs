using System;
using System.Collections.Generic;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace LLVM.Build.Tasks
{
	public abstract class CommandLineTask : Task
	{
		#region static members

		public static ArgComparer GetArgComparer(IDictionary<string, uint> priorities)
		{
			return new ArgComparer(priorities);
		}

		#endregion

		#region private members

		public class ArgComparer : IComparer<string>
		{
			private IDictionary<string, uint> argPriorities;

			public ArgComparer(IDictionary<string, uint> priorities)
			{
				argPriorities = priorities;
			}

			public int Compare(string x, string y)
			{
				return argPriorities[x].CompareTo(argPriorities[y]);
			}
		}

		#endregion

		#region protected members

		protected IDictionary<string, object> argValues;

		protected IDictionary<string, string> argStrings;

		protected void SetArgumentProperty(string key, bool value, string stringValue)
		{
			argValues[key] = value;
			if (value)
				argStrings[key] = stringValue;
		}

		protected void SetArgumentProperty(string key, string value, string stringValue)
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

		protected void SetArgumentProperty(string key, string[] value, string separator)
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

		protected void SetArgumentProperty(string key, ITaskItem value, string stringValue)
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

		protected void SetArgumentProperty(string key, ITaskItem[] value, string separator)
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

		#endregion // protected members
	}
}
