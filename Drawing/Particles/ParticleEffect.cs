using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.Particles
{
	public class ParticleEffect : ParticleBase<Texture2D>
	{
		public void Flush()
		{
			for (int i = 0; i < this._particleCores.Count; i++)
			{
				this._particleCores[i].ParticleEmitter.ReleaseCore();
			}
			this._particleCores.Clear();
		}

		internal ParticleEmitter.ParticleEmitterCore CreateParticleCore(ParticleEmitter emitter)
		{
			for (int i = 0; i < this._particleCores.Count; i++)
			{
				if (!this._particleCores[i].HasActiveParticles || this._particleCores[i].ParticleEmitter.Scene == null)
				{
					this._particleCores[i].ParticleEmitter.ReleaseCore();
					emitter.SetCore(this._particleCores[i]);
					return this._particleCores[i];
				}
			}
			ParticleEmitter.ParticleEmitterCore particleEmitterCore = new ParticleEmitter.ParticleEmitterCore(emitter);
			this._particleCores.Add(particleEmitterCore);
			return particleEmitterCore;
		}

		public ParticleEmitter CreateEmitter(DNAGame game)
		{
			return ParticleEmitter.Create(game, this, null);
		}

		public ParticleEmitter CreateEmitter(DNAGame game, Texture2D reflectionMap)
		{
			return ParticleEmitter.Create(game, this, reflectionMap);
		}

		[NonSerialized]
		private List<ParticleEmitter.ParticleEmitterCore> _particleCores = new List<ParticleEmitter.ParticleEmitterCore>();
	}
}
