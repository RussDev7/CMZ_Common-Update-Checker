using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Profiling
{
	public class ProfilerUtils
	{
		public static ProfilerThreadEnum ThreadIndex
		{
			get
			{
				return ProfilerUtils._threadIndex;
			}
			set
			{
				ProfilerUtils._threadIndex = value;
			}
		}

		public static void SeedRandom(int seed)
		{
			ProfilerUtils.random = new Random(seed);
		}

		public static int NextInt(int top)
		{
			return ProfilerUtils.random.Next(top);
		}

		public static float NextFloat()
		{
			return (float)ProfilerUtils.random.NextDouble();
		}

		public static BinaryReader OpenBinaryFile(string dir, string filename)
		{
			Stream stream = TitleContainer.OpenStream(Path.Combine(dir, filename));
			return new BinaryReader(stream);
		}

		public static string ReadBinaryString(BinaryReader br)
		{
			int num = (int)br.ReadUInt16();
			byte[] array = br.ReadBytes(num);
			return Encoding.UTF8.GetString(array, 0, array.Length);
		}

		public static void RemoveBySwap<T>(List<T> theList, T theElement) where T : class
		{
			for (int i = 0; i < theList.Count; i++)
			{
				if (theList[i] == theElement)
				{
					theList[i] = theList[theList.Count - 1];
					theList.RemoveAt(theList.Count - 1);
					return;
				}
			}
		}

		public static Color MultiplyColors(Color c1, Color c2)
		{
			byte b = (byte)((uint)(c1.R * c2.R) >> 8);
			byte b2 = (byte)((uint)(c1.G * c2.G) >> 8);
			byte b3 = (byte)((uint)(c1.B * c2.B) >> 8);
			byte b4 = (byte)((uint)(c1.A * c2.A) >> 8);
			return new Color((int)b, (int)b2, (int)b3, (int)b4);
		}

		public static Matrix _standard2DProjection;

		public static object TRUE_VALUE = true;

		public static object FALSE_VALUE = null;

		[ThreadStatic]
		private static ProfilerThreadEnum _threadIndex;

		private static Random random = new Random();

		public static SpriteFont SystemFont;
	}
}
