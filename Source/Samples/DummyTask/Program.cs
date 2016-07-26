#region Using directives

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#endregion

namespace DummyTaskNameSpace
{
	public class DummyTask : Task
	{
		#region Private members

		private string dummyProperty;

		#endregion

		#region Public properties

		public string DummyProperty
		{
			get { return dummyProperty; }
			set { dummyProperty = value; }
		}

		#endregion

		#region Public methods

		public override bool Execute()
		{
			return true;
		}

		#endregion
	}
}