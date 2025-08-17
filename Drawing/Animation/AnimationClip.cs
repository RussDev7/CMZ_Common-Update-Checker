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
			Matrix[] array = new Matrix[this.BoneCount];
			List<Vector3>[] array2 = new List<Vector3>[this.BoneCount];
			List<Quaternion>[] array3 = new List<Quaternion>[this.BoneCount];
			List<Vector3>[] array4 = new List<Vector3>[this.BoneCount];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = new List<Vector3>();
				array3[i] = new List<Quaternion>();
				array4[i] = new List<Vector3>();
			}
			TimeSpan timeSpan = TimeSpan.FromSeconds(1.0 / (double)frameRate);
			TimeSpan timeSpan2 = TimeSpan.Zero;
			while (timeSpan2 <= this.Duration)
			{
				float num = (float)((double)this._animationFrameRate * timeSpan2.TotalSeconds);
				int num2 = (int)num;
				float num3 = num - (float)num2;
				for (int j = 0; j < array.Length; j++)
				{
					Quaternion[] array5 = this._rotations[j];
					Quaternion quaternion;
					if (num2 >= array5.Length)
					{
						quaternion = array5[array5.Length - 1];
					}
					else
					{
						quaternion = array5[num2];
						if (num2 < array5.Length - 2)
						{
							quaternion = Quaternion.Slerp(quaternion, array5[num2 + 1], num3);
						}
					}
					Vector3[] array6 = this._positions[j];
					Vector3 vector;
					if (num2 >= array6.Length)
					{
						vector = array6[array6.Length - 1];
					}
					else
					{
						vector = array6[num2];
						if (num2 < array6.Length - 2)
						{
							vector = Vector3.Lerp(vector, array6[num2 + 1], num3);
						}
					}
					Vector3[] array7 = this._scales[j];
					Vector3 vector2;
					if (num2 >= array7.Length)
					{
						vector2 = array7[array7.Length - 1];
					}
					else
					{
						vector2 = array7[num2];
						if (num2 < array7.Length - 2)
						{
							vector2 = Vector3.Lerp(vector2, array7[num2 + 1], num3);
						}
					}
					array4[j].Add(vector2);
					array2[j].Add(vector);
					array3[j].Add(quaternion);
				}
				timeSpan2 += timeSpan;
			}
			this._frameRate = frameRate;
			this._scales = new Vector3[this.BoneCount][];
			this._positions = new Vector3[this.BoneCount][];
			this._rotations = new Quaternion[this.BoneCount][];
			for (int k = 0; k < array.Length; k++)
			{
				this._scales[k] = array4[k].ToArray();
				this._positions[k] = array2[k].ToArray();
				this._rotations[k] = array3[k].ToArray();
			}
			this.ReduceKeys();
		}

		public void ReduceKeys()
		{
			for (int i = 0; i < this.BoneCount; i++)
			{
				bool flag = true;
				for (int j = 1; j < this._positions[i].Length; j++)
				{
					if (this._positions[i][0] != this._positions[i][j])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					Vector3 vector = this._positions[i][0];
					this._positions[i] = new Vector3[1];
					this._positions[i][0] = vector;
				}
				flag = true;
				for (int k = 1; k < this._scales[i].Length; k++)
				{
					if (this._scales[i][0] != this._scales[i][k])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					Vector3 vector2 = this._scales[i][0];
					this._scales[i] = new Vector3[1];
					this._scales[i][0] = vector2;
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
				flag = true;
				for (int m = 1; m < this._rotations[i].Length; m++)
				{
					if (this._rotations[i][0] != this._rotations[i][m])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					Quaternion quaternion = this._rotations[i][0];
					this._rotations[i] = new Quaternion[1];
					this._rotations[i][0] = quaternion;
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
				int count = keys[i].Count;
				this._positions[i] = new Vector3[count];
				this._scales[i] = new Vector3[count];
				this._rotations[i] = new Quaternion[count];
				for (int j = 0; j < count; j++)
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
			AnimationClip animationClip = new AnimationClip();
			animationClip.Read(reader);
			return animationClip;
		}

		public void CopyTransforms(Vector3[] translations, Quaternion[] rotations, Vector3[] scales, TimeSpan position, bool[] influenceMap)
		{
			float num = (float)((double)this._animationFrameRate * position.TotalSeconds);
			int num2 = (int)num;
			float num3 = num - (float)num2;
			int num4 = translations.Length;
			for (int i = 0; i < num4; i++)
			{
				if (influenceMap == null || influenceMap[i])
				{
					Quaternion[] array = this._rotations[i];
					Quaternion quaternion;
					if (num2 >= array.Length)
					{
						quaternion = array[array.Length - 1];
					}
					else
					{
						quaternion = array[num2];
						if (num2 < array.Length - 2)
						{
							quaternion = Quaternion.Slerp(quaternion, array[num2 + 1], num3);
						}
					}
					Vector3[] array2 = this._positions[i];
					Vector3 vector;
					if (num2 >= array2.Length)
					{
						vector = array2[array2.Length - 1];
					}
					else
					{
						vector = array2[num2];
						if (num2 < array2.Length - 2)
						{
							vector = Vector3.Lerp(vector, array2[num2 + 1], num3);
						}
					}
					Vector3[] array3 = this._scales[i];
					Vector3 vector2;
					if (num2 >= array3.Length)
					{
						vector2 = array3[array3.Length - 1];
					}
					else
					{
						vector2 = array3[num2];
						if (num2 < array3.Length - 2)
						{
							vector2 = Vector3.Lerp(vector2, array3[num2 + 1], num3);
						}
					}
					translations[i] = vector;
					rotations[i] = quaternion;
					scales[i] = vector2;
				}
			}
		}

		public void Read(BinaryReader reader)
		{
			this.Name = reader.ReadString();
			this._animationFrameRate = reader.ReadInt32();
			this.Duration = TimeSpan.FromTicks(reader.ReadInt64());
			int num = reader.ReadInt32();
			this._scales = new Vector3[num][];
			this._positions = new Vector3[num][];
			this._rotations = new Quaternion[num][];
			for (int i = 0; i < num; i++)
			{
				int num2 = reader.ReadInt32();
				this._positions[i] = new Vector3[num2];
				for (int j = 0; j < num2; j++)
				{
					this._positions[i][j] = reader.ReadVector3();
				}
				num2 = reader.ReadInt32();
				this._rotations[i] = new Quaternion[num2];
				for (int k = 0; k < num2; k++)
				{
					this._rotations[i][k] = reader.ReadQuaternion();
				}
				num2 = reader.ReadInt32();
				this._scales[i] = new Vector3[num2];
				for (int l = 0; l < num2; l++)
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
				int num = this._positions[i].Length;
				writer.Write(num);
				for (int j = 0; j < num; j++)
				{
					writer.Write(this._positions[i][j]);
				}
				num = this._rotations[i].Length;
				writer.Write(num);
				for (int k = 0; k < num; k++)
				{
					writer.Write(this._rotations[i][k]);
				}
				num = this._scales[i].Length;
				writer.Write(num);
				for (int l = 0; l < num; l++)
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
				AnimationClip animationClip = new AnimationClip();
				animationClip.Read(input);
				return animationClip;
			}
		}
	}
}
