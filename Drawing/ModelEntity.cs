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
			Skeleton bones = this.Skeleton;
			for (int i = 0; i < bones.Count; i++)
			{
				bones[i].SetTransform(this._defaultPose[i]);
			}
		}

		protected static void ChangeEffectUsedByMesh(ModelMesh mesh, Effect replacementEffect)
		{
			Dictionary<Effect, Effect> effectMapping = new Dictionary<Effect, Effect>();
			foreach (Effect oldEffect in mesh.Effects)
			{
				if (!effectMapping.ContainsKey(oldEffect))
				{
					Effect newEffect = replacementEffect.Clone();
					effectMapping[oldEffect] = newEffect;
				}
			}
			foreach (ModelMeshPart meshPart in mesh.MeshParts)
			{
				meshPart.Effect = effectMapping[meshPart.Effect];
			}
		}

		protected static void ChangeEffectUsedByModel(Model model, Effect replacementEffect)
		{
			new Dictionary<Effect, Effect>();
			foreach (ModelMesh mesh in model.Meshes)
			{
				ModelEntity.ChangeEffectUsedByMesh(mesh, replacementEffect);
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
				ModelMesh mesh = this._model.Meshes[i];
				for (int j = 0; j < mesh.Effects.Count; j++)
				{
					if (mesh.Effects[j] is BasicEffect)
					{
						BasicEffect effect = (BasicEffect)mesh.Effects[j];
						effect.EnableDefaultLighting();
						effect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetLighting(Vector3 ambient, Vector3 Direction0, Vector3 DColor0, Vector3 SColor0)
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh mesh = this._model.Meshes[i];
				for (int j = 0; j < mesh.Effects.Count; j++)
				{
					if (mesh.Effects[j] is BasicEffect)
					{
						BasicEffect effect = (BasicEffect)mesh.Effects[j];
						effect.AmbientLightColor = ambient;
						DirectionalLight d = effect.DirectionalLight0;
						d.DiffuseColor = DColor0;
						d.SpecularColor = SColor0;
						d.Direction = Direction0;
						d.Enabled = true;
						effect.DirectionalLight1.Enabled = false;
						effect.DirectionalLight2.Enabled = false;
						effect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetLighting(Vector3 ambient, Vector3 Direction0, Vector3 DColor0, Vector3 SColor0, Vector3 Direction1, Vector3 DColor1, Vector3 SColor1)
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh mesh = this._model.Meshes[i];
				for (int j = 0; j < mesh.Effects.Count; j++)
				{
					if (mesh.Effects[j] is BasicEffect)
					{
						BasicEffect effect = (BasicEffect)mesh.Effects[j];
						effect.AmbientLightColor = ambient;
						DirectionalLight d = effect.DirectionalLight0;
						d.DiffuseColor = DColor0;
						d.SpecularColor = SColor0;
						d.Direction = Direction0;
						d.Enabled = false;
						d = effect.DirectionalLight1;
						d.DiffuseColor = DColor1;
						d.SpecularColor = SColor1;
						d.Direction = Direction1;
						d.Enabled = true;
						effect.DirectionalLight2.Enabled = false;
						effect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetLighting(Vector3 ambient, Vector3 Direction0, Vector3 DColor0, Vector3 SColor0, Vector3 Direction1, Vector3 DColor1, Vector3 SColor1, Vector3 Direction2, Vector3 DColor2, Vector3 SColor2)
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh mesh = this._model.Meshes[i];
				for (int j = 0; j < mesh.Effects.Count; j++)
				{
					if (mesh.Effects[j] is BasicEffect)
					{
						BasicEffect effect = (BasicEffect)mesh.Effects[j];
						effect.AmbientLightColor = ambient;
						DirectionalLight d = effect.DirectionalLight0;
						d.DiffuseColor = DColor0;
						d.SpecularColor = SColor0;
						d.Direction = Direction0;
						d.Enabled = false;
						d = effect.DirectionalLight1;
						d.DiffuseColor = DColor1;
						d.SpecularColor = SColor1;
						d.Direction = Direction1;
						d.Enabled = true;
						d = effect.DirectionalLight2;
						d.DiffuseColor = DColor2;
						d.SpecularColor = SColor2;
						d.Direction = Direction2;
						d.Enabled = true;
						effect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetAlphaTest(int referenceAlpha, CompareFunction compareFunction)
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh mesh = this._model.Meshes[i];
				for (int j = 0; j < mesh.Effects.Count; j++)
				{
					AlphaTestEffect effect = (AlphaTestEffect)mesh.Effects[j];
					effect.ReferenceAlpha = referenceAlpha;
					effect.AlphaFunction = compareFunction;
				}
			}
		}

		public void EnablePerPixelLighting()
		{
			for (int i = 0; i < this._model.Meshes.Count; i++)
			{
				ModelMesh mesh = this._model.Meshes[i];
				for (int j = 0; j < mesh.Effects.Count; j++)
				{
					if (mesh.Effects[j] is BasicEffect)
					{
						BasicEffect effect = (BasicEffect)mesh.Effects[j];
						effect.PreferPerPixelLighting = true;
						effect.LightingEnabled = true;
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
			Matrix[] bindPose = new Matrix[this.Skeleton.Count];
			this.GetDefaultPose(bindPose);
			this._bindPose = new ReadOnlyCollection<Matrix>(bindPose);
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
			AnimationClip clip = this._animationData.AnimationClips[clipName];
			AnimationPlayer animationPlayer = new AnimationPlayer(clip, this.Skeleton.BonesFromNames(influenceBoneNames));
			animationPlayer.Looping = looping;
			animationPlayer.Play();
			this._animations.PlayAnimation(channel, animationPlayer, blendTime);
			return animationPlayer;
		}

		public AnimationPlayer PlayClip(int channel, string clipName, bool looping, IList<Bone> influenceBones, TimeSpan blendTime)
		{
			AnimationClip clip = this._animationData.AnimationClips[clipName];
			AnimationPlayer animationPlayer = new AnimationPlayer(clip, influenceBones);
			animationPlayer.Looping = looping;
			animationPlayer.Play();
			this._animations.PlayAnimation(channel, animationPlayer, blendTime);
			return animationPlayer;
		}

		public AnimationPlayer PlayClip(int channel, string clipName, bool looping, TimeSpan blendTime)
		{
			AnimationClip clip = this._animationData.AnimationClips[clipName];
			AnimationPlayer animationPlayer = new AnimationPlayer(clip);
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
			BoundingSphere sphere = this._model.Meshes[0].BoundingSphere;
			for (int i = 1; i < this._model.Meshes.Count; i++)
			{
				sphere = BoundingSphere.CreateMerged(sphere, this._model.Meshes[i].BoundingSphere);
			}
			return sphere;
		}

		public override BoundingBox GetAABB()
		{
			Vector3 r = new Vector3(this.GetLocalBoundingSphere().Radius);
			Vector3 wp = base.WorldPosition;
			return new BoundingBox(wp - r, wp + r);
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
				IEffectMatrices emat = (IEffectMatrices)effect;
				emat.World = world;
				emat.View = view;
				emat.Projection = projection;
			}
			if (effect is IEffectTime)
			{
				IEffectTime emat2 = (IEffectTime)effect;
				emat2.ElaspedTime = gameTime.ElapsedGameTime;
				emat2.TotalTime = gameTime.TotalGameTime;
			}
			if (effect is IEffectColor)
			{
				IEffectColor emat3 = (IEffectColor)effect;
				if (this.EntityColor != null)
				{
					emat3.DiffuseColor = this.EntityColor.Value;
				}
			}
			if (effect is BasicEffect)
			{
				BasicEffect be = (BasicEffect)effect;
				if (this.EntityColor != null)
				{
					Color value = this.EntityColor.Value;
					be.DiffuseColor = this._cachedColor;
					be.Alpha = this._cachedAlpha;
				}
			}
			else if (effect is AlphaTestEffect)
			{
				AlphaTestEffect be2 = (AlphaTestEffect)effect;
				if (this.EntityColor != null)
				{
					Color value2 = this.EntityColor.Value;
					be2.DiffuseColor = this._cachedColor;
					be2.Alpha = this._cachedAlpha;
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
				Color col = this.EntityColor.Value;
				this._cachedColor = col.ToVector3();
				this._cachedAlpha = (float)col.A / 255f;
			}
			int meshCount = this._model.Meshes.Count;
			int i = 0;
			IL_00D1:
			while (i < meshCount)
			{
				ModelMesh mesh = this._model.Meshes[i];
				Matrix world = this._worldBoneTransforms[mesh.ParentBone.Index];
				int effectCount = mesh.Effects.Count;
				for (int j = 0; j < effectCount; j++)
				{
					if (!this.SetEffectParams(mesh, mesh.Effects[j], gameTime, world, view, projection))
					{
						IL_00CD:
						i++;
						goto IL_00D1;
					}
				}
				this.DrawMesh(device, mesh);
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
			Matrix[] worldBones = this._worldBoneTransforms;
			if (this._wireFrameVerts == null)
			{
				this._wireFrameVerts = new VertexPositionColor[worldBones.Length * 2];
			}
			this._wireFrameVerts[0].Color = Color.Blue;
			this._wireFrameVerts[0].Position = worldBones[0].Translation;
			this._wireFrameVerts[1] = this._wireFrameVerts[0];
			for (int i = 2; i < worldBones.Length * 2; i += 2)
			{
				this._wireFrameVerts[i].Position = worldBones[i / 2].Translation;
				this._wireFrameVerts[i].Color = Color.Red;
				this._wireFrameVerts[i + 1].Position = worldBones[this.Skeleton[i / 2].Parent.Index].Translation;
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
				EffectPass pass = this._wireFrameEffect.CurrentTechnique.Passes[j];
				pass.Apply();
				graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, this._wireFrameVerts, 0, worldBones.Length);
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
