using System;

namespace DNA.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class DisplayNameAttribute : Attribute
	{
		public string DisplayName
		{
			get
			{
				return this._displayName;
			}
		}

		public static string GetDisplayName(Type t)
		{
			object[] attribs = t.GetCustomAttributes(typeof(DisplayNameAttribute), false);
			if (attribs.Length == 0)
			{
				throw new ArgumentException("Class " + t.Name + " Does not have a Display Name");
			}
			DisplayNameAttribute dna = (DisplayNameAttribute)attribs[0];
			return dna.DisplayName;
		}

		public DisplayNameAttribute(string displayName)
		{
			this._displayName = displayName;
		}

		private string _displayName;
	}
}
