﻿using System;

namespace DNA.Data.Units
{
	[Serializable]
	public struct Volume
	{
		public float Liters
		{
			get
			{
				return this._liters;
			}
		}

		public float CubicInches
		{
			get
			{
				return this._liters * 61.023743f;
			}
		}

		public float Tablespoons
		{
			get
			{
				return this._liters * 67.628044f;
			}
		}

		public float Teaspoons
		{
			get
			{
				return this._liters * 202.88414f;
			}
		}

		public float Cups
		{
			get
			{
				return this._liters * 4.2267528f;
			}
		}

		public float Gallons
		{
			get
			{
				return this._liters * 0.26417205f;
			}
		}

		public float Pints
		{
			get
			{
				return this._liters * 2.1133764f;
			}
		}

		public float Quarts
		{
			get
			{
				return this._liters * 1.0566882f;
			}
		}

		public float FluidOunces
		{
			get
			{
				return this._liters * 33.814022f;
			}
		}

		public static Volume Parse(string unit, float amount)
		{
			unit = unit.ToLower();
			string text;
			switch (text = unit)
			{
			case "c":
				return Volume.FromCups(amount);
			case "cup":
				return Volume.FromCups(amount);
			case "cups":
				return Volume.FromCups(amount);
			case "tbsp":
				return Volume.FromTablespoons(amount);
			case "tablespoon":
				return Volume.FromTablespoons(amount);
			case "tablespoons":
				return Volume.FromTablespoons(amount);
			case "tsp":
				return Volume.FromTeaspoons(amount);
			case "teaspoon":
				return Volume.FromTeaspoons(amount);
			case "teaspoons":
				return Volume.FromTeaspoons(amount);
			case "pts":
				return Volume.FromPints(amount);
			case "pint":
				return Volume.FromPints(amount);
			case "pints":
				return Volume.FromPints(amount);
			case "qts":
				return Volume.FromQuarts(amount);
			case "quart":
				return Volume.FromQuarts(amount);
			case "quarts":
				return Volume.FromQuarts(amount);
			case "gal":
				return Volume.FromGallons(amount);
			case "gallon":
				return Volume.FromGallons(amount);
			case "gallons":
				return Volume.FromGallons(amount);
			case "fl oz":
				return Volume.FromFluidOunces(amount);
			case "fluid ounce":
				return Volume.FromFluidOunces(amount);
			case "fluid ounces":
				return Volume.FromFluidOunces(amount);
			case "ml":
				return Volume.FromMilliliters(amount);
			case "milliliter":
				return Volume.FromMilliliters(amount);
			case "milliliters":
				return Volume.FromMilliliters(amount);
			case "l":
				return Volume.FromLiters(amount);
			case "liter":
				return Volume.FromLiters(amount);
			case "liters":
				return Volume.FromLiters(amount);
			}
			throw new ArgumentException("Unreconized Unit " + unit);
		}

		public static Volume Parse(string str)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return this.Liters.ToString() + " L";
		}

		public static Volume FromPints(float pints)
		{
			return new Volume(pints / 2.1133764f);
		}

		public static Volume FromGallons(float gallons)
		{
			return new Volume(gallons / 0.26417205f);
		}

		public static Volume FromMilliliters(float milliliters)
		{
			return new Volume(milliliters / 1000f);
		}

		public static Volume FromLiters(float liters)
		{
			return new Volume(liters);
		}

		public static Volume FromCups(float cups)
		{
			return new Volume(cups / 4.2267528f);
		}

		public static Volume FromTablespoons(float tbs)
		{
			return new Volume(tbs / 67.628044f);
		}

		public static Volume FromTeaspoons(float tsp)
		{
			return new Volume(tsp / 202.88414f);
		}

		public static Volume FromFluidOunces(float floz)
		{
			return new Volume(floz / 33.814022f);
		}

		public static Volume FromQuarts(float qts)
		{
			return new Volume(qts / 1.0566882f);
		}

		public static Volume FromCubicInches(float in2)
		{
			return new Volume(in2 / 61.023743f);
		}

		public static Volume operator +(Volume m1, Volume m2)
		{
			return Volume.FromLiters(m1._liters + m2._liters);
		}

		public static Volume operator -(Volume m1, Volume m2)
		{
			return Volume.FromLiters(m1._liters - m2._liters);
		}

		public static double operator /(Volume m1, Volume m2)
		{
			return (double)(m1._liters / m2._liters);
		}

		public static Volume operator *(Volume m1, float scalar)
		{
			return Volume.FromLiters(m1._liters * scalar);
		}

		public static Volume operator /(Volume m1, float scalar)
		{
			return Volume.FromLiters(m1._liters / scalar);
		}

		private Volume(float liters)
		{
			this._liters = liters;
		}

		public override int GetHashCode()
		{
			return this._liters.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Volume b = (Volume)obj;
			return this._liters == b._liters;
		}

		public static bool operator ==(Volume a, Volume b)
		{
			return a._liters == b._liters;
		}

		public static bool operator !=(Volume a, Volume b)
		{
			return a._liters != b._liters;
		}

		private const float tablespoonsPerLiter = 67.628044f;

		private const float teaspoonsPerLiter = 202.88414f;

		private const float cupsPerLiter = 4.2267528f;

		private const float gallonsPerLiter = 0.26417205f;

		private const float pintsPerLiter = 2.1133764f;

		private const float quartsPerLiter = 1.0566882f;

		private const float fluidOuncesPerLiter = 33.814022f;

		private const float cubicInchesPerLiter = 61.023743f;

		private const float millilitersPerLiter = 1000f;

		private float _liters;
	}
}
