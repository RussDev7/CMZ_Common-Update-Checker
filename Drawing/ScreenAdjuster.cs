using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public class ScreenAdjuster
	{
		public Vector2 ScaleFactor
		{
			get
			{
				return this._scale;
			}
		}

		public Size ScreenSize
		{
			get
			{
				return this._screenSize;
			}
			set
			{
				this._screenSize = value;
				this.Recalulate();
			}
		}

		public Size AuthoredSize
		{
			get
			{
				return this._authoredSize;
			}
			set
			{
				this._authoredSize = value;
				this.Recalulate();
			}
		}

		public Rectangle ScreenRect
		{
			get
			{
				return new Rectangle(0, 0, this._screenSize.Width, this._screenSize.Height);
			}
		}

		private void Recalulate()
		{
			float num = (float)this._authoredSize.Height / (float)this._authoredSize.Width;
			float num2 = (float)this._screenSize.Height / (float)this._screenSize.Width;
			this._scale = new Vector2((float)this._screenSize.Width / (float)this._authoredSize.Width, (float)this._screenSize.Height / (float)this._authoredSize.Height);
			this._authoredToStretched = Matrix.CreateScale((float)this._screenSize.Width / (float)this._authoredSize.Width, (float)this._screenSize.Height / (float)this._authoredSize.Height, 1f);
			if (num < num2)
			{
				this._authoredToClipped = Matrix.CreateScale((float)this._screenSize.Height / (float)this._authoredSize.Height, (float)this._screenSize.Height / (float)this._authoredSize.Height, 1f) * Matrix.CreateTranslation(new Vector3(((float)this._screenSize.Width - (float)(this._authoredSize.Width * this._screenSize.Height) / (float)this._authoredSize.Height) / 2f, 0f, 0f));
				this._authoredToLetterBox = Matrix.CreateScale((float)this._screenSize.Width / (float)this._authoredSize.Width, (float)this._screenSize.Width / (float)this._authoredSize.Width, 1f) * Matrix.CreateTranslation(new Vector3(0f, ((float)this._screenSize.Height - (float)(this._authoredSize.Height * this._screenSize.Width) / (float)this._authoredSize.Width) / 2f, 0f));
				return;
			}
			this._authoredToClipped = Matrix.CreateScale((float)this._screenSize.Width / (float)this._authoredSize.Width, (float)this._screenSize.Width / (float)this._authoredSize.Width, 1f) * Matrix.CreateTranslation(new Vector3(0f, ((float)this._screenSize.Height - (float)(this._authoredSize.Height * this._screenSize.Width) / (float)this._authoredSize.Width) / 2f, 0f));
			this._authoredToLetterBox = Matrix.CreateScale((float)this._screenSize.Height / (float)this._authoredSize.Height, (float)this._screenSize.Height / (float)this._authoredSize.Height, 1f) * Matrix.CreateTranslation(new Vector3(((float)this._screenSize.Width - (float)(this._authoredSize.Width * this._screenSize.Height) / (float)this._authoredSize.Height) / 2f, 0f, 0f));
		}

		private static Rectangle Transform(Rectangle original, Matrix matrix)
		{
			Vector3 vector = Vector3.Transform(new Vector3((float)original.Left, (float)original.Top, 0f), matrix);
			Vector3 vector2 = Vector3.TransformNormal(new Vector3((float)original.Width, (float)original.Height, 0f), matrix);
			return new Rectangle((int)vector.X, (int)vector.Y, (int)vector2.X, (int)vector2.Y);
		}

		private static Vector2 Transform(Vector2 original, Matrix matrix)
		{
			Vector3 vector = Vector3.Transform(new Vector3(original.X, original.Y, 0f), matrix);
			return new Vector2(vector.X, vector.Y);
		}

		public Vector2 Scale(Vector2 original)
		{
			return original * this._scale;
		}

		public Rectangle TransformClipped(Rectangle original)
		{
			return ScreenAdjuster.Transform(original, this._authoredToClipped);
		}

		public Rectangle TransformStretched(Rectangle original)
		{
			return ScreenAdjuster.Transform(original, this._authoredToStretched);
		}

		public Rectangle TransformLetterBox(Rectangle original)
		{
			return ScreenAdjuster.Transform(original, this._authoredToLetterBox);
		}

		public Vector2 TransformClipped(Vector2 original)
		{
			return ScreenAdjuster.Transform(original, this._authoredToClipped);
		}

		public Vector2 TransformStretched(Vector2 original)
		{
			return ScreenAdjuster.Transform(original, this._authoredToStretched);
		}

		public Vector2 TransformLetterBox(Vector2 original)
		{
			return ScreenAdjuster.Transform(original, this._authoredToLetterBox);
		}

		private Size _authoredSize;

		private Size _screenSize;

		private Vector2 _scale;

		private Matrix _authoredToLetterBox = Matrix.Identity;

		private Matrix _authoredToClipped = Matrix.Identity;

		private Matrix _authoredToStretched = Matrix.Identity;
	}
}
