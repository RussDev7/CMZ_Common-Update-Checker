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
			for (int i = 0; i < this._skinTransforms.Length; i++)
			{
				this._skinTransforms[i] = this.SkinningData.InverseBindPose[i] * this._worldBoneTransforms[i];
			}
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (effect is SkinnedEffect)
			{
				SkinnedEffect skinnedEffect = (SkinnedEffect)effect;
				skinnedEffect.SetBoneTransforms(this._skinTransforms);
				skinnedEffect.EnableDefaultLighting();
				skinnedEffect.SpecularColor = new Vector3(0.25f);
				skinnedEffect.SpecularPower = 16f;
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
				ModelMesh modelMesh = base.Model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					Effect effect = modelMesh.Effects[j];
					this.SetEffectParams(modelMesh, effect, gameTime, base.LocalToWorld, view, projection);
				}
				modelMesh.Draw();
			}
			if (this.ShowSkeleton)
			{
				base.DrawWireframeBones(device, view, projection);
			}
		}

		protected Matrix[] _skinTransforms;
	}
}
