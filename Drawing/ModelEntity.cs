using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DNA.Drawing.Animation;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class ModelEntity : Entity
	{
		public bool Animated
		{
			get
			{
				return this._animationData != null;
			}
		}

		public LayeredAnimationPlayer Animations
		{
			get
			{
				return this._animations;
			}
		}

		public ReadOnlyCollection<Matrix> BindPose
		{
			get
			{
				return this._bindPose;
			}
		}

		public Matrix[] DefaultPose
		{
			get
			{
				return this._defaultPose;
			}
		}

		public Matrix[] WorldBoneTransforms
		{
			get
			{
				return this._worldBoneTransforms;
			}
		}

		public Skeleton Skeleton
		{
			get
			{
				return this._skeleton;
			}
		}

		public bool Lighting
		{
			get
			{
				return this._lighting;
			}
			set
			{
				this._lighting = value;
			}
		}

		protected Model Model
		{
			get
			{
				return this._model;
			}
			set
			{
				this.SetupModel(value);
			}
		}

		private void GetDefaultPose(Matrix[] pose)
		{
			this.Skeleton.CopyTransformsTo(pose);
		}

		private void AssumeDefaultPose()
		{
			Skeleton skeleton = this.Skeleton;
			for (int i = 0; i < skeleton.Count; i++)
			{
				skeleton[i].SetTransform(this._defaultPose[i]);
			}
		}

		protected static void ChangeEffectUsedByMesh(ModelMesh mesh, Effect replacementEffect)
		{
			Dictionary<Effect, Effect> dictionary = new Dictionary<Effect, Effect>();
			foreach (Effect effect in mesh.Effects)
			{
				if (!dictionary.ContainsKey(effect))
				{
					Effect effect2 = replacementEffect.Clone();
					dictionary[effect] = effect2;
				}
			}
			foreach (ModelMeshPart modelMeshPart in mesh.MeshParts)
			{
				modelMeshPart.Effect = dictionary[modelMeshPart.Effect];
			}
		}

		protected static void ChangeEffectUsedByModel(Model model, Effect replacementEffect)
		{
			new Dictionary<Effect, Effect>();
			foreach (ModelMesh modelMesh in model.Meshes)
			{
				ModelEntity.ChangeEffectUsedByMesh(modelMesh, replacementEffect);
			}
		}

		protected virtual Skeleton GetSkeleton()
		{
			return Bone.BuildSkeleton(this._model);
		}

		protected override void OnApplyEffect(Effect sourceEffect)
		{
			ModelEntity.ChangeEffectUsedByModel(this._model, sourceEffect);
		}

		public void EnableDefaultLighting()
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = this._model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					if (modelMesh.Effects[j] is BasicEffect)
					{
						BasicEffect basicEffect = (BasicEffect)modelMesh.Effects[j];
						basicEffect.EnableDefaultLighting();
						basicEffect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetLighting(Vector3 ambient, Vector3 Direction0, Vector3 DColor0, Vector3 SColor0)
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = this._model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					if (modelMesh.Effects[j] is BasicEffect)
					{
						BasicEffect basicEffect = (BasicEffect)modelMesh.Effects[j];
						basicEffect.AmbientLightColor = ambient;
						DirectionalLight directionalLight = basicEffect.DirectionalLight0;
						directionalLight.DiffuseColor = DColor0;
						directionalLight.SpecularColor = SColor0;
						directionalLight.Direction = Direction0;
						directionalLight.Enabled = true;
						basicEffect.DirectionalLight1.Enabled = false;
						basicEffect.DirectionalLight2.Enabled = false;
						basicEffect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetLighting(Vector3 ambient, Vector3 Direction0, Vector3 DColor0, Vector3 SColor0, Vector3 Direction1, Vector3 DColor1, Vector3 SColor1)
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = this._model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					if (modelMesh.Effects[j] is BasicEffect)
					{
						BasicEffect basicEffect = (BasicEffect)modelMesh.Effects[j];
						basicEffect.AmbientLightColor = ambient;
						DirectionalLight directionalLight = basicEffect.DirectionalLight0;
						directionalLight.DiffuseColor = DColor0;
						directionalLight.SpecularColor = SColor0;
						directionalLight.Direction = Direction0;
						directionalLight.Enabled = false;
						directionalLight = basicEffect.DirectionalLight1;
						directionalLight.DiffuseColor = DColor1;
						directionalLight.SpecularColor = SColor1;
						directionalLight.Direction = Direction1;
						directionalLight.Enabled = true;
						basicEffect.DirectionalLight2.Enabled = false;
						basicEffect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetLighting(Vector3 ambient, Vector3 Direction0, Vector3 DColor0, Vector3 SColor0, Vector3 Direction1, Vector3 DColor1, Vector3 SColor1, Vector3 Direction2, Vector3 DColor2, Vector3 SColor2)
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = this._model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					if (modelMesh.Effects[j] is BasicEffect)
					{
						BasicEffect basicEffect = (BasicEffect)modelMesh.Effects[j];
						basicEffect.AmbientLightColor = ambient;
						DirectionalLight directionalLight = basicEffect.DirectionalLight0;
						directionalLight.DiffuseColor = DColor0;
						directionalLight.SpecularColor = SColor0;
						directionalLight.Direction = Direction0;
						directionalLight.Enabled = false;
						directionalLight = basicEffect.DirectionalLight1;
						directionalLight.DiffuseColor = DColor1;
						directionalLight.SpecularColor = SColor1;
						directionalLight.Direction = Direction1;
						directionalLight.Enabled = true;
						directionalLight = basicEffect.DirectionalLight2;
						directionalLight.DiffuseColor = DColor2;
						directionalLight.SpecularColor = SColor2;
						directionalLight.Direction = Direction2;
						directionalLight.Enabled = true;
						basicEffect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetAlphaTest(int referenceAlpha, CompareFunction compareFunction)
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = this._model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					AlphaTestEffect alphaTestEffect = (AlphaTestEffect)modelMesh.Effects[j];
					alphaTestEffect.ReferenceAlpha = referenceAlpha;
					alphaTestEffect.AlphaFunction = compareFunction;
				}
			}
		}

		public void EnablePerPixelLighting()
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = this._model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					if (modelMesh.Effects[j] is BasicEffect)
					{
						BasicEffect basicEffect = (BasicEffect)modelMesh.Effects[j];
						basicEffect.PreferPerPixelLighting = true;
						basicEffect.LightingEnabled = true;
					}
				}
			}
		}

		protected void AllocateBoneTransforms()
		{
			this._worldBoneTransforms = new Matrix[this.Skeleton.Count];
			this._defaultPose = new Matrix[this.Skeleton.Count];
		}

		public ModelEntity(Model model)
		{
			this.SetupModel(model);
		}

		private void SetupModel(Model model)
		{
			this._model = model;
			this._animationData = (AnimationData)model.Tag;
			this._skeleton = this.GetSkeleton();
			this.AllocateBoneTransforms();
			this.GetDefaultPose(this._defaultPose);
			Matrix[] array = new Matrix[this.Skeleton.Count];
			this.GetDefaultPose(array);
			this._bindPose = new ReadOnlyCollection<Matrix>(array);
			this.Skeleton.CopyAbsoluteBoneTransformsTo(this._worldBoneTransforms, base.LocalToWorld);
		}

		public AnimationPlayer PlayClip(string clipName, bool looping, IList<string> influenceBoneNames, TimeSpan blendTime)
		{
			return this.PlayClip(0, clipName, looping, influenceBoneNames, blendTime);
		}

		public AnimationPlayer PlayClip(string clipName, bool looping, IList<Bone> influenceBones, TimeSpan blendTime)
		{
			return this.PlayClip(0, clipName, looping, influenceBones, blendTime);
		}

		public AnimationPlayer PlayClip(string clipName, bool looping, TimeSpan blendTime)
		{
			return this.PlayClip(0, clipName, looping, blendTime);
		}

		public AnimationPlayer PlayClip(int channel, string clipName, bool looping, IList<string> influenceBoneNames, TimeSpan blendTime)
		{
			AnimationClip animationClip = this._animationData.AnimationClips[clipName];
			AnimationPlayer animationPlayer = new AnimationPlayer(animationClip, this.Skeleton.BonesFromNames(influenceBoneNames));
			animationPlayer.Looping = looping;
			animationPlayer.Play();
			this._animations.PlayAnimation(channel, animationPlayer, blendTime);
			return animationPlayer;
		}

		public AnimationPlayer PlayClip(int channel, string clipName, bool looping, IList<Bone> influenceBones, TimeSpan blendTime)
		{
			AnimationClip animationClip = this._animationData.AnimationClips[clipName];
			AnimationPlayer animationPlayer = new AnimationPlayer(animationClip, influenceBones);
			animationPlayer.Looping = looping;
			animationPlayer.Play();
			this._animations.PlayAnimation(channel, animationPlayer, blendTime);
			return animationPlayer;
		}

		public AnimationPlayer PlayClip(int channel, string clipName, bool looping, TimeSpan blendTime)
		{
			AnimationClip animationClip = this._animationData.AnimationClips[clipName];
			AnimationPlayer animationPlayer = new AnimationPlayer(animationClip);
			animationPlayer.Looping = looping;
			animationPlayer.Play();
			this._animations.PlayAnimation(channel, animationPlayer, blendTime);
			return animationPlayer;
		}

		public void DumpAnimationNames()
		{
			foreach (string text in this._animationData.AnimationClips.Keys)
			{
			}
		}

		public override BoundingSphere GetLocalBoundingSphere()
		{
			BoundingSphere boundingSphere = this._model.Meshes[0].BoundingSphere;
			for (int i = 1; i < this._model.Meshes.Count; i++)
			{
				boundingSphere = BoundingSphere.CreateMerged(boundingSphere, this._model.Meshes[i].BoundingSphere);
			}
			return boundingSphere;
		}

		public override BoundingBox GetAABB()
		{
			Vector3 vector = new Vector3(this.GetLocalBoundingSphere().Radius);
			Vector3 worldPosition = base.WorldPosition;
			return new BoundingBox(worldPosition - vector, worldPosition + vector);
		}

		protected override void OnMoved()
		{
			this._getWorldBones = true;
			base.OnMoved();
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (this.Animated)
			{
				this.AssumeDefaultPose();
				this._animations.Update(gameTime.ElapsedGameTime, this.Skeleton);
				this.Skeleton.CopyAbsoluteBoneTransformsTo(this._worldBoneTransforms, base.LocalToWorld);
			}
			else if (this._getWorldBones)
			{
				this.Skeleton.CopyAbsoluteBoneTransformsTo(this._worldBoneTransforms, base.LocalToWorld);
				this._getWorldBones = false;
			}
			base.OnUpdate(gameTime);
		}

		protected virtual EffectTechnique GetEffectTechnique(Effect effect)
		{
			return effect.Techniques[0];
		}

		protected virtual bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (effect is IEffectMatrices)
			{
				IEffectMatrices effectMatrices = (IEffectMatrices)effect;
				effectMatrices.World = world;
				effectMatrices.View = view;
				effectMatrices.Projection = projection;
			}
			if (effect is IEffectTime)
			{
				IEffectTime effectTime = (IEffectTime)effect;
				effectTime.ElaspedTime = gameTime.ElapsedGameTime;
				effectTime.TotalTime = gameTime.TotalGameTime;
			}
			if (effect is IEffectColor)
			{
				IEffectColor effectColor = (IEffectColor)effect;
				if (this.EntityColor != null)
				{
					effectColor.DiffuseColor = this.EntityColor.Value;
				}
			}
			if (effect is BasicEffect)
			{
				BasicEffect basicEffect = (BasicEffect)effect;
				if (this.EntityColor != null)
				{
					Color value = this.EntityColor.Value;
					basicEffect.DiffuseColor = this._cachedColor;
					basicEffect.Alpha = this._cachedAlpha;
				}
			}
			else if (effect is AlphaTestEffect)
			{
				AlphaTestEffect alphaTestEffect = (AlphaTestEffect)effect;
				if (this.EntityColor != null)
				{
					Color value2 = this.EntityColor.Value;
					alphaTestEffect.DiffuseColor = this._cachedColor;
					alphaTestEffect.Alpha = this._cachedAlpha;
				}
			}
			if (this.Technique == null)
			{
				effect.CurrentTechnique = effect.Techniques[0];
			}
			else
			{
				effect.CurrentTechnique = effect.Techniques[this.Technique];
			}
			return true;
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			if (this.EntityColor != null && this.EntityColor != null)
			{
				Color value = this.EntityColor.Value;
				this._cachedColor = value.ToVector3();
				this._cachedAlpha = (float)value.A / 255f;
			}
			int count = this._model.Meshes.Count;
			int i = 0;
			IL_00D1:
			while (i < count)
			{
				ModelMesh modelMesh = this._model.Meshes[i];
				Matrix matrix = this._worldBoneTransforms[modelMesh.ParentBone.Index];
				int count2 = modelMesh.Effects.Count;
				for (int j = 0; j < count2; j++)
				{
					if (!this.SetEffectParams(modelMesh, modelMesh.Effects[j], gameTime, matrix, view, projection))
					{
						IL_00CD:
						i++;
						goto IL_00D1;
					}
				}
				this.DrawMesh(device, modelMesh);
				goto IL_00CD;
			}
			base.Draw(device, gameTime, view, projection);
		}

		protected virtual void DrawMesh(GraphicsDevice device, ModelMesh mesh)
		{
			mesh.Draw();
		}

		protected void DrawWireframeBones(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
		{
			Matrix[] worldBoneTransforms = this._worldBoneTransforms;
			if (this._wireFrameVerts == null)
			{
				this._wireFrameVerts = new VertexPositionColor[worldBoneTransforms.Length * 2];
			}
			this._wireFrameVerts[0].Color = Color.Blue;
			this._wireFrameVerts[0].Position = worldBoneTransforms[0].Translation;
			this._wireFrameVerts[1] = this._wireFrameVerts[0];
			for (int i = 2; i < worldBoneTransforms.Length * 2; i += 2)
			{
				this._wireFrameVerts[i].Position = worldBoneTransforms[i / 2].Translation;
				this._wireFrameVerts[i].Color = Color.Red;
				this._wireFrameVerts[i + 1].Position = worldBoneTransforms[this.Skeleton[i / 2].Parent.Index].Translation;
				this._wireFrameVerts[i + 1].Color = Color.Green;
			}
			if (this._wireFrameEffect == null)
			{
				this._wireFrameEffect = new BasicEffect(graphicsDevice);
			}
			this._wireFrameEffect.LightingEnabled = false;
			this._wireFrameEffect.TextureEnabled = false;
			this._wireFrameEffect.VertexColorEnabled = true;
			this._wireFrameEffect.Projection = projection;
			this._wireFrameEffect.View = view;
			this._wireFrameEffect.World = Matrix.Identity;
			for (int j = 0; j < this._wireFrameEffect.CurrentTechnique.Passes.Count; j++)
			{
				EffectPass effectPass = this._wireFrameEffect.CurrentTechnique.Passes[j];
				effectPass.Apply();
				graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, this._wireFrameVerts, 0, worldBoneTransforms.Length);
			}
		}

		protected AnimationData _animationData;

		private LayeredAnimationPlayer _animations = new LayeredAnimationPlayer(16);

		public bool ShowSkeleton;

		private BasicEffect _wireFrameEffect;

		private VertexPositionColor[] _wireFrameVerts;

		public string Technique;

		protected Matrix[] _worldBoneTransforms;

		protected Matrix[] _defaultPose;

		protected ReadOnlyCollection<Matrix> _bindPose;

		private Skeleton _skeleton;

		private bool _lighting = true;

		private Model _model;

		private bool _getWorldBones = true;

		private Vector3 _cachedColor;

		private float _cachedAlpha;
	}
}
