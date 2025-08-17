using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DNA.Avatars;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class Bone
	{
		[ContentSerializer]
		public ReadOnlyCollection<Bone> Children { get; private set; }

		[ContentSerializer]
		public int Index { get; private set; }

		[ContentSerializer]
		public string Name { get; private set; }

		[ContentSerializer]
		public Bone Parent { get; private set; }

		public Vector3 Translation
		{
			get
			{
				return this._translation;
			}
			set
			{
				if (this._dirty || this._translation != value)
				{
					this._translation = value;
					this._dirty = true;
				}
			}
		}

		public Vector3 Scale
		{
			get
			{
				return this._scale;
			}
			set
			{
				if (this._dirty || this._scale != value)
				{
					this._scale = value;
					this._unitScale = this._scale == Vector3.One;
					this._dirty = true;
				}
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return this._rotation;
			}
			set
			{
				if (this._dirty || this._rotation != value)
				{
					this._rotation = value;
					this._dirty = true;
				}
			}
		}

		public void SetTransform(ref Vector3 trans, ref Quaternion rot, ref Vector3 scale)
		{
			this._translation = trans;
			this._rotation = rot;
			this._scale = scale;
			this._unitScale = this._scale == Vector3.One;
			this._dirty = true;
		}

		internal void EnsureTransformComposed()
		{
			if (this._dirty)
			{
				this.ComposeTransform();
			}
		}

		public Matrix Transform
		{
			get
			{
				if (this._dirty)
				{
					this.ComposeTransform();
				}
				return this._transform;
			}
		}

		public void GetTransform(out Matrix tx)
		{
			if (this._dirty)
			{
				this.ComposeTransform();
			}
			tx = this._transform;
		}

		public void Reset()
		{
			this._rotation = Quaternion.Identity;
			this._translation = Vector3.Zero;
			this._scale = Vector3.One;
			this._transform = Matrix.Identity;
			this._dirty = false;
		}

		private void ComposeTransform()
		{
			if (this._unitScale)
			{
				Matrix.CreateFromQuaternion(ref this._rotation, out this._transform);
			}
			else
			{
				Matrix.CreateScale(ref this._scale, out this._transform);
				this._transform *= Matrix.CreateFromQuaternion(this._rotation);
			}
			this._transform.Translation = this._translation;
			this._dirty = false;
		}

		public void SetTransform(Matrix xform)
		{
			this._transform = xform;
			this._transform.Decompose(out this._scale, out this._rotation, out this._translation);
			this._dirty = false;
		}

		public void SetTransform(Vector3 trans, Quaternion rot, Vector3 scale)
		{
			this._translation = trans;
			this._scale = scale;
			this._rotation = rot;
			this.ComposeTransform();
			this._dirty = false;
		}

		public Bone(int index, string name, Bone parent, Vector3 translation, Quaternion rotation, Vector3 scale, IList<Bone> children)
		{
			this.Index = index;
			this.Name = name;
			this.SetTransform(translation, rotation, scale);
			this.Children = new ReadOnlyCollection<Bone>(children);
		}

		public Bone(int index, string name, Bone parent, Matrix xform, IList<Bone> children)
		{
			this.Index = index;
			this.Name = name;
			this.SetTransform(xform);
			this.Children = new ReadOnlyCollection<Bone>(children);
		}

		private Bone()
		{
		}

		public static Skeleton BuildSkeleton(AvatarRenderer avatarRenderer)
		{
			return Bone.BuildSkeleton(avatarRenderer.BindPose, avatarRenderer.ParentBones, Avatar.BoneNames);
		}

		public static Skeleton BuildSkeleton(Model model)
		{
			int count = model.Bones.Count;
			Matrix[] array = new Matrix[count];
			string[] array2 = new string[count];
			int[] array3 = new int[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = model.Bones[i].Transform;
				array2[i] = model.Bones[i].Name;
				array3[i] = ((model.Bones[i].Parent == null) ? (-1) : model.Bones[i].Parent.Index);
			}
			return Bone.BuildSkeleton(array, array3, array2);
		}

		public static Skeleton BuildSkeleton(IList<Matrix> transforms, IList<int> heirachy, IList<string> names)
		{
			Bone[] array = new Bone[transforms.Count];
			for (int i = 0; i < transforms.Count; i++)
			{
				if (heirachy[i] < 0)
				{
					Bone bone = new Bone();
					bone.Index = i;
					bone.Name = names[i];
					bone.SetTransform(transforms[i]);
					array[bone.Index] = bone;
					bone.BuildSubSkeleton(array, transforms, heirachy, names);
				}
			}
			return new Skeleton(array);
		}

		private void BuildSubSkeleton(Bone[] bones, IList<Matrix> transforms, IList<int> heirachy, IList<string> names)
		{
			List<Bone> list = new List<Bone>();
			for (int i = 0; i < transforms.Count; i++)
			{
				if (heirachy[i] == this.Index)
				{
					Bone bone = new Bone();
					bone.Index = i;
					bone.Name = names[i];
					bone.SetTransform(transforms[i]);
					bone.Parent = this;
					bones[bone.Index] = bone;
					bone.BuildSubSkeleton(bones, transforms, heirachy, names);
					list.Add(bone);
				}
			}
			this.Children = new ReadOnlyCollection<Bone>(list);
		}

		public override string ToString()
		{
			return this.Name;
		}

		[ContentSerializer]
		private Vector3 _translation;

		private bool _unitScale;

		[ContentSerializer]
		private Vector3 _scale;

		[ContentSerializer]
		private Quaternion _rotation;

		private bool _dirty = true;

		internal Matrix _transform;
	}
}
