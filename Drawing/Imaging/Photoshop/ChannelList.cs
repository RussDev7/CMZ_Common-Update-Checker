using System;
using System.Collections.Generic;
using System.Linq;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class ChannelList : List<Channel>
	{
		public Channel[] ToIdArray()
		{
			short maxId = this.Max((Channel x) => x.ID);
			Channel[] idArray = new Channel[(int)(maxId + 1)];
			foreach (Channel channel in this)
			{
				if (channel.ID >= 0)
				{
					idArray[(int)channel.ID] = channel;
				}
			}
			return idArray;
		}

		public Channel GetId(int id)
		{
			return this.Single((Channel x) => (int)x.ID == id);
		}

		public bool ContainsId(int id)
		{
			return base.Exists((Channel x) => (int)x.ID == id);
		}
	}
}
