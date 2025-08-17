using System;
using System.Collections.Generic;

namespace DNA.Drawing.Animation
{
	public abstract class BaseAnimationPlayer
	{
		public virtual string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		public float Speed
		{
			get
			{
				return this._speed;
			}
			set
			{
				this._speed = value;
			}
		}

		public bool Playing
		{
			get
			{
				return this._playing;
			}
		}

		public void Play()
		{
			this._playing = true;
		}

		public void Pause()
		{
			this._playing = false;
		}

		public void Stop()
		{
			this._playing = false;
			this.Reset();
		}

		protected virtual void Reset()
		{
			this._speed = 1f;
		}

		public abstract void Update(TimeSpan timeSpan, IList<Bone> boneTransforms);

		private string _name;

		private float _speed = 1f;

		private bool _playing = true;
	}
}
