using System;
using System.Collections.Generic;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace LLVM.Build.Tasks
{
	public abstract class CommandLineTask : Task
	{
		#region protected members

		protected IDictionary<string, object> argValues;

		protected IDictionary<string, Func<object, string>> argFuncs;

		protected int argCount;

		protected void SetArgumentProperty(string key, bool value, Func<object, string> valueFunc)
		{
			argValues[key] = value;
			if (value)
			{
				argFuncs[key] = valueFunc;
				argCount++;
			}
		}

		protected void SetArgumentProperty(string key, bool value, string stringValue)
		{
			SetArgumentProperty(key, value, (v) => { return stringValue; });
		}

		protected void SetArgumentProperty(string key, string value, Func<object, string> valueFunc)
		{
			if (!string.IsNullOrEmpty(value))
			{
				argValues[key] = value;
				argFuncs[key] = valueFunc;

				argCount++;
			}
			else if (argValues.ContainsKey(key))
			{
				argValues.Remove(key);
				argFuncs.Remove(key);

				argCount--;
			}
		}

		protected void SetArgumentProperty(string key, string value, string stringValue)
		{
			SetArgumentProperty(key, value, (v) => { return stringValue; });
		}

		protected void SetArgumentProperty(string key, string[] value, Func<object, string> valueFunc)
		{
			if (value != null && value.Length > 0)
			{
				argValues[key] = value;
				argFuncs[key] = valueFunc;

				argCount += value.Length;
			}
			else if (argValues.ContainsKey(key))
			{
				argCount -= (argValues[key] as string[]).Length;

				argValues.Remove(key);
				argFuncs.Remove(key);
			}
		}

		protected void SetArgumentProperty(string key, string[] value, string separator)
		{
			SetArgumentProperty(key, value, (v) => { return $"{separator}{v}"; });
		}

		protected void SetArgumentProperty(string key, string[] value)
		{
			SetArgumentProperty(key, value, (v) => { return v.ToString(); });
		}

		protected void SetArgumentProperty(string key, ITaskItem value, Func<object, string> valueFunc)
		{
			if (value != null)
			{
				argValues[key] = value;
				argFuncs[key] = valueFunc;

				argCount++;
			}
			else if (argValues.ContainsKey(key))
			{
				argValues.Remove(key);
				argFuncs.Remove(key);

				argCount--;
			}
		}

		protected void SetArgumentProperty(string key, ITaskItem value, string stringValue)
		{
			SetArgumentProperty(key, value, (v) => { return stringValue; });
		}

		protected void SetArgumentProperty(string key, ITaskItem[] value, Func<object, string> valueFunc)
		{
			if (value != null && value.Length > 0)
			{
				argValues[key] = value;
				argFuncs[key] = valueFunc;

				argCount += value.Length;
			}
			else if (argValues.ContainsKey(key))
			{
				argCount -= (argValues[key] as ITaskItem[]).Length;

				argValues.Remove(key);
				argFuncs.Remove(key);
			}
		}

		protected void SetArgumentProperty(string key, ITaskItem[] value, string separator)
		{
			SetArgumentProperty(key, value, (v) => { return $"{separator}{v}"; });
		}

		protected void SetArgumentProperty(string key, ITaskItem[] value)
		{
			SetArgumentProperty(key, value, (v) => { return v.ToString(); });
		}

		#endregion // protected members
	}
}
