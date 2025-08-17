﻿using System;

namespace DNA.Data.Units
{
	[Serializable]
	public struct Acceleration
	{
		private Acceleration(float mps2)
		{
			this._mps2 = mps2;
		}

		public override int GetHashCode()
		{
			return this._mps2.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return this._mps2 == ((Acceleration)obj)._mps2;
		}

		public static bool operator ==(Acceleration a, Acceleration b)
		{
			return a._mps2 == b._mps2;
		}

		public static bool operator !=(Acceleration a, Acceleration b)
		{
			return a._mps2 != b._mps2;
		}

		private float _mps2;
	}
}
