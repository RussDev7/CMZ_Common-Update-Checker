using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace DNA.Collections
{
	public static class Byte4ArrayTool
	{
		unsafe static Byte4ArrayTool()
		{
			fixed (IntPtr* pBytes = new byte[1])
			{
				fixed (IntPtr* pByte4s = new Byte4[1])
				{
					fixed (IntPtr* pUnts = new uint[1])
					{
						Byte4ArrayTool.BYTE_ARRAY_TYPE = Byte4ArrayTool.getHeader((void*)pBytes)->type;
						Byte4ArrayTool.BYTE4_ARRAY_TYPE = Byte4ArrayTool.getHeader((void*)pByte4s)->type;
						Byte4ArrayTool.UINT_ARRAY_TYPE = Byte4ArrayTool.getHeader((void*)pUnts)->type;
					}
				}
			}
		}

		public static void AsByteArray(this Byte4[] byte4s, Action<byte[]> action)
		{
			if (byte4s.handleNullOrEmptyArray(action))
			{
				return;
			}
			Byte4ArrayTool.Union union = new Byte4ArrayTool.Union
			{
				byte4s = byte4s
			};
			union.byte4s.toByteArray();
			try
			{
				action(union.bytes);
			}
			finally
			{
				union.bytes.toByte4Array();
			}
		}

		public static void AsUintArray(this Byte4[] byte4s, Action<uint[]> action)
		{
			if (byte4s.handleNullOrEmptyArray(action))
			{
				return;
			}
			Byte4ArrayTool.Union union = new Byte4ArrayTool.Union
			{
				byte4s = byte4s
			};
			union.byte4s.toUintArray();
			try
			{
				action(union.uints);
			}
			finally
			{
				union.uints.toByte4Array();
			}
		}

		public static void AsByte4Array(this byte[] bytes, Action<Byte4[]> action)
		{
			if (bytes.handleNullOrEmptyArray(action))
			{
				return;
			}
			Byte4ArrayTool.Union union = new Byte4ArrayTool.Union
			{
				bytes = bytes
			};
			union.bytes.toByte4Array();
			try
			{
				action(union.byte4s);
			}
			finally
			{
				union.byte4s.toByteArray();
			}
		}

		public static void AsByte4Array(this uint[] uints, Action<Byte4[]> action)
		{
			if (uints.handleNullOrEmptyArray(action))
			{
				return;
			}
			Byte4ArrayTool.Union union = new Byte4ArrayTool.Union
			{
				uints = uints
			};
			union.bytes.toByte4Array();
			try
			{
				action(union.byte4s);
			}
			finally
			{
				union.byte4s.toUintArray();
			}
		}

		public static bool handleNullOrEmptyArray<TSrc, TDst>(this TSrc[] array, Action<TDst[]> action)
		{
			if (array == null)
			{
				action(null);
				return true;
			}
			if (array.Length == 0)
			{
				action(new TDst[0]);
				return true;
			}
			return false;
		}

		private unsafe static Byte4ArrayTool.ArrayHeader* getHeader(void* pBytes)
		{
			return (Byte4ArrayTool.ArrayHeader*)((byte*)pBytes - sizeof(Byte4ArrayTool.ArrayHeader));
		}

		private unsafe static void toByte4Array(this byte[] bytes)
		{
			fixed (IntPtr* pArray = bytes)
			{
				Byte4ArrayTool.ArrayHeader* pHeader = Byte4ArrayTool.getHeader((void*)pArray);
				pHeader->type = Byte4ArrayTool.BYTE4_ARRAY_TYPE;
				pHeader->length = (UIntPtr)((ulong)((long)(bytes.Length / sizeof(Byte4))));
			}
		}

		private unsafe static void toByte4Array(this uint[] uints)
		{
			fixed (IntPtr* pArray = uints)
			{
				Byte4ArrayTool.ArrayHeader* pHeader = Byte4ArrayTool.getHeader((void*)pArray);
				pHeader->type = Byte4ArrayTool.BYTE4_ARRAY_TYPE;
			}
		}

		private unsafe static void toByteArray(this Byte4[] floats)
		{
			fixed (IntPtr* pArray = floats)
			{
				Byte4ArrayTool.ArrayHeader* pHeader = Byte4ArrayTool.getHeader((void*)pArray);
				pHeader->type = Byte4ArrayTool.BYTE_ARRAY_TYPE;
				pHeader->length = (UIntPtr)((ulong)((long)(floats.Length * sizeof(Byte4))));
			}
		}

		private unsafe static void toUintArray(this Byte4[] byte4s)
		{
			fixed (IntPtr* pArray = byte4s)
			{
				Byte4ArrayTool.ArrayHeader* pHeader = Byte4ArrayTool.getHeader((void*)pArray);
				pHeader->type = Byte4ArrayTool.UINT_ARRAY_TYPE;
			}
		}

		private static readonly UIntPtr BYTE_ARRAY_TYPE;

		private static readonly UIntPtr BYTE4_ARRAY_TYPE;

		private static readonly UIntPtr UINT_ARRAY_TYPE;

		[StructLayout(LayoutKind.Explicit)]
		private struct Union
		{
			[FieldOffset(0)]
			public byte[] bytes;

			[FieldOffset(0)]
			public Byte4[] byte4s;

			[FieldOffset(0)]
			public uint[] uints;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct ArrayHeader
		{
			public UIntPtr type;

			public UIntPtr length;
		}
	}
}
