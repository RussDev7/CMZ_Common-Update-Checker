using System;
using DNA.Drawing.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class SkinnedModelEntity : ModelEntity
	{
		private SkinedAnimationData SkinningData
		{
			get
			{
				return (SkinedAnimationData)base.Model.Tag;
			}
		}

		protected override Skeleton GetSkeleton()
		{
			return this.SkinningData.Skeleton;
		}

		public SkinnedModelEntity(Model model)
			: base(model)
		{
			this._skinTransforms = new Matrix[this.SkinningData.Skeleton.Count];
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			this.UpdateSkinTransforms();
		}

		private void UpdateSkinTransforms()
		{
			for (int bone = 0; bone < this._skinTransforms.Length; bone++)
			{
				this._skinTransforms[bone] = this.SkinningData.InverseBindPose[bone] * this._worldBoneTransforms[bone];
			}
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (effect is SkinnedEffect)
			{
				SkinnedEffect seffect = (SkinnedEffect)effect;
				seffect.SetBoneTransforms(this._skinTransforms);
				seffect.EnableDefaultLighting();
				seffect.SpecularColor = new Vector3(0.25f);
				seffect.SpecularPower = 16f;
			}
			else if (effect.Parameters["Bones"] != null)
			{
				effect.Parameters["Bones"].SetValue(this._skinTransforms);
			}
			return base.SetEffectParams(mesh, effect, gameTime, Matrix.Identity, view, projection);
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			for (int i = 0; i < base.Model.Meshes.Count; i++)
			{
				ModelMesh mesh = base.Model.Meshes[i];
				for (int j = 0; j < mesh.Effects.Count; j++)
				{
					Effect effect = mesh.Effects[j];
					this.SetEffectParams(mesh, effect, gameTime, base.LocalToWorld, view, projection);
				}
				mesh.Draw();
			}
			if (this.ShowSkeleton)
			{
				base.DrawWireframeBones(device, view, projection);
			}
		}

		protected Matrix[] _skinTransforms;
	}
}
