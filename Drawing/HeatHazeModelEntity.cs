using System;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class HeatHazeModelEntity : ModelEntity, IScreenDistortion
	{
		public Texture2D ScreenBackground
		{
			get
			{
				return this._backgroundImage;
			}
			set
			{
				this._backgroundImage = value;
			}
		}

		public HeatHazeModelEntity(Game game, Model model, Texture2D backgoundImage)
			: base(model)
		{
			this._backgroundImage = backgoundImage;
			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					part.Effect = new HeatHazeEffect(game);
				}
			}
			this.AlphaSort = true;
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			for (int i = 0; i < base.Model.Meshes.Count; i++)
			{
				ModelMesh mesh = base.Model.Meshes[i];
				for (int j = 0; j < mesh.Effects.Count; j++)
				{
					HeatHazeEffect effect = (HeatHazeEffect)mesh.Effects[j];
					effect.WaveMagnitude = this.WaveMagnitude;
					effect.ScreenTexture = this._backgroundImage;
				}
			}
			base.Draw(device, gameTime, view, projection);
		}

		private Texture2D _backgroundImage;

		public float WaveMagnitude = 0.2f;
	}
}
