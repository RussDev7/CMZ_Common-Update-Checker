using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.Effects
{
	public class DNAEffect : Effect, IEffectMatrices, IEffectTime, IEffectColor, IEffectTextured
	{
		public DNAEffect(DNAEffect cloneSource)
			: base(cloneSource)
		{
			base.GraphicsDevice.DeviceReset += this.GraphicsDevice_DeviceReset;
			this.SetupParams();
		}

		public DNAEffect(Effect cloneSource)
			: base(cloneSource)
		{
			base.GraphicsDevice.DeviceReset += this.GraphicsDevice_DeviceReset;
			this.SetupParams();
		}

		private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
		{
			this._alteredParams = (DNAEffect.ParamFlags)4294967295U;
		}

		public Matrix Projection
		{
			get
			{
				return this._proj;
			}
			set
			{
				this._proj = value;
				this._alteredParams |= DNAEffect.ParamFlags.Projection;
			}
		}

		public Matrix View
		{
			get
			{
				return this._view;
			}
			set
			{
				this._view = value;
				this._alteredParams |= DNAEffect.ParamFlags.View;
			}
		}

		public Matrix World
		{
			get
			{
				return this._world;
			}
			set
			{
				this._world = value;
				this._alteredParams |= DNAEffect.ParamFlags.World;
			}
		}

		public TimeSpan TotalTime
		{
			get
			{
				return this._totalTime;
			}
			set
			{
				this._totalTime = value;
				if (this._totalTimeParam != null)
				{
					this._alteredParams |= DNAEffect.ParamFlags.Time;
				}
			}
		}

		public TimeSpan ElaspedTime
		{
			get
			{
				return this._elaspedTime;
			}
			set
			{
				this._elaspedTime = value;
				if (this._elaspedTimeParam != null)
				{
					this._alteredParams |= DNAEffect.ParamFlags.ElaspedTime;
				}
			}
		}

		public ColorF DiffuseColor
		{
			get
			{
				return this._diffuseColor;
			}
			set
			{
				this._diffuseColor = value;
				if (this._diffuseColorParam != null)
				{
					this._alteredParams |= DNAEffect.ParamFlags.Diffuse;
				}
			}
		}

		public ColorF AmbientColor
		{
			get
			{
				return this._ambientColor;
			}
			set
			{
				this._ambientColor = value;
				if (this._ambientColorParam != null)
				{
					this._alteredParams |= DNAEffect.ParamFlags.Ambient;
				}
			}
		}

		public ColorF SpecularColor
		{
			get
			{
				return this._specularColor;
			}
			set
			{
				this._specularColor = value;
				if (this._specularColorParam != null)
				{
					this._alteredParams |= DNAEffect.ParamFlags.Specular;
				}
			}
		}

		public ColorF EmissiveColor
		{
			get
			{
				return this._emissiveColor;
			}
			set
			{
				this._emissiveColor = value;
				if (this._emissiveColorParam != null)
				{
					this._alteredParams |= DNAEffect.ParamFlags.Emissive;
				}
			}
		}

		public Texture DiffuseMap
		{
			get
			{
				return this._diffuseMap;
			}
			set
			{
				this._diffuseMap = value;
				if (this._diffuseMap != null)
				{
					if (this._diffuseMapParam != null)
					{
						this._alteredParams |= DNAEffect.ParamFlags.DiffuseMap;
						return;
					}
				}
				else
				{
					this._alteredParams &= ~DNAEffect.ParamFlags.DiffuseMap;
				}
			}
		}

		public Texture OpacityMap
		{
			get
			{
				return this._opacityMap;
			}
			set
			{
				this._opacityMap = value;
				if (this._opacityMap != null)
				{
					if (this._opacityMapParam != null)
					{
						this._alteredParams |= DNAEffect.ParamFlags.OpacityMap;
						return;
					}
				}
				else
				{
					this._alteredParams &= ~DNAEffect.ParamFlags.OpacityMap;
				}
			}
		}

		public Texture SpecularMap
		{
			get
			{
				return this._specularMap;
			}
			set
			{
				this._specularMap = value;
				if (this._specularMap != null)
				{
					if (this._specularMapParam != null)
					{
						this._alteredParams |= DNAEffect.ParamFlags.SpecularMap;
						return;
					}
				}
				else
				{
					this._alteredParams &= ~DNAEffect.ParamFlags.SpecularMap;
				}
			}
		}

		public Texture NormalMap
		{
			get
			{
				return this._normalMap;
			}
			set
			{
				this._normalMap = value;
				if (this._normalMap != null)
				{
					if (this._normalMapParam != null)
					{
						this._alteredParams |= DNAEffect.ParamFlags.NormalMap;
						return;
					}
				}
				else
				{
					this._alteredParams &= ~DNAEffect.ParamFlags.NormalMap;
				}
			}
		}

		public Texture DisplacementMap
		{
			get
			{
				return this._displacementMap;
			}
			set
			{
				this._displacementMap = value;
				if (this._displacementMap != null)
				{
					if (this._displacementMapParam != null)
					{
						this._alteredParams |= DNAEffect.ParamFlags.DisplacementMap;
						return;
					}
				}
				else
				{
					this._alteredParams &= ~DNAEffect.ParamFlags.DisplacementMap;
				}
			}
		}

		public Texture LightMap
		{
			get
			{
				return this._lightMap;
			}
			set
			{
				this._lightMap = value;
				if (this._lightMap != null)
				{
					if (this._lightMapParam != null)
					{
						this._alteredParams |= DNAEffect.ParamFlags.LightMap;
						return;
					}
				}
				else
				{
					this._alteredParams &= ~DNAEffect.ParamFlags.LightMap;
				}
			}
		}

		public Texture ReflectionMap
		{
			get
			{
				return this._reflectionMap;
			}
			set
			{
				this._reflectionMap = value;
				if (this._reflectionMap != null)
				{
					if (this._reflectionMapParam != null)
					{
						this._alteredParams |= DNAEffect.ParamFlags.ReflectionMap;
						return;
					}
				}
				else
				{
					this._alteredParams &= ~DNAEffect.ParamFlags.ReflectionMap;
				}
			}
		}

		public void SetupParams()
		{
			this._worldParam = base.Parameters.GetParameterBySemantic("WORLD");
			this._worldInvParam = base.Parameters.GetParameterBySemantic("WORLDI");
			this._worldInvTrnParam = base.Parameters.GetParameterBySemantic("WORLDIT");
			this._worldTrnParam = base.Parameters.GetParameterBySemantic("WORLDT");
			this._viewParam = base.Parameters.GetParameterBySemantic("VIEW");
			this._viewTrnParam = base.Parameters.GetParameterBySemantic("VIEWT");
			this._viewInvParam = base.Parameters.GetParameterBySemantic("VIEWI");
			this._viewInvTrnParam = base.Parameters.GetParameterBySemantic("VIEWIT");
			this._projParam = base.Parameters.GetParameterBySemantic("PROJECTION");
			this._projTrnParam = base.Parameters.GetParameterBySemantic("PROJECTIONT");
			this._projInvParam = base.Parameters.GetParameterBySemantic("PROJECTIONI");
			this._projInvTrnParam = base.Parameters.GetParameterBySemantic("PROJECTIONIT");
			this._worldViewParam = base.Parameters.GetParameterBySemantic("WORLDVIEW");
			this._worldViewInvParam = base.Parameters.GetParameterBySemantic("WORLDVIEWI");
			this._worldViewInvTrnParam = base.Parameters.GetParameterBySemantic("WORLDVIEWIT");
			this._worldViewProjParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJ");
			if (this._worldViewProjParam == null)
			{
				this._worldViewProjParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJECTION");
			}
			this._worldViewProjInvParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJI");
			if (this._worldViewProjInvParam == null)
			{
				this._worldViewProjInvParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJECTIONI");
			}
			this._worldViewProjInvTrnParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJIT");
			if (this._worldViewProjInvTrnParam == null)
			{
				this._worldViewProjInvTrnParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJECTIONIT");
			}
			this._totalTimeParam = base.Parameters.GetParameterBySemantic("TIMETOTAL");
			if (this._totalTimeParam == null)
			{
				this._totalTimeParam = base.Parameters.GetParameterBySemantic("TIME");
			}
			this._elaspedTimeParam = base.Parameters.GetParameterBySemantic("TIMEELASPED");
			this._diffuseColorParam = base.Parameters.GetParameterBySemantic("DIFFUSECOLOR");
			this._ambientColorParam = base.Parameters.GetParameterBySemantic("AMBIENTCOLOR");
			this._emissiveColorParam = base.Parameters.GetParameterBySemantic("EMISSIVECOLOR");
			this._specularColorParam = base.Parameters.GetParameterBySemantic("SPECULARCOLOR");
			if (this._diffuseColorParam != null)
			{
				this._diffuseColor = this.GetColor(this._diffuseColorParam);
			}
			if (this._ambientColorParam != null)
			{
				this._ambientColor = this.GetColor(this._ambientColorParam);
			}
			if (this._emissiveColorParam != null)
			{
				this._emissiveColor = this.GetColor(this._emissiveColorParam);
			}
			if (this._specularColorParam != null)
			{
				this._specularColor = this.GetColor(this._specularColorParam);
			}
			this._diffuseColorParam = base.Parameters.GetParameterBySemantic("DIFFUSECOLOR");
			this._diffuseMapParam = base.Parameters.GetParameterBySemantic("DIFFUSEMAP");
			this._opacityMapParam = base.Parameters.GetParameterBySemantic("OPACITYMAP");
			this._specularMapParam = base.Parameters.GetParameterBySemantic("SPECULARMAP");
			this._normalMapParam = base.Parameters.GetParameterBySemantic("NORMALMAP");
			if (this._normalMapParam == null)
			{
				this._normalMapParam = base.Parameters.GetParameterBySemantic("BUMPMAP");
			}
			this._displacementMapParam = base.Parameters.GetParameterBySemantic("DISPLACEMENTMAP");
			this._lightMapParam = base.Parameters.GetParameterBySemantic("LIGHTMAP");
			this._reflectionMapParam = base.Parameters.GetParameterBySemantic("REFLECTIONMAP");
			this._diffuseMap = this.GetTexture(this._diffuseMapParam);
			this._opacityMap = this.GetTexture(this._opacityMapParam);
			this._specularMap = this.GetTexture(this._specularMapParam);
			this._normalMap = this.GetTexture(this._normalMapParam);
			this._displacementMap = this.GetTexture(this._displacementMapParam);
			this._lightMap = this.GetTexture(this._lightMapParam);
			this._reflectionMap = this.GetTexture(this._reflectionMapParam);
		}

		private ColorF GetColor(EffectParameter param)
		{
			if (param.ColumnCount == 3)
			{
				return ColorF.FromVector3(param.GetValueVector3());
			}
			if (param.ColumnCount == 4)
			{
				return ColorF.FromVector4(param.GetValueVector4());
			}
			throw new Exception("Bad Color Value:" + param.ColumnCount.ToString());
		}

		private Texture GetTexture(EffectParameter param)
		{
			if (param == null)
			{
				return null;
			}
			switch (param.ParameterType)
			{
			case EffectParameterType.Texture:
				return param.GetValueTexture2D();
			case EffectParameterType.Texture1D:
				return null;
			case EffectParameterType.Texture2D:
				return param.GetValueTexture2D();
			case EffectParameterType.Texture3D:
				return param.GetValueTexture3D();
			case EffectParameterType.TextureCube:
				return param.GetValueTextureCube();
			default:
				return null;
			}
		}

		public override Effect Clone()
		{
			return new DNAEffect(this);
		}

		protected override void OnApply()
		{
			if ((this._alteredParams & DNAEffect.ParamFlags.MatrixFlags) != DNAEffect.ParamFlags.None)
			{
				if (this._worldParam != null)
				{
					this._worldParam.SetValue(this._world);
				}
				if (this._worldInvParam != null || this._worldInvTrnParam != null)
				{
					Matrix worldI;
					Matrix.Invert(ref this._world, out worldI);
					if (this._worldInvParam != null)
					{
						this._worldInvParam.SetValue(worldI);
					}
					if (this._worldInvTrnParam != null)
					{
						this._worldInvTrnParam.SetValue(Matrix.Transpose(worldI));
					}
				}
				if (this._worldTrnParam != null)
				{
					this._worldTrnParam.SetValue(Matrix.Transpose(this._world));
				}
				if (this._viewParam != null)
				{
					this._viewParam.SetValue(this._view);
				}
				if (this._viewInvParam != null || this._viewInvTrnParam != null)
				{
					Matrix viewI;
					Matrix.Invert(ref this._view, out viewI);
					if (this._viewInvParam != null)
					{
						this._viewInvParam.SetValue(viewI);
					}
					if (this._viewInvTrnParam != null)
					{
						this._viewInvTrnParam.SetValue(Matrix.Transpose(viewI));
					}
				}
				if (this._viewTrnParam != null)
				{
					this._viewTrnParam.SetValue(Matrix.Transpose(this._view));
				}
				if (this._projParam != null)
				{
					this._projParam.SetValue(this._proj);
				}
				if (this._projInvParam != null || this._projInvTrnParam != null)
				{
					Matrix projI;
					Matrix.Invert(ref this._proj, out projI);
					if (this._projInvParam != null)
					{
						this._projInvParam.SetValue(projI);
					}
					if (this._projInvTrnParam != null)
					{
						this._projInvTrnParam.SetValue(Matrix.Transpose(projI));
					}
				}
				if (this._projTrnParam != null)
				{
					this._projTrnParam.SetValue(Matrix.Transpose(this._proj));
				}
				if (this._worldViewParam != null || this._worldViewInvParam != null || this._worldViewInvTrnParam != null || this._worldViewProjParam != null || this._worldViewProjInvParam != null || this._worldViewProjInvTrnParam != null)
				{
					Matrix worldView;
					Matrix.Multiply(ref this._world, ref this._view, out worldView);
					if (this._worldViewParam != null)
					{
						this._worldViewParam.SetValue(worldView);
					}
					if (this._worldViewInvParam != null || this._worldViewInvTrnParam != null)
					{
						Matrix worldViewI = Matrix.Invert(worldView);
						if (this._worldViewInvParam != null)
						{
							this._worldViewInvParam.SetValue(worldViewI);
						}
						if (this._worldViewInvTrnParam != null)
						{
							this._worldViewInvTrnParam.SetValue(Matrix.Transpose(worldViewI));
						}
					}
					if (this._worldViewProjParam != null || this._worldViewProjInvParam != null || this._worldViewProjInvTrnParam != null)
					{
						Matrix worldViewProj;
						Matrix.Multiply(ref worldView, ref this._proj, out worldViewProj);
						if (this._worldViewProjParam != null)
						{
							this._worldViewProjParam.SetValue(worldViewProj);
						}
						if (this._worldViewProjInvParam != null || this._worldViewProjInvTrnParam != null)
						{
							Matrix worldViewProjI = Matrix.Invert(worldViewProj);
							if (this._worldViewProjInvParam != null)
							{
								this._worldViewProjInvParam.SetValue(worldViewProjI);
							}
							if (this._worldViewProjInvTrnParam != null)
							{
								this._worldViewProjInvTrnParam.SetValue(Matrix.Transpose(worldViewProjI));
							}
						}
					}
				}
			}
			if (this._totalTimeParam != null)
			{
				this._totalTimeParam.SetValue((float)this._totalTime.TotalSeconds);
			}
			if (this._elaspedTimeParam != null)
			{
				this._elaspedTimeParam.SetValue((float)this._elaspedTime.TotalSeconds);
			}
			if ((this._alteredParams & DNAEffect.ParamFlags.ColorFlags) != DNAEffect.ParamFlags.None)
			{
				if ((this._alteredParams & DNAEffect.ParamFlags.Diffuse) != DNAEffect.ParamFlags.None)
				{
					this.SetColor(this._diffuseColorParam, this._diffuseColor);
				}
				if ((this._alteredParams & DNAEffect.ParamFlags.Ambient) != DNAEffect.ParamFlags.None)
				{
					this.SetColor(this._ambientColorParam, this._ambientColor);
				}
				if ((this._alteredParams & DNAEffect.ParamFlags.Specular) != DNAEffect.ParamFlags.None)
				{
					this.SetColor(this._specularColorParam, this._specularColor);
				}
				if ((this._alteredParams & DNAEffect.ParamFlags.Emissive) != DNAEffect.ParamFlags.None)
				{
					this.SetColor(this._emissiveColorParam, this._emissiveColor);
				}
			}
			if ((this._alteredParams & DNAEffect.ParamFlags.MapFlags) != DNAEffect.ParamFlags.None)
			{
				if (this._diffuseMapParam != null && this._diffuseMap != null)
				{
					this._diffuseMapParam.SetValue(this._diffuseMap);
				}
				if (this._opacityMapParam != null && this._opacityMap != null)
				{
					this._opacityMapParam.SetValue(this._opacityMap);
				}
				if (this._specularMapParam != null && this._specularMap != null)
				{
					this._specularMapParam.SetValue(this._specularMap);
				}
				if (this._normalMapParam != null && this._normalMap != null)
				{
					this._normalMapParam.SetValue(this._normalMap);
				}
				if (this._displacementMapParam != null && this._displacementMap != null)
				{
					this._displacementMapParam.SetValue(this._displacementMap);
				}
				if (this._lightMapParam != null && this._lightMap != null)
				{
					this._lightMapParam.SetValue(this._lightMap);
				}
				if (this._reflectionMapParam != null && this._reflectionMap != null)
				{
					this._reflectionMapParam.SetValue(this._reflectionMap);
				}
			}
			base.OnApply();
			this._alteredParams = DNAEffect.ParamFlags.None;
		}

		private void SetColor(EffectParameter param, ColorF color)
		{
			if (param == null)
			{
				return;
			}
			if (param.ColumnCount == 3)
			{
				param.SetValue(color.ToVector3());
				return;
			}
			if (param.ColumnCount == 4)
			{
				param.SetValue(color.ToVector4());
				return;
			}
			throw new Exception("Bad Color Value:" + param.Name);
		}

		private DNAEffect.ParamFlags _alteredParams;

		private EffectParameter _worldParam;

		private EffectParameter _worldInvParam;

		private EffectParameter _worldInvTrnParam;

		private EffectParameter _worldTrnParam;

		private EffectParameter _viewParam;

		private EffectParameter _viewTrnParam;

		private EffectParameter _viewInvParam;

		private EffectParameter _viewInvTrnParam;

		private EffectParameter _projParam;

		private EffectParameter _projTrnParam;

		private EffectParameter _projInvParam;

		private EffectParameter _projInvTrnParam;

		private EffectParameter _worldViewParam;

		private EffectParameter _worldViewInvParam;

		private EffectParameter _worldViewInvTrnParam;

		private EffectParameter _worldViewProjParam;

		private EffectParameter _worldViewProjInvParam;

		private EffectParameter _worldViewProjInvTrnParam;

		private EffectParameter _totalTimeParam;

		private EffectParameter _elaspedTimeParam;

		private EffectParameter _diffuseColorParam;

		private EffectParameter _ambientColorParam;

		private EffectParameter _emissiveColorParam;

		private EffectParameter _specularColorParam;

		private EffectParameter _diffuseMapParam;

		private EffectParameter _opacityMapParam;

		private EffectParameter _specularMapParam;

		private EffectParameter _normalMapParam;

		private EffectParameter _displacementMapParam;

		private EffectParameter _lightMapParam;

		private EffectParameter _reflectionMapParam;

		private Matrix _world;

		private Matrix _view;

		private Matrix _proj;

		private TimeSpan _elaspedTime;

		private TimeSpan _totalTime;

		private ColorF _diffuseColor = Color.White;

		private ColorF _ambientColor = Color.Gray;

		private ColorF _specularColor = Color.White;

		private ColorF _emissiveColor;

		private Texture _diffuseMap;

		private Texture _opacityMap;

		private Texture _specularMap;

		private Texture _normalMap;

		private Texture _displacementMap;

		private Texture _lightMap;

		private Texture _reflectionMap;

		public enum EffectValueTypes : byte
		{
			intValue,
			stringValue,
			boolValue,
			floatValue,
			Vector2Value,
			Vector3Value,
			Vector4Value,
			MatrixValue
		}

		public class Reader : ContentTypeReader<DNAEffect>
		{
			protected override DNAEffect Read(ContentReader input, DNAEffect existingInstance)
			{
				Effect baseEffect = input.ReadExternalReference<Effect>();
				int textureCount = input.ReadInt32();
				for (int i = 0; i < textureCount; i++)
				{
					string textureName = input.ReadString();
					Texture texture = input.ReadExternalReference<Texture>();
					baseEffect.Parameters[textureName].SetValue(texture);
				}
				int valueCount = input.ReadInt32();
				for (int j = 0; j < valueCount; j++)
				{
					string valueName = input.ReadString();
					EffectParameter param = baseEffect.Parameters[valueName];
					DNAEffect.EffectValueTypes valueType = (DNAEffect.EffectValueTypes)input.ReadByte();
					int vcount = input.ReadInt32();
					if (param == null)
					{
						if (vcount != 0)
						{
							switch (valueType)
							{
							case DNAEffect.EffectValueTypes.intValue:
							{
								int[] array = new int[vcount];
								for (int k = 0; k < array.Length; k++)
								{
									array[k] = input.ReadInt32();
								}
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.boolValue:
							{
								bool[] array2 = new bool[vcount];
								for (int l = 0; l < array2.Length; l++)
								{
									array2[l] = input.ReadBoolean();
								}
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.floatValue:
							{
								float[] array3 = new float[vcount];
								for (int m = 0; m < array3.Length; m++)
								{
									array3[m] = input.ReadSingle();
								}
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.Vector2Value:
							{
								Vector2[] array4 = new Vector2[vcount];
								for (int n = 0; n < array4.Length; n++)
								{
									array4[n] = input.ReadVector2();
								}
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.Vector3Value:
							{
								Vector3[] array5 = new Vector3[vcount];
								for (int j2 = 0; j2 < array5.Length; j2++)
								{
									array5[j2] = input.ReadVector3();
								}
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.Vector4Value:
							{
								Vector4[] array6 = new Vector4[vcount];
								for (int j3 = 0; j3 < array6.Length; j3++)
								{
									array6[j3] = input.ReadVector4();
								}
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.MatrixValue:
							{
								Matrix[] array7 = new Matrix[vcount];
								for (int j4 = 0; j4 < array7.Length; j4++)
								{
									array7[j4] = input.ReadMatrix();
								}
								goto IL_05B2;
							}
							}
							throw new Exception("Unsupported Value Type");
						}
						switch (valueType)
						{
						case DNAEffect.EffectValueTypes.intValue:
							input.ReadInt32();
							break;
						case DNAEffect.EffectValueTypes.stringValue:
							input.ReadString();
							break;
						case DNAEffect.EffectValueTypes.boolValue:
							input.ReadBoolean();
							break;
						case DNAEffect.EffectValueTypes.floatValue:
							input.ReadSingle();
							break;
						case DNAEffect.EffectValueTypes.Vector2Value:
							input.ReadVector2();
							break;
						case DNAEffect.EffectValueTypes.Vector3Value:
							input.ReadVector3();
							break;
						case DNAEffect.EffectValueTypes.Vector4Value:
							input.ReadVector4();
							break;
						case DNAEffect.EffectValueTypes.MatrixValue:
							input.ReadMatrix();
							break;
						default:
							throw new Exception("Unsupported Value Type");
						}
					}
					else
					{
						if (vcount != 0)
						{
							switch (valueType)
							{
							case DNAEffect.EffectValueTypes.intValue:
							{
								int[] array8 = new int[vcount];
								for (int j5 = 0; j5 < array8.Length; j5++)
								{
									array8[j5] = input.ReadInt32();
								}
								param.SetValue(array8);
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.boolValue:
							{
								bool[] array9 = new bool[vcount];
								for (int j6 = 0; j6 < array9.Length; j6++)
								{
									array9[j6] = input.ReadBoolean();
								}
								param.SetValue(array9);
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.floatValue:
							{
								float[] array10 = new float[vcount];
								for (int j7 = 0; j7 < array10.Length; j7++)
								{
									array10[j7] = input.ReadSingle();
								}
								param.SetValue(array10);
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.Vector2Value:
							{
								Vector2[] array11 = new Vector2[vcount];
								for (int j8 = 0; j8 < array11.Length; j8++)
								{
									array11[j8] = input.ReadVector2();
								}
								param.SetValue(array11);
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.Vector3Value:
							{
								Vector3[] array12 = new Vector3[vcount];
								for (int j9 = 0; j9 < array12.Length; j9++)
								{
									array12[j9] = input.ReadVector3();
								}
								param.SetValue(array12);
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.Vector4Value:
							{
								Vector4[] array13 = new Vector4[vcount];
								for (int j10 = 0; j10 < array13.Length; j10++)
								{
									array13[j10] = input.ReadVector4();
								}
								param.SetValue(array13);
								goto IL_05B2;
							}
							case DNAEffect.EffectValueTypes.MatrixValue:
							{
								Matrix[] array14 = new Matrix[vcount];
								for (int j11 = 0; j11 < array14.Length; j11++)
								{
									array14[j11] = input.ReadMatrix();
								}
								param.SetValue(array14);
								goto IL_05B2;
							}
							}
							throw new Exception("Unsupported Value Type");
						}
						switch (valueType)
						{
						case DNAEffect.EffectValueTypes.intValue:
							param.SetValue(input.ReadInt32());
							break;
						case DNAEffect.EffectValueTypes.stringValue:
							param.SetValue(input.ReadString());
							break;
						case DNAEffect.EffectValueTypes.boolValue:
							param.SetValue(input.ReadBoolean());
							break;
						case DNAEffect.EffectValueTypes.floatValue:
							param.SetValue(input.ReadSingle());
							break;
						case DNAEffect.EffectValueTypes.Vector2Value:
							param.SetValue(input.ReadVector2());
							break;
						case DNAEffect.EffectValueTypes.Vector3Value:
							param.SetValue(input.ReadVector3());
							break;
						case DNAEffect.EffectValueTypes.Vector4Value:
						{
							Vector4 vec = input.ReadVector4();
							if (param.ColumnCount == 2)
							{
								param.SetValue(new Vector2(vec.X, vec.Y));
							}
							else if (param.ColumnCount == 3)
							{
								param.SetValue(new Vector3(vec.X, vec.Y, vec.Z));
							}
							else
							{
								param.SetValue(vec);
							}
							break;
						}
						case DNAEffect.EffectValueTypes.MatrixValue:
							baseEffect.Parameters[valueName].SetValue(input.ReadMatrix());
							break;
						default:
							throw new Exception("Unsupported Value Type");
						}
					}
					IL_05B2:;
				}
				return new DNAEffect(baseEffect);
			}
		}

		[Flags]
		private enum ParamFlags : uint
		{
			None = 0U,
			World = 1U,
			View = 2U,
			Projection = 4U,
			Time = 8U,
			ElaspedTime = 16U,
			Diffuse = 32U,
			Ambient = 64U,
			Emissive = 128U,
			Specular = 256U,
			DiffuseMap = 512U,
			OpacityMap = 1024U,
			SpecularMap = 2048U,
			NormalMap = 4096U,
			DisplacementMap = 8192U,
			LightMap = 16384U,
			ReflectionMap = 32768U,
			MatrixFlags = 7U,
			TimeFlags = 24U,
			ColorFlags = 480U,
			MapFlags = 65024U,
			AllFlags = 4294967295U
		}
	}
}
