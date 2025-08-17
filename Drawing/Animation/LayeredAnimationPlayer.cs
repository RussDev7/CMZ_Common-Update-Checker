using System;
using System.Collections.Generic;

namespace DNA.Drawing.Animation
{
	public class LayeredAnimationPlayer : BaseAnimationPlayer
	{
		public AnimationPlayer this[int index]
		{
			get
			{
				return this._blenders[index].ActiveAnimation;
			}
		}

		public AnimationPlayer GetAnimation(int channel)
		{
			return this._blenders[channel].ActiveAnimation;
		}

		public void PlayAnimation(int channel, AnimationPlayer player, TimeSpan blendTime)
		{
			this._blenders[channel].Play(player, blendTime);
		}

		public void ClearAnimation(int channel, TimeSpan blendTime)
		{
			this.PlayAnimation(channel, null, blendTime);
		}

		public LayeredAnimationPlayer(int channels)
		{
			this._blenders = new AnimBlender[channels];
			for (int i = 0; i < this._blenders.Length; i++)
			{
				this._blenders[i] = new AnimBlender();
			}
		}

		public override void Update(TimeSpan timeSpan, IList<Bone> boneTransforms)
		{
			for (int i = 0; i < this._blenders.Length; i++)
			{
				this._blenders[i].Update(timeSpan, boneTransforms);
			}
		}

		private AnimBlender[] _blenders;
	}
}
