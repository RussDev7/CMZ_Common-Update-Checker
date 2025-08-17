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
				SpriteManager spriteManager = new SpriteManager();
				Texture2D texture2D = input.ReadObject<Texture2D>();
				int num = input.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					string text = input.ReadString();
					Rectangle rectangle = new Rectangle(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
					spriteManager._sprites[text] = new Sprite(texture2D, rectangle);
				}
				return spriteManager;
			}
		}
	}
}
