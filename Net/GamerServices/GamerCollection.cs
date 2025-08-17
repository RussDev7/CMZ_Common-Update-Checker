using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DNA.Net.GamerServices
{
	public class GamerCollection<T> : ReadOnlyCollection<T>, IEnumerable<Gamer>, IEnumerable where T : Gamer
	{
		internal GamerCollection(IList<T> list)
			: base(list)
		{
		}

		internal GamerCollection()
			: base(new T[0])
		{
		}

		IEnumerator<Gamer> IEnumerable<Gamer>.GetEnumerator()
		{
			return base.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return base.GetEnumerator();
		}
	}
}
