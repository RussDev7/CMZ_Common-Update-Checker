﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class Sprite
	{
		public Texture2D Texture
		{
			get
			{
				return this._texture;
			}
		}

		public Rectangle SourceRectangle
		{
			get
			{
				return this._sourceRectangle;
			}
		}

		public RectangleF UVRectangle
		{
			get
			{
				return new RectangleF((float)this._sourceRectangle.Left / (float)this._texture.Width, (float)this._sourceRectangle.Top / (float)this._texture.Height, (float)this._sourceRectangle.Width / (float)this._texture.Width, (float)this._sourceRectangle.Height / (float)this._texture.Height);
			}
		}

		public int Width
		{
			get
			{
				return this._sourceRectangle.Width;
			}
		}

		public int Height
		{
			get
			{
				return this._sourceRectangle.Height;
			}
		}

		public Sprite(Texture2D texture, Rectangle sourceRectangle)
		{
			this._texture = texture;
			this._sourceRectangle = sourceRectangle;
		}

		public void Draw(SpriteBatch batch, Rectangle destinationRectangle, Color color)
		{
			batch.Draw(this._texture, destinationRectangle, new Rectangle?(this._sourceRectangle), color);
		}

		public void Draw(SpriteBatch batch, Vector2 position, Color color)
		{
			batch.Draw(this._texture, position, new Rectangle?(this._sourceRectangle), color);
		}

		public void Draw(SpriteBatch batch, Vector2 position, float scale, Color color)
		{
			batch.Draw(this._texture, new Rectangle((int)position.X, (int)position.Y, (int)((float)this._sourceRectangle.Width * scale), (int)((float)this._sourceRectangle.Height * scale)), new Rectangle?(this._sourceRectangle), color);
		}

		public void Draw(SpriteBatch batch, Vector2 position, float scale, Color color, SpriteEffects effects)
		{
			Rectangle rectangle = new Rectangle((int)position.X, (int)position.Y, (int)((float)this._sourceRectangle.Width * scale), (int)((float)this._sourceRectangle.Height * scale));
			batch.Draw(this._texture, rectangle, new Rectangle?(this._sourceRectangle), color, 0f, Vector2.Zero, effects, 1f);
		}

		public void Draw(SpriteBatch batch, Rectangle destinationRectangle, Rectangle sourceRectangle, Color color)
		{
			Rectangle rectangle = new Rectangle(sourceRectangle.Left + this._sourceRectangle.Left, sourceRectangle.Top + this._sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height);
			batch.Draw(this._texture, destinationRectangle, new Rectangle?(rectangle), color);
		}

		public void Draw(SpriteBatch batch, Vector2 position, Rectangle sourceRectangle, Color color)
		{
			this.Draw(batch, new Rectangle((int)position.X, (int)position.Y, sourceRectangle.Width, sourceRectangle.Height), sourceRectangle, color);
		}

		public void Draw(SpriteBatch batch, Rectangle destinationRectangle, Rectangle sourceRectangle, Color color, Angle rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
		{
			Rectangle rectangle = new Rectangle(sourceRectangle.Left + this._sourceRectangle.Left, sourceRectangle.Top + this._sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height);
			batch.Draw(this._texture, destinationRectangle, new Rectangle?(rectangle), color, rotation.Radians, origin, effects, layerDepth);
		}

		public void Draw(SpriteBatch batch, Vector2 position, Color color, Angle rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
		{
			batch.Draw(this._texture, position, new Rectangle?(this._sourceRectangle), color, rotation.Radians, origin, scale, effects, layerDepth);
		}

		public void Draw(SpriteBatch batch, Vector2 position, Rectangle sourceRectangle, Color color, Angle rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
		{
			Rectangle rectangle = new Rectangle(sourceRectangle.Left + this._sourceRectangle.Left, sourceRectangle.Top + this._sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height);
			batch.Draw(this._texture, position, new Rectangle?(rectangle), color, rotation.Radians, origin, scale, effects, layerDepth);
		}

		public void Draw(SpriteBatch batch, Vector2 position, Rectangle sourceRectangle, Color color, Angle rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
		{
			Rectangle rectangle = new Rectangle(sourceRectangle.Left + this._sourceRectangle.Left, sourceRectangle.Top + this._sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height);
			batch.Draw(this._texture, position, new Rectangle?(rectangle), color, rotation.Radians, origin, scale, effects, layerDepth);
		}

		private Texture2D _texture;

		private Rectangle _sourceRectangle;
	}
}
