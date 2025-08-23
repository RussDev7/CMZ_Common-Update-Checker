using System;
using System.Collections.Generic;
using System.IO;
using DNA.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace DNA.Drawing.Animation
{
	public class AnimationClip
	{
		public string Name { get; private set; }

		public TimeSpan Duration { get; private set; }

		public int BoneCount
		{
			get
			{
				return this._positions.Length;
			}
		}

		public void Resample(int frameRate)
		{
			Matrix[] boneTransforms = new Matrix[this.BoneCount];
			List<Vector3>[] newPosList = new List<Vector3>[this.BoneCount];
			List<Quaternion>[] newRotList = new List<Quaternion>[this.BoneCount];
			List<Vector3>[] newScaleList = new List<Vector3>[this.BoneCount];
			for (int i = 0; i < boneTransforms.Length; i++)
			{
				newPosList[i] = new List<Vector3>();
				newRotList[i] = new List<Quaternion>();
				newScaleList[i] = new List<Vector3>();
			}
			TimeSpan inc = TimeSpan.FromSeconds(1.0 / (double)frameRate);
			TimeSpan time = TimeSpan.Zero;
			while (time <= this.Duration)
			{
				float frameTime = (float)((double)this._animationFrameRate * time.TotalSeconds);
				int currentKeyframe = (int)frameTime;
				float blender = frameTime - (float)currentKeyframe;
				for (int j = 0; j < boneTransforms.Length; j++)
				{
					Quaternion[] quatKeys = this._rotations[j];
					Quaternion rotation;
					if (currentKeyframe >= quatKeys.Length)
					{
						rotation = quatKeys[quatKeys.Length - 1];
					}
					else
					{
						rotation = quatKeys[currentKeyframe];
						if (currentKeyframe < quatKeys.Length - 2)
						{
							rotation = Quaternion.Slerp(rotation, quatKeys[currentKeyframe + 1], blender);
						}
					}
					Vector3[] tranKeys = this._positions[j];
					Vector3 translation;
					if (currentKeyframe >= tranKeys.Length)
					{
						translation = tranKeys[tranKeys.Length - 1];
					}
					else
					{
						translation = tranKeys[currentKeyframe];
						if (currentKeyframe < tranKeys.Length - 2)
						{
							translation = Vector3.Lerp(translation, tranKeys[currentKeyframe + 1], blender);
						}
					}
					Vector3[] scaleKeys = this._scales[j];
					Vector3 scale;
					if (currentKeyframe >= scaleKeys.Length)
					{
						scale = scaleKeys[scaleKeys.Length - 1];
					}
					else
					{
						scale = scaleKeys[currentKeyframe];
						if (currentKeyframe < scaleKeys.Length - 2)
						{
							scale = Vector3.Lerp(scale, scaleKeys[currentKeyframe + 1], blender);
						}
					}
					newScaleList[j].Add(scale);
					newPosList[j].Add(translation);
					newRotList[j].Add(rotation);
				}
				time += inc;
			}
			this._frameRate = frameRate;
			this._scales = new Vector3[this.BoneCount][];
			this._positions = new Vector3[this.BoneCount][];
			this._rotations = new Quaternion[this.BoneCount][];
			for (int k = 0; k < boneTransforms.Length; k++)
			{
				this._scales[k] = newScaleList[k].ToArray();
				this._positions[k] = newPosList[k].ToArray();
				this._rotations[k] = newRotList[k].ToArray();
			}
			this.ReduceKeys();
		}

		public void ReduceKeys()
		{
			for (int i = 0; i < this.BoneCount; i++)
			{
				bool reduce = true;
				for (int j = 1; j < this._positions[i].Length; j++)
				{
					if (this._positions[i][0] != this._positions[i][j])
					{
						reduce = false;
						break;
					}
				}
				if (reduce)
				{
					Vector3 pos = this._positions[i][0];
					this._positions[i] = new Vector3[1];
					this._positions[i][0] = pos;
				}
				reduce = true;
				for (int k = 1; k < this._scales[i].Length; k++)
				{
					if (this._scales[i][0] != this._scales[i][k])
					{
						reduce = false;
						break;
					}
				}
				if (reduce)
				{
					Vector3 pos2 = this._scales[i][0];
					this._scales[i] = new Vector3[1];
					this._scales[i][0] = pos2;
				}
				for (int l = 0; l < this._scales[i].Length; l++)
				{
					if ((double)Math.Abs(this._scales[i][l].X - 1f) < 0.001)
					{
						this._scales[i][l].X = 1f;
					}
					if ((double)Math.Abs(this._scales[i][l].Y - 1f) < 0.001)
					{
						this._scales[i][l].Y = 1f;
					}
					if ((double)Math.Abs(this._scales[i][l].Z - 1f) < 0.001)
					{
						this._scales[i][l].Z = 1f;
					}
				}
				reduce = true;
				for (int m = 1; m < this._rotations[i].Length; m++)
				{
					if (this._rotations[i][0] != this._rotations[i][m])
					{
						reduce = false;
						break;
					}
				}
				if (reduce)
				{
					Quaternion pos3 = this._rotations[i][0];
					this._rotations[i] = new Quaternion[1];
					this._rotations[i][0] = pos3;
				}
			}
		}

		public AnimationClip(string name, int frameRate, TimeSpan duration, Vector3[][] positions, Quaternion[][] rotations, Vector3[][] scales)
		{
			this.Name = name;
			this._frameRate = frameRate;
			this.Duration = duration;
			if (positions.Length != rotations.Length)
			{
				throw new Exception("Bone Counts Must be the same");
			}
			this._positions = positions;
			this._rotations = rotations;
			this._scales = scales;
		}

		public AnimationClip(string name, int frameRate, TimeSpan duration, IList<IList<Matrix>> keys)
		{
			this.Name = name;
			this._frameRate = frameRate;
			this.Duration = duration;
			this._scales = new Vector3[keys.Count][];
			this._positions = new Vector3[keys.Count][];
			this._rotations = new Quaternion[keys.Count][];
			for (int i = 0; i < keys.Count; i++)
			{
				int keyCount = keys[i].Count;
				this._positions[i] = new Vector3[keyCount];
				this._scales[i] = new Vector3[keyCount];
				this._rotations[i] = new Quaternion[keyCount];
				for (int j = 0; j < keyCount; j++)
				{
					keys[i][j].Decompose(out this._scales[i][j], out this._rotations[i][j], out this._positions[i][j]);
				}
			}
			this.ReduceKeys();
		}

		protected AnimationClip()
		{
		}

		public static AnimationClip Load(BinaryReader reader)
		{
			AnimationClip clip = new AnimationClip();
			clip.Read(reader);
			return clip;
		}

		public void CopyTransforms(Vector3[] translations, Quaternion[] rotations, Vector3[] scales, TimeSpan position, bool[] influenceMap)
		{
			float frameTime = (float)((double)this._animationFrameRate * position.TotalSeconds);
			int currentKeyframe = (int)frameTime;
			float blender = frameTime - (float)currentKeyframe;
			int boneCount = translations.Length;
			for (int i = 0; i < boneCount; i++)
			{
				if (influenceMap == null || influenceMap[i])
				{
					Quaternion[] quatKeys = this._rotations[i];
					Quaternion rotation;
					if (currentKeyframe >= quatKeys.Length)
					{
						rotation = quatKeys[quatKeys.Length - 1];
					}
					else
					{
						rotation = quatKeys[currentKeyframe];
						if (currentKeyframe < quatKeys.Length - 2)
						{
							rotation = Quaternion.Slerp(rotation, quatKeys[currentKeyframe + 1], blender);
						}
					}
					Vector3[] tranKeys = this._positions[i];
					Vector3 translation;
					if (currentKeyframe >= tranKeys.Length)
					{
						translation = tranKeys[tranKeys.Length - 1];
					}
					else
					{
						translation = tranKeys[currentKeyframe];
						if (currentKeyframe < tranKeys.Length - 2)
						{
							translation = Vector3.Lerp(translation, tranKeys[currentKeyframe + 1], blender);
						}
					}
					Vector3[] scaleKeys = this._scales[i];
					Vector3 scale;
					if (currentKeyframe >= scaleKeys.Length)
					{
						scale = scaleKeys[scaleKeys.Length - 1];
					}
					else
					{
						scale = scaleKeys[currentKeyframe];
						if (currentKeyframe < scaleKeys.Length - 2)
						{
							scale = Vector3.Lerp(scale, scaleKeys[currentKeyframe + 1], blender);
						}
					}
					translations[i] = translation;
					rotations[i] = rotation;
					scales[i] = scale;
				}
			}
		}

		public void Read(BinaryReader reader)
		{
			this.Name = reader.ReadString();
			this._animationFrameRate = reader.ReadInt32();
			this.Duration = TimeSpan.FromTicks(reader.ReadInt64());
			int boneCount = reader.ReadInt32();
			this._scales = new Vector3[boneCount][];
			this._positions = new Vector3[boneCount][];
			this._rotations = new Quaternion[boneCount][];
			for (int i = 0; i < boneCount; i++)
			{
				int frames = reader.ReadInt32();
				this._positions[i] = new Vector3[frames];
				for (int j = 0; j < frames; j++)
				{
					this._positions[i][j] = reader.ReadVector3();
				}
				frames = reader.ReadInt32();
				this._rotations[i] = new Quaternion[frames];
				for (int k = 0; k < frames; k++)
				{
					this._rotations[i][k] = reader.ReadQuaternion();
				}
				frames = reader.ReadInt32();
				this._scales[i] = new Vector3[frames];
				for (int l = 0; l < frames; l++)
				{
					this._scales[i][l] = reader.ReadVector3();
				}
			}
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(this.Name);
			writer.Write(this._animationFrameRate);
			writer.Write(this.Duration.Ticks);
			writer.Write(this.BoneCount);
			for (int i = 0; i < this.BoneCount; i++)
			{
				int frames = this._positions[i].Length;
				writer.Write(frames);
				for (int j = 0; j < frames; j++)
				{
					writer.Write(this._positions[i][j]);
				}
				frames = this._rotations[i].Length;
				writer.Write(frames);
				for (int k = 0; k < frames; k++)
				{
					writer.Write(this._rotations[i][k]);
				}
				frames = this._scales[i].Length;
				writer.Write(frames);
				for (int l = 0; l < frames; l++)
				{
					writer.Write(this._scales[i][l]);
				}
			}
		}

		private int _animationFrameRate = 30;

		protected Vector3[][] _positions;

		protected Quaternion[][] _rotations;

		protected Vector3[][] _scales;

		private int _frameRate;

		public class Reader : ContentTypeReader<AnimationClip>
		{
			protected override AnimationClip Read(ContentReader input, AnimationClip existingInstance)
			{
				AnimationClip data = new AnimationClip();
				data.Read(input);
				return data;
			}
		}
	}
}
