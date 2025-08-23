using System;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class DistortSkinnedEffect : DNAEffect
	{
		public float DistortionScale
		{
			get
			{
				return this._distortionScale;
			}
			set
			{
				this._distortionScale = value;
			}
		}

		public float Alpha
		{
			get
			{
				return this.alpha;
			}
			set
			{
				this.alpha = value;
			}
		}

		public Texture2D Texture
		{
			get
			{
				return this.textureParam.GetValueTexture2D();
			}
			set
			{
				this.textureParam.SetValue(value);
			}
		}

		public int WeightsPerVertex
		{
			get
			{
				return this.weightsPerVertex;
			}
			set
			{
				if (value != 1 && value != 2 && value != 4)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this.weightsPerVertex = value;
			}
		}

		private void SetBlurEffectParameters(float dx, float dy)
		{
			EffectParameter weightsParameter = base.Parameters["SampleWeights"];
			EffectParameter offsetsParameter = base.Parameters["SampleOffsets"];
			int sampleCount = weightsParameter.Elements.Count;
			float[] sampleWeights = new float[sampleCount];
			Vector2[] sampleOffsets = new Vector2[sampleCount];
			sampleWeights[0] = DistortSkinnedEffect.ComputeGaussian(0f);
			sampleOffsets[0] = new Vector2(0f);
			float totalWeights = sampleWeights[0];
			for (int i = 0; i < sampleCount / 2; i++)
			{
				float weight = DistortSkinnedEffect.ComputeGaussian((float)(i + 1));
				sampleWeights[i * 2 + 1] = weight;
				sampleWeights[i * 2 + 2] = weight;
				totalWeights += weight * 2f;
				float sampleOffset = (float)(i * 2) + 1.5f;
				Vector2 delta = new Vector2(dx, dy) * sampleOffset;
				sampleOffsets[i * 2 + 1] = delta;
				sampleOffsets[i * 2 + 2] = -delta;
			}
			for (int j = 0; j < sampleWeights.Length; j++)
			{
				sampleWeights[j] /= totalWeights;
			}
			weightsParameter.SetValue(sampleWeights);
			offsetsParameter.SetValue(sampleOffsets);
		}

		private static float ComputeGaussian(float n)
		{
			return (float)(1.0 / Math.Sqrt(12.566370614359172) * Math.Exp((double)(-(double)(n * n) / 8f)));
		}

		public void SetBoneTransforms(Matrix[] boneTransforms)
		{
			if (boneTransforms == null || boneTransforms.Length == 0)
			{
				throw new ArgumentNullException("boneTransforms");
			}
			if (boneTransforms.Length > 72)
			{
				throw new ArgumentException();
			}
			this.bonesParam.SetValue(boneTransforms);
		}

		public Matrix[] GetBoneTransforms(int count)
		{
			if (count <= 0 || count > 72)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			Matrix[] bones = this.bonesParam.GetValueMatrixArray(count);
			for (int i = 0; i < bones.Length; i++)
			{
				bones[i].M44 = 1f;
			}
			return bones;
		}

		public DistortSkinnedEffect(Game game)
			: base(game.Content.Load<Effect>("DistortSkinnedEffect"))
		{
			this.CacheEffectParameters(null);
			Matrix[] identityBones = new Matrix[72];
			for (int i = 0; i < 72; i++)
			{
				identityBones[i] = Matrix.Identity;
			}
			this.SetBoneTransforms(identityBones);
		}

		protected DistortSkinnedEffect(DistortSkinnedEffect cloneSource)
			: base(cloneSource)
		{
			this.CacheEffectParameters(cloneSource);
			this.Blur = cloneSource.Blur;
			this.alpha = cloneSource.alpha;
			this._distortionScale = cloneSource._distortionScale;
			this.weightsPerVertex = cloneSource.weightsPerVertex;
		}

		public override Effect Clone()
		{
			return new DistortSkinnedEffect(this);
		}

		private void CacheEffectParameters(DistortSkinnedEffect cloneSource)
		{
			this.textureParam = base.Parameters["Texture"];
			this.distortionScaleParam = base.Parameters["DistortionScale"];
			this.bonesParam = base.Parameters["Bones"];
			this.shaderIndexParam = base.Parameters["ShaderIndex"];
			this.distortTechnique = base.Techniques["Distort"];
			this.distortBlurTechnique = base.Techniques["DistortBlur"];
			PresentationParameters pp = base.GraphicsDevice.PresentationParameters;
			int width = pp.BackBufferWidth;
			int height = pp.BackBufferHeight;
			SurfaceFormat backBufferFormat = pp.BackBufferFormat;
			DepthFormat depthStencilFormat = pp.DepthStencilFormat;
			this.SetBlurEffectParameters(1f / (float)width, 1f / (float)height);
		}

		protected override void OnApply()
		{
			int shaderIndex = 0;
			if (this.weightsPerVertex == 2)
			{
				shaderIndex++;
			}
			else if (this.weightsPerVertex == 4)
			{
				shaderIndex += 2;
			}
			this.shaderIndexParam.SetValue(shaderIndex);
			this.distortionScaleParam.SetValue(this._distortionScale);
			base.CurrentTechnique = (this.Blur ? this.distortBlurTechnique : this.distortTechnique);
			base.OnApply();
		}

		public const int MaxBones = 72;

		private const float blurAmount = 2f;

		private float _distortionScale = 0.1f;

		private EffectParameter textureParam;

		private EffectParameter bonesParam;

		private EffectParameter shaderIndexParam;

		private EffectParameter distortionScaleParam;

		private float alpha = 1f;

		public bool Blur = true;

		private EffectTechnique distortTechnique;

		private EffectTechnique distortBlurTechnique;

		private int weightsPerVertex = 4;
	}
}
