using System;
using System.Collections.Generic;
using DNA.Drawing.Animation;
using DNA.Net.GamerServices;

namespace DNA.Avatars
{
	public class AvatarAnimationManager
	{
		public void RegisterAnimation(string name, AnimationClip clip, bool looping, IList<AvatarBone> bones, IList<AvatarBone> maskedBones)
		{
			this._clips[name] = new AvatarAnimationManager.AvatarAnimationSet(clip, null, Avatar.GetInfluncedBoneList(bones, maskedBones), looping);
		}

		public void RegisterAnimation(string name, AnimationClip maleClip, AnimationClip femaleClip, bool looping, IList<AvatarBone> bones, IList<AvatarBone> maskedBones)
		{
			this._clips[name] = new AvatarAnimationManager.AvatarAnimationSet(maleClip, femaleClip, Avatar.GetInfluncedBoneList(bones, maskedBones), looping);
		}

		public void RegisterAnimation(string name, AnimationClip clip, bool looping, IList<AvatarBone> bones)
		{
			this._clips[name] = new AvatarAnimationManager.AvatarAnimationSet(clip, null, Avatar.GetInfluncedBoneList(bones), looping);
		}

		public void RegisterAnimation(string name, AnimationClip maleClip, AnimationClip femaleClip, bool looping, IList<AvatarBone> bones)
		{
			this._clips[name] = new AvatarAnimationManager.AvatarAnimationSet(maleClip, femaleClip, Avatar.GetInfluncedBoneList(bones), looping);
		}

		public void RegisterAnimation(string name, AnimationClip clip, bool looping)
		{
			this._clips[name] = new AvatarAnimationManager.AvatarAnimationSet(clip, null, null, looping);
		}

		public void RegisterAnimation(string name, AnimationClip maleClip, AnimationClip femaleClip, bool looping)
		{
			this._clips[name] = new AvatarAnimationManager.AvatarAnimationSet(maleClip, femaleClip, null, looping);
		}

		public AnimationPlayer GetAnimation(string name, bool male)
		{
			AvatarAnimationManager.AvatarAnimationSet avatarAnimationSet = this._clips[name];
			AnimationPlayer animationPlayer = new AnimationPlayer(avatarAnimationSet.GetClip(male));
			animationPlayer.Name = name;
			animationPlayer.Looping = avatarAnimationSet.Looping;
			animationPlayer.SetInfluncedBones(avatarAnimationSet.InfluencedBones);
			return animationPlayer;
		}

		public void GetAnimation(AnimationPlayer player, string name, bool male)
		{
			AvatarAnimationManager.AvatarAnimationSet avatarAnimationSet = this._clips[name];
			player.SetClip(avatarAnimationSet.GetClip(male));
			player.Name = name;
			player.Looping = avatarAnimationSet.Looping;
			player.SetInfluncedBones(avatarAnimationSet.InfluencedBones);
		}

		private AvatarAnimationManager()
		{
		}

		public static AvatarAnimationManager Instance = new AvatarAnimationManager();

		private Dictionary<string, AvatarAnimationManager.AvatarAnimationSet> _clips = new Dictionary<string, AvatarAnimationManager.AvatarAnimationSet>();

		public static readonly AnimationClip DefaultAnimationClip;

		private class AvatarAnimationSet
		{
			public AnimationClip GetClip(bool male)
			{
				if (male)
				{
					if (this.Male == null)
					{
						return this.Female;
					}
					return this.Male;
				}
				else
				{
					if (this.Female == null)
					{
						return this.Male;
					}
					return this.Female;
				}
			}

			public AvatarAnimationSet(AnimationClip male, AnimationClip female, bool[] influencedBones, bool looping)
			{
				this.Male = male;
				this.Female = female;
				this.InfluencedBones = influencedBones;
				this.Looping = looping;
			}

			public AnimationClip Male;

			public AnimationClip Female;

			public bool[] InfluencedBones;

			public bool Looping;
		}
	}
}
