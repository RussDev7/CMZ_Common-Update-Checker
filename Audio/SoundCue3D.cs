using System;
using Microsoft.Xna.Framework.Audio;

namespace DNA.Audio
{
	public class SoundCue3D
	{
		public AudioEmitter AudioEmitter
		{
			get
			{
				return this._emitter;
			}
		}

		public Cue Cue
		{
			get
			{
				return this._cue;
			}
		}

		public void Set(Cue cue, AudioEmitter emitter)
		{
			this._cue = cue;
			this._emitter = emitter;
		}

		public SoundCue3D(Cue cue, AudioEmitter emitter)
		{
			this._cue = cue;
			this._emitter = emitter;
		}

		public bool IsCreated
		{
			get
			{
				return this.Cue.IsCreated;
			}
		}

		public bool IsDisposed
		{
			get
			{
				return this.Cue.IsDisposed;
			}
		}

		public bool IsPaused
		{
			get
			{
				return this.Cue.IsPaused;
			}
		}

		public bool IsPlaying
		{
			get
			{
				return this.Cue.IsPlaying;
			}
		}

		public bool IsPrepared
		{
			get
			{
				return this.Cue.IsPrepared;
			}
		}

		public bool IsPreparing
		{
			get
			{
				return this.Cue.IsPreparing;
			}
		}

		public bool IsStopped
		{
			get
			{
				return this.Cue.IsStopped;
			}
		}

		public bool IsStopping
		{
			get
			{
				return this.Cue.IsStopping;
			}
		}

		public string Name
		{
			get
			{
				return this.Cue.Name;
			}
		}

		public void Dispose()
		{
			this.Cue.Dispose();
		}

		public float GetVariable(string name)
		{
			return this.Cue.GetVariable(name);
		}

		public void Pause()
		{
			this.Cue.Pause();
		}

		public void Play()
		{
			this.Cue.Play();
		}

		public void Resume()
		{
			this.Cue.Resume();
		}

		public void SetVariable(string name, float value)
		{
			this.Cue.SetVariable(name, value);
		}

		public void Stop(AudioStopOptions options)
		{
			this.Cue.Stop(options);
		}

		public void Apply3D(AudioListener listener)
		{
			this._cue.Apply3D(listener, this._emitter);
		}

		private AudioEmitter _emitter;

		private Cue _cue;
	}
}
