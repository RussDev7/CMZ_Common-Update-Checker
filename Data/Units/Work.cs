using System;

namespace DNA.Data.Units
{
	[Serializable]
	public struct Work
	{
		public float Watts
		{
			get
			{
				return this._watts;
			}
			set
			{
				this._watts = value;
			}
		}

		public override int GetHashCode()
		{
			return this._watts.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Work b = (Work)obj;
			return this._watts == b._watts;
		}

		public static bool operator ==(Work a, Work b)
		{
			return a._watts == b._watts;
		}

		public static bool operator !=(Work a, Work b)
		{
			return a._watts != b._watts;
		}

		private float _watts;
	}
}
