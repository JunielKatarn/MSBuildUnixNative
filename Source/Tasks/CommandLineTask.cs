using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace LLVM.Build.Tasks
{
	/// <summary>
	/// Base class modeling a command line tool with variable arguments.
	/// </summary>
	public abstract class CommandLineTask : ToolTask
	{
		#region protected members

		/// <summary>
		/// Actual values for the command line arguments.
		/// Each value is identified by a task property name.
		/// </summary>
		protected IDictionary<string, object> argValues;

		/// <summary>
		/// Post-processing functions for stored argument values.
		/// Meant to operate on this instance's arguments before they get serialized.
		/// </summary>
		protected IDictionary<string, Func<object, string>> argFuncs;

		/// <summary>
		/// Member counting the set argument values.
		/// </summary>
		protected int argCount;

		/// <summary>
		/// Sets a boolean/switch argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		/// <param name="valueFunc">Associated formatting function</param>
		protected void SetArgumentProperty(string key, bool value, Func<object, string> valueFunc)
		{
			argValues[key] = value;

			// Persist the value only if the switch is present (true).
			if (value)
			{
				argFuncs[key] = valueFunc;
				argCount++;
			}
		}

		/// <summary>
		/// Sets a boolean/switch argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		/// <param name="stringValue">Associated constant literal value</param>
		protected void SetArgumentProperty(string key, bool value, string stringValue)
		{
			SetArgumentProperty(key, value, (v) => { return stringValue; });
		}

		/// <summary>
		/// Sets a string argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		/// <param name="valueFunc">Associated formatting function</param>
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

		/// <summary>
		/// Sets a string argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		/// <param name="stringValue">Associated constant literal value</param>
		protected void SetArgumentProperty(string key, string value, string stringValue)
		{
			SetArgumentProperty(key, value, (v) => { return stringValue; });
		}

		/// <summary>
		/// Sets a string array argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		/// <param name="valueFunc">
		/// Associated formatting function for each value element.
		/// </param>
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

		/// <summary>
		/// Sets a string array argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		/// <param name="separator">Separator string to be joined between the value elements.</param>
		protected void SetArgumentProperty(string key, string[] value, string separator)
		{
			SetArgumentProperty(key, value, (v) => { return $"{separator}{v}"; });
		}

		/// <summary>
		/// Sets a string array argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		protected void SetArgumentProperty(string key, string[] value)
		{
			SetArgumentProperty(key, value, (v) => { return v.ToString(); });
		}

		/// <summary>
		/// Sets a task item argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		/// <param name="valueFunc">Asssociated formatting function</param>
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

		/// <summary>
		/// Sets a task item argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		/// <param name="stringValue">Associated constant literal value</param>
		protected void SetArgumentProperty(string key, ITaskItem value, string stringValue)
		{
			SetArgumentProperty(key, value, (v) => { return stringValue; });
		}

		/// <summary>
		/// Sets a task item array argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		/// <param name="valueFunc">Associated formatting function for each item</param>
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

		/// <summary>
		/// Sets a task item array argument property.
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		/// <param name="separator">String to be joined between the value elements</param>
		protected void SetArgumentProperty(string key, ITaskItem[] value, string separator)
		{
			SetArgumentProperty(key, value, (v) => { return $"{separator}{v}"; });
		}

		/// <summary>
		/// Sets a task item array argument property
		/// </summary>
		/// <param name="key">Argument name</param>
		/// <param name="value">Argument value</param>
		protected void SetArgumentProperty(string key, ITaskItem[] value)
		{
			SetArgumentProperty(key, value, (v) => { return v.ToString(); });
		}

		#endregion // protected members

		#region public members

		[Required]
		public string ToolDir { get; set; }

		protected override string GenerateFullPathToTool()
		{
			return Path.Combine(ToolDir, ToolName);
		}

		#endregion // public members
	}
}
