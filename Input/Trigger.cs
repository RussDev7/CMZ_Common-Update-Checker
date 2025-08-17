using System;

namespace DNA.Input
{
	public struct Trigger
	{
		public static Trigger operator |(Trigger t1, Trigger t2)
		{
			return new Trigger
			{
				Pressed = (t1.Pressed || t2.Pressed),
				Released = (t1.Released || t2.Released),
				Held = (t1.Held || t2.Held)
			};
		}

		public bool Pressed;

		public bool Released;

		public bool Held;
	}
}
