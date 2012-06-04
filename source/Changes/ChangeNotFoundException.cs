using System;

namespace Changes
{
	public class ChangeNotFoundException : Exception
	{
		public ChangeNotFoundException(string propertyName) : base(String.Format("No changes for property '{0}' found in change set.", propertyName))
		{
		}
	}
}
