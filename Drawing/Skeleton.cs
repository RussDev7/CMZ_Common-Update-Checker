using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using DNA.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace DNA.Drawing
{
	public class Skeleton : ReadOnlyCollection<Bone>
	{
		public void Reset()
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i].Reset();
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(base.Count);
			for (int i = 0; i < base.Count; i++)
			{
				Bone b = base[i];
				writer.Write(b.Name);
				writer.Write((b.Parent == null) ? (-1) : b.Parent.Index);
				writer.Write(b.Transform);
			}
		}

		public static Skeleton Load(BinaryReader reader)
		{
			int boneCount = reader.ReadInt32();
			string[] boneNames = new string[boneCount];
			int[] heirachy = new int[boneCount];
			Matrix[] xforms = new Matrix[boneCount];
			for (int i = 0; i < boneCount; i++)
			{
				boneNames[i] = reader.ReadString();
				heirachy[i] = reader.ReadInt32();
				xforms[i] = reader.ReadMatrix();
			}
			return Bone.BuildSkeleton(xforms, heirachy, boneNames);
		}

		public Skeleton(IList<Bone> bones)
			: base(bones)
		{
			this._bones = new Bone[bones.Count];
			bones.CopyTo(this._bones, 0);
			for (int i = 0; i < bones.Count; i++)
			{
				if (bones[i].Name != null)
				{
					this.boneLookup[bones[i].Name] = bones[i];
				}
			}
		}

		public Bone this[string boneName]
		{
			get
			{
				return this.boneLookup[boneName];
			}
		}

		public int IndexOf(string boneName)
		{
			return this.boneLookup[boneName].Index;
		}

		public IList<Bone> BonesFromNames(IList<string> boneNames)
		{
			Bone[] bones = new Bone[boneNames.Count];
			for (int i = 0; i < boneNames.Count; i++)
			{
				bones[i] = this.boneLookup[boneNames[i]];
			}
			return bones;
		}

		public void CopyTransformsFrom(Matrix[] sourceBoneTransforms)
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i].SetTransform(sourceBoneTransforms[i]);
			}
		}

		public void CopyTransformsTo(Matrix[] destinationBoneTransforms)
		{
			int count = this._bones.Length;
			for (int i = 0; i < count; i++)
			{
				this._bones[i].GetTransform(out destinationBoneTransforms[i]);
			}
		}

		public void CopyAbsoluteBoneTransformsTo(Matrix[] worldBoneTransforms, Matrix localToWorld)
		{
			for (int i = 0; i < base.Count; i++)
			{
				Bone bone = base[i];
				if (bone.Parent == null)
				{
					this.CopyAbsoluteBoneTransformsTo(bone, worldBoneTransforms, ref localToWorld);
				}
			}
		}

		private void CopyAbsoluteBoneTransformsTo(Bone bone, Matrix[] worldBoneTransforms, ref Matrix localToWorld)
		{
			bone.EnsureTransformComposed();
			if (bone.Parent == null)
			{
				Matrix.Multiply(ref bone._transform, ref localToWorld, out worldBoneTransforms[bone.Index]);
			}
			else
			{
				Matrix.Multiply(ref bone._transform, ref worldBoneTransforms[bone.Parent.Index], out worldBoneTransforms[bone.Index]);
			}
			ReadOnlyCollection<Bone> children = bone.Children;
			int count = bone.Children.Count;
			for (int i = 0; i < count; i++)
			{
				this.CopyAbsoluteBoneTransformsTo(bone.Children[i], worldBoneTransforms, ref localToWorld);
			}
		}

		public Dictionary<string, Bone> boneLookup = new Dictionary<string, Bone>();

		private Bone[] _bones;

		public class Reader : ContentTypeReader<Skeleton>
		{
			protected override Skeleton Read(ContentReader input, Skeleton existingInstance)
			{
				if (existingInstance != null)
				{
					throw new NotImplementedException();
				}
				return Skeleton.Load(input);
			}
		}
	}
}
