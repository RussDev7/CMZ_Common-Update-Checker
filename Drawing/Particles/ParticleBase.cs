using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Particles
{
	public class ParticleBase<T>
	{
		public string TexturePath
		{
			get
			{
				return this._texturePath;
			}
			set
			{
				this._texturePath = value;
			}
		}

		public T Texture
		{
			get
			{
				return this._texture;
			}
			set
			{
				this._texture = value;
			}
		}

		public int NumTilesWide
		{
			get
			{
				return this._numTilesWide;
			}
			set
			{
				this._numTilesWide = value;
			}
		}

		public int NumTilesHigh
		{
			get
			{
				return this._numTilesHigh;
			}
			set
			{
				this._numTilesHigh = value;
			}
		}

		public Vector2 TileSize
		{
			get
			{
				return new Vector2(1f / (float)this._numTilesWide, 1f / (float)this._numTilesHigh);
			}
		}

		public int FirstTileToInclude
		{
			get
			{
				return this._firstTileToInclude;
			}
			set
			{
				this._firstTileToInclude = value;
			}
		}

		public int LastTileToInclude
		{
			get
			{
				return this._lastTileToInclude;
			}
			set
			{
				this._lastTileToInclude = value;
			}
		}

		public ParticleBlendMode BlendMode
		{
			get
			{
				return this._blendMode;
			}
			set
			{
				this._blendMode = value;
			}
		}

		public ParticleTechnique Technique
		{
			get
			{
				return this._technique;
			}
			set
			{
				this._technique = value;
			}
		}

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

		public float DistortionAmplitude
		{
			get
			{
				return this._distortionAmplitude;
			}
			set
			{
				this._distortionAmplitude = value;
			}
		}

		public bool RandomizeRotations
		{
			get
			{
				return this._randomizeRotations;
			}
			set
			{
				this._randomizeRotations = value;
			}
		}

		public float ParticlesPerSecond
		{
			get
			{
				return this._particlesPerSecond;
			}
			set
			{
				this._particlesPerSecond = value;
			}
		}

		public bool LocalSpace
		{
			get
			{
				return this._localSpace;
			}
			set
			{
				this._localSpace = value;
			}
		}

		public bool FadeOut
		{
			get
			{
				return this._fadeOut;
			}
			set
			{
				this._fadeOut = value;
			}
		}

		public TimeSpan EmmissionTime
		{
			get
			{
				return this._emmissionTime;
			}
			set
			{
				this._emmissionTime = value;
			}
		}

		public TimeSpan ParticleLifeTime
		{
			get
			{
				return this._particleLifeTime;
			}
			set
			{
				this._particleLifeTime = value;
			}
		}

		public float LifetimeVariation
		{
			get
			{
				return this._lifetimeVariation;
			}
			set
			{
				this._lifetimeVariation = value;
			}
		}

		public float EmitterVelocitySensitivity
		{
			get
			{
				return this._emitterVelocitySensitivity;
			}
			set
			{
				this._emitterVelocitySensitivity = value;
			}
		}

		public float HorizontalVelocityMin
		{
			get
			{
				return this._minHorizontalVelocity;
			}
			set
			{
				this._minHorizontalVelocity = value;
			}
		}

		public float HorizontalVelocityMax
		{
			get
			{
				return this._maxHorizontalVelocity;
			}
			set
			{
				this._maxHorizontalVelocity = value;
			}
		}

		public float VerticalVelocityMin
		{
			get
			{
				return this._minVerticalVelocity;
			}
			set
			{
				this._minVerticalVelocity = value;
			}
		}

		public float VerticalVelocityMax
		{
			get
			{
				return this._maxVerticalVelocity;
			}
			set
			{
				this._maxVerticalVelocity = value;
			}
		}

		public Vector3 Gravity
		{
			get
			{
				return this._gravity;
			}
			set
			{
				this._gravity = value;
			}
		}

		public float VelocityEnd
		{
			get
			{
				return this._endVelocity;
			}
			set
			{
				this._endVelocity = value;
			}
		}

		public Color ColorMin
		{
			get
			{
				return this._minColor;
			}
			set
			{
				this._minColor = value;
			}
		}

		public Color ColorMax
		{
			get
			{
				return this._maxColor;
			}
			set
			{
				this._maxColor = value;
			}
		}

		public float RotateSpeedMin
		{
			get
			{
				return this._minRotateSpeed;
			}
			set
			{
				this._minRotateSpeed = value;
			}
		}

		public float RotateSpeedMax
		{
			get
			{
				return this._maxRotateSpeed;
			}
			set
			{
				this._maxRotateSpeed = value;
			}
		}

		public float StartSizeMin
		{
			get
			{
				return this._minStartSize;
			}
			set
			{
				this._minStartSize = value;
			}
		}

		public float StartSizeMax
		{
			get
			{
				return this._maxStartSize;
			}
			set
			{
				this._maxStartSize = value;
			}
		}

		public float EndSizeMin
		{
			get
			{
				return this._minEndSize;
			}
			set
			{
				this._minEndSize = value;
			}
		}

		public float EndSizeMax
		{
			get
			{
				return this._maxEndSize;
			}
			set
			{
				this._maxEndSize = value;
			}
		}

		public bool DieAfterEmmision
		{
			get
			{
				return this._dieAfterEmmision;
			}
			set
			{
				this._dieAfterEmmision = value;
			}
		}

		private string _texturePath;

		[NonSerialized]
		private T _texture = default(T);

		private int _numTilesWide = 1;

		private int _numTilesHigh = 1;

		private int _firstTileToInclude;

		private int _lastTileToInclude;

		private ParticleBlendMode _blendMode = ParticleBlendMode.NonPreMult;

		private ParticleTechnique _technique;

		private float _distortionScale = 2f;

		private float _distortionAmplitude = 0.2f;

		private bool _randomizeRotations = true;

		private float _particlesPerSecond = 100f;

		private bool _localSpace;

		private bool _fadeOut = true;

		private TimeSpan _emmissionTime = TimeSpan.FromSeconds(0.0);

		private TimeSpan _particleLifeTime = TimeSpan.FromSeconds(1.0);

		private float _lifetimeVariation;

		private float _emitterVelocitySensitivity = 1f;

		private float _minHorizontalVelocity;

		private float _maxHorizontalVelocity;

		private float _minVerticalVelocity;

		private float _maxVerticalVelocity;

		private Vector3 _gravity = Vector3.Zero;

		private float _endVelocity = 1f;

		private Color _minColor = Color.White;

		private Color _maxColor = Color.White;

		private float _minRotateSpeed;

		private float _maxRotateSpeed;

		private float _minStartSize = 100f;

		private float _maxStartSize = 100f;

		private float _minEndSize = 100f;

		private float _maxEndSize = 100f;

		private bool _dieAfterEmmision = true;
	}
}
