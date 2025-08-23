using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class SpriteManager
	{
		public Sprite this[string name]
		{
			get
			{
				return this._sprites[name];
			}
		}

		private Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

		public class SpriteManagerReader : ContentTypeReader<SpriteManager>
		{
			protected override SpriteManager Read(ContentReader input, SpriteManager existingInstance)
			{
				SpriteManager manager = new SpriteManager();
				Texture2D texture = input.ReadObject<Texture2D>();
				int count = input.ReadInt32();
				for (int i = 0; i < count; i++)
				{
					string name = input.ReadString();
					Rectangle rect = new Rectangle(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
					manager._sprites[name] = new Sprite(texture, rect);
				}
				return manager;
			}
		}
	}
}
