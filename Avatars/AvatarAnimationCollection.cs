using System;
using DNA.Drawing.Animation;

namespace DNA.Avatars
{
	public class AvatarAnimationCollection : LayeredAnimationPlayer
	{
		public AvatarAnimationCollection(Avatar avatar)
			: base(16)
		{
			this._avatar = avatar;
			this.players = new AnimationPlayer[16, 3];
			this.currentPlayers = new int[16];
		}

		public AnimationPlayer Play(string id, int channel, TimeSpan blendTime)
		{
			int num = this.currentPlayers[channel];
			AnimationPlayer animationPlayer = this.players[channel, num];
			if (animationPlayer == null)
			{
				animationPlayer = (this.players[channel, num] = AvatarAnimationManager.Instance.GetAnimation(id, this._avatar.IsMale));
			}
			else
			{
				AvatarAnimationManager.Instance.GetAnimation(animationPlayer, id, this._avatar.IsMale);
			}
			this.currentPlayers[channel] = (this.currentPlayers[channel] + 1) % 3;
			base.PlayAnimation(channel, animationPlayer, blendTime);
			return animationPlayer;
		}

		private Avatar _avatar;

		private AnimationPlayer[,] players;

		private int[] currentPlayers;
	}
}
