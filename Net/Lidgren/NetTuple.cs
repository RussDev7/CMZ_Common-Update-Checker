﻿using System;

namespace DNA.Net.Lidgren
{
	internal struct NetTuple<A, B>
	{
		public NetTuple(A item1, B item2)
		{
			this.Item1 = item1;
			this.Item2 = item2;
		}

		public A Item1;

		public B Item2;
	}
}
