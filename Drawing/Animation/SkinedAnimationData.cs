using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace DNA.Drawing.Animation
{
	public class SkinedAnimationData : AnimationData
	{
		public SkinedAnimationData(Dictionary<string, AnimationClip> animationClips, List<Matrix> inverseBindPose, Skeleton skeleton)
			: base(animationClips)
		{
			this.InverseBindPose = inverseBindPose.ToArray();
			this.Skeleton = skeleton;
		}

		private SkinedAnimationData()
		{
		}

		[ContentSerializer]
		public Matrix[] InverseBindPose { get; private set; }

		[ContentSerializer]
		public Skeleton Skeleton { get; private set; }
	}
}
