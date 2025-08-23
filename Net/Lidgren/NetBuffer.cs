using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;

namespace DNA.Net.Lidgren
{
	public class NetBuffer
	{
		public byte[] PeekDataBuffer()
		{
			return this.m_data;
		}

		public bool PeekBoolean()
		{
			byte retval = NetBitWriter.ReadByte(this.m_data, 1, this.m_readPosition);
			return retval > 0;
		}

		public byte PeekByte()
		{
			return NetBitWriter.ReadByte(this.m_data, 8, this.m_readPosition);
		}

		[CLSCompliant(false)]
		public sbyte PeekSByte()
		{
			byte retval = NetBitWriter.ReadByte(this.m_data, 8, this.m_readPosition);
			return (sbyte)retval;
		}

		public byte PeekByte(int numberOfBits)
		{
			return NetBitWriter.ReadByte(this.m_data, numberOfBits, this.m_readPosition);
		}

		public byte[] PeekBytes(int numberOfBytes)
		{
			byte[] retval = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(this.m_data, numberOfBytes, this.m_readPosition, retval, 0);
			return retval;
		}

		public void PeekBytes(byte[] into, int offset, int numberOfBytes)
		{
			NetBitWriter.ReadBytes(this.m_data, numberOfBytes, this.m_readPosition, into, offset);
		}

		public short PeekInt16()
		{
			uint retval = (uint)NetBitWriter.ReadUInt16(this.m_data, 16, this.m_readPosition);
			return (short)retval;
		}

		[CLSCompliant(false)]
		public ushort PeekUInt16()
		{
			uint retval = (uint)NetBitWriter.ReadUInt16(this.m_data, 16, this.m_readPosition);
			return (ushort)retval;
		}

		public int PeekInt32()
		{
			return (int)NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
		}

		public int PeekInt32(int numberOfBits)
		{
			uint retval = NetBitWriter.ReadUInt32(this.m_data, numberOfBits, this.m_readPosition);
			if (numberOfBits == 32)
			{
				return (int)retval;
			}
			int signBit = 1 << numberOfBits - 1;
			if (((ulong)retval & (ulong)((long)signBit)) == 0UL)
			{
				return (int)retval;
			}
			uint mask = uint.MaxValue >> 33 - numberOfBits;
			uint tmp = (retval & mask) + 1U;
			return (int)(-(int)tmp);
		}

		[CLSCompliant(false)]
		public uint PeekUInt32()
		{
			return NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
		}

		[CLSCompliant(false)]
		public uint PeekUInt32(int numberOfBits)
		{
			return NetBitWriter.ReadUInt32(this.m_data, numberOfBits, this.m_readPosition);
		}

		[CLSCompliant(false)]
		public ulong PeekUInt64()
		{
			ulong low = (ulong)NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
			ulong high = (ulong)NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition + 32);
			return low + (high << 32);
		}

		public long PeekInt64()
		{
			return (long)this.PeekUInt64();
		}

		[CLSCompliant(false)]
		public ulong PeekUInt64(int numberOfBits)
		{
			ulong retval;
			if (numberOfBits <= 32)
			{
				retval = (ulong)NetBitWriter.ReadUInt32(this.m_data, numberOfBits, this.m_readPosition);
			}
			else
			{
				retval = (ulong)NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
				retval |= (ulong)NetBitWriter.ReadUInt32(this.m_data, numberOfBits - 32, this.m_readPosition);
			}
			return retval;
		}

		public long PeekInt64(int numberOfBits)
		{
			return (long)this.PeekUInt64(numberOfBits);
		}

		public float PeekFloat()
		{
			return this.PeekSingle();
		}

		public float PeekSingle()
		{
			if ((this.m_readPosition & 7) == 0)
			{
				return BitConverter.ToSingle(this.m_data, this.m_readPosition >> 3);
			}
			byte[] bytes = this.PeekBytes(4);
			return BitConverter.ToSingle(bytes, 0);
		}

		public double PeekDouble()
		{
			if ((this.m_readPosition & 7) == 0)
			{
				return BitConverter.ToDouble(this.m_data, this.m_readPosition >> 3);
			}
			byte[] bytes = this.PeekBytes(8);
			return BitConverter.ToDouble(bytes, 0);
		}

		public string PeekString()
		{
			int wasReadPosition = this.m_readPosition;
			string retval = this.ReadString();
			this.m_readPosition = wasReadPosition;
			return retval;
		}

		public void ReadAllFields(object target)
		{
			this.ReadAllFields(target, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public void ReadAllFields(object target, BindingFlags flags)
		{
			if (target == null)
			{
				return;
			}
			Type tp = target.GetType();
			FieldInfo[] fields = tp.GetFields(flags);
			NetUtility.SortMembersList(fields);
			foreach (FieldInfo fi in fields)
			{
				MethodInfo readMethod;
				if (NetBuffer.s_readMethods.TryGetValue(fi.FieldType, out readMethod))
				{
					object value = readMethod.Invoke(this, null);
					fi.SetValue(target, value);
				}
			}
		}

		public void ReadAllProperties(object target)
		{
			this.ReadAllProperties(target, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public void ReadAllProperties(object target, BindingFlags flags)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (target == null)
			{
				return;
			}
			Type tp = target.GetType();
			PropertyInfo[] fields = tp.GetProperties(flags);
			NetUtility.SortMembersList(fields);
			foreach (PropertyInfo fi in fields)
			{
				MethodInfo readMethod;
				if (NetBuffer.s_readMethods.TryGetValue(fi.PropertyType, out readMethod))
				{
					object value = readMethod.Invoke(this, null);
					MethodInfo setMethod = fi.GetSetMethod((flags & BindingFlags.NonPublic) == BindingFlags.NonPublic);
					setMethod.Invoke(target, new object[] { value });
				}
			}
		}

		public bool ReadBoolean()
		{
			byte retval = NetBitWriter.ReadByte(this.m_data, 1, this.m_readPosition);
			this.m_readPosition++;
			return retval > 0;
		}

		public byte ReadByte()
		{
			byte retval = NetBitWriter.ReadByte(this.m_data, 8, this.m_readPosition);
			this.m_readPosition += 8;
			return retval;
		}

		public bool ReadByte(out byte result)
		{
			if (this.m_bitLength - this.m_readPosition < 8)
			{
				result = 0;
				return false;
			}
			result = NetBitWriter.ReadByte(this.m_data, 8, this.m_readPosition);
			this.m_readPosition += 8;
			return true;
		}

		[CLSCompliant(false)]
		public sbyte ReadSByte()
		{
			byte retval = NetBitWriter.ReadByte(this.m_data, 8, this.m_readPosition);
			this.m_readPosition += 8;
			return (sbyte)retval;
		}

		public byte ReadByte(int numberOfBits)
		{
			byte retval = NetBitWriter.ReadByte(this.m_data, numberOfBits, this.m_readPosition);
			this.m_readPosition += numberOfBits;
			return retval;
		}

		public byte[] ReadBytes(int numberOfBytes)
		{
			byte[] retval = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(this.m_data, numberOfBytes, this.m_readPosition, retval, 0);
			this.m_readPosition += 8 * numberOfBytes;
			return retval;
		}

		public bool ReadBytes(int numberOfBytes, out byte[] result)
		{
			if (this.m_bitLength - this.m_readPosition + 7 < numberOfBytes * 8)
			{
				result = null;
				return false;
			}
			result = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(this.m_data, numberOfBytes, this.m_readPosition, result, 0);
			this.m_readPosition += 8 * numberOfBytes;
			return true;
		}

		public bool GetAlignedData(out byte[] data, out int offset)
		{
			if ((this.m_readPosition & 7) == 0)
			{
				data = this.m_data;
				offset = this.m_readPosition >> 3;
				return true;
			}
			data = null;
			offset = 0;
			return false;
		}

		public void ReadBytes(byte[] into, int offset, int numberOfBytes)
		{
			NetBitWriter.ReadBytes(this.m_data, numberOfBytes, this.m_readPosition, into, offset);
			this.m_readPosition += 8 * numberOfBytes;
		}

		public void ReadBits(byte[] into, int offset, int numberOfBits)
		{
			int numberOfWholeBytes = numberOfBits / 8;
			int extraBits = numberOfBits - numberOfWholeBytes * 8;
			NetBitWriter.ReadBytes(this.m_data, numberOfWholeBytes, this.m_readPosition, into, offset);
			this.m_readPosition += 8 * numberOfWholeBytes;
			if (extraBits > 0)
			{
				into[offset + numberOfWholeBytes] = this.ReadByte(extraBits);
			}
		}

		public short ReadInt16()
		{
			uint retval = (uint)NetBitWriter.ReadUInt16(this.m_data, 16, this.m_readPosition);
			this.m_readPosition += 16;
			return (short)retval;
		}

		[CLSCompliant(false)]
		public ushort ReadUInt16()
		{
			uint retval = (uint)NetBitWriter.ReadUInt16(this.m_data, 16, this.m_readPosition);
			this.m_readPosition += 16;
			return (ushort)retval;
		}

		public int ReadInt32()
		{
			uint retval = NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
			this.m_readPosition += 32;
			return (int)retval;
		}

		[CLSCompliant(false)]
		public bool ReadInt32(out int result)
		{
			if (this.m_bitLength - this.m_readPosition < 32)
			{
				result = 0;
				return false;
			}
			result = (int)NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
			this.m_readPosition += 32;
			return true;
		}

		public int ReadInt32(int numberOfBits)
		{
			uint retval = NetBitWriter.ReadUInt32(this.m_data, numberOfBits, this.m_readPosition);
			this.m_readPosition += numberOfBits;
			if (numberOfBits == 32)
			{
				return (int)retval;
			}
			int signBit = 1 << numberOfBits - 1;
			if (((ulong)retval & (ulong)((long)signBit)) == 0UL)
			{
				return (int)retval;
			}
			uint mask = uint.MaxValue >> 33 - numberOfBits;
			uint tmp = (retval & mask) + 1U;
			return (int)(-(int)tmp);
		}

		[CLSCompliant(false)]
		public uint ReadUInt32()
		{
			uint retval = NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
			this.m_readPosition += 32;
			return retval;
		}

		[CLSCompliant(false)]
		public bool ReadUInt32(out uint result)
		{
			if (this.m_bitLength - this.m_readPosition < 32)
			{
				result = 0U;
				return false;
			}
			result = NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
			this.m_readPosition += 32;
			return true;
		}

		[CLSCompliant(false)]
		public uint ReadUInt32(int numberOfBits)
		{
			uint retval = NetBitWriter.ReadUInt32(this.m_data, numberOfBits, this.m_readPosition);
			this.m_readPosition += numberOfBits;
			return retval;
		}

		[CLSCompliant(false)]
		public ulong ReadUInt64()
		{
			ulong low = (ulong)NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
			this.m_readPosition += 32;
			ulong high = (ulong)NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
			ulong retval = low + (high << 32);
			this.m_readPosition += 32;
			return retval;
		}

		public long ReadInt64()
		{
			return (long)this.ReadUInt64();
		}

		[CLSCompliant(false)]
		public ulong ReadUInt64(int numberOfBits)
		{
			ulong retval;
			if (numberOfBits <= 32)
			{
				retval = (ulong)NetBitWriter.ReadUInt32(this.m_data, numberOfBits, this.m_readPosition);
			}
			else
			{
				retval = (ulong)NetBitWriter.ReadUInt32(this.m_data, 32, this.m_readPosition);
				retval |= (ulong)NetBitWriter.ReadUInt32(this.m_data, numberOfBits - 32, this.m_readPosition);
			}
			this.m_readPosition += numberOfBits;
			return retval;
		}

		public long ReadInt64(int numberOfBits)
		{
			return (long)this.ReadUInt64(numberOfBits);
		}

		public float ReadFloat()
		{
			return this.ReadSingle();
		}

		public float ReadSingle()
		{
			if ((this.m_readPosition & 7) == 0)
			{
				float retval = BitConverter.ToSingle(this.m_data, this.m_readPosition >> 3);
				this.m_readPosition += 32;
				return retval;
			}
			byte[] bytes = this.ReadBytes(4);
			return BitConverter.ToSingle(bytes, 0);
		}

		public bool ReadSingle(out float result)
		{
			if (this.m_bitLength - this.m_readPosition < 32)
			{
				result = 0f;
				return false;
			}
			if ((this.m_readPosition & 7) == 0)
			{
				result = BitConverter.ToSingle(this.m_data, this.m_readPosition >> 3);
				this.m_readPosition += 32;
				return true;
			}
			byte[] bytes = this.ReadBytes(4);
			result = BitConverter.ToSingle(bytes, 0);
			return true;
		}

		public double ReadDouble()
		{
			if ((this.m_readPosition & 7) == 0)
			{
				double retval = BitConverter.ToDouble(this.m_data, this.m_readPosition >> 3);
				this.m_readPosition += 64;
				return retval;
			}
			byte[] bytes = this.ReadBytes(8);
			return BitConverter.ToDouble(bytes, 0);
		}

		[CLSCompliant(false)]
		public uint ReadVariableUInt32()
		{
			int num = 0;
			int num2 = 0;
			byte num3;
			do
			{
				num3 = this.ReadByte();
				num |= (int)(num3 & 127) << num2;
				num2 += 7;
			}
			while ((num3 & 128) != 0);
			return (uint)num;
		}

		[CLSCompliant(false)]
		public bool ReadVariableUInt32(out uint result)
		{
			int num = 0;
			int num2 = 0;
			byte num3;
			while (this.ReadByte(out num3))
			{
				num |= (int)(num3 & 127) << num2;
				num2 += 7;
				if ((num3 & 128) == 0)
				{
					result = (uint)num;
					return true;
				}
			}
			result = 0U;
			return false;
		}

		public int ReadVariableInt32()
		{
			uint i = this.ReadVariableUInt32();
			return (int)((i >> 1) ^ -(int)(i & 1U));
		}

		public long ReadVariableInt64()
		{
			ulong i = this.ReadVariableUInt64();
			return (long)((i >> 1) ^ -(long)(i & 1UL));
		}

		[CLSCompliant(false)]
		public ulong ReadVariableUInt64()
		{
			ulong num = 0UL;
			int num2 = 0;
			byte num3;
			do
			{
				num3 = this.ReadByte();
				num |= ((ulong)num3 & 127UL) << num2;
				num2 += 7;
			}
			while ((num3 & 128) != 0);
			return num;
		}

		public float ReadSignedSingle(int numberOfBits)
		{
			uint encodedVal = this.ReadUInt32(numberOfBits);
			int maxVal = (1 << numberOfBits) - 1;
			return ((encodedVal + 1U) / (float)(maxVal + 1) - 0.5f) * 2f;
		}

		public float ReadUnitSingle(int numberOfBits)
		{
			uint encodedVal = this.ReadUInt32(numberOfBits);
			int maxVal = (1 << numberOfBits) - 1;
			return (encodedVal + 1U) / (float)(maxVal + 1);
		}

		public float ReadRangedSingle(float min, float max, int numberOfBits)
		{
			float range = max - min;
			int maxVal = (1 << numberOfBits) - 1;
			float encodedVal = this.ReadUInt32(numberOfBits);
			float unit = encodedVal / (float)maxVal;
			return min + unit * range;
		}

		public int ReadRangedInteger(int min, int max)
		{
			uint range = (uint)(max - min);
			int numBits = NetUtility.BitsToHoldUInt(range);
			uint rvalue = this.ReadUInt32(numBits);
			return (int)((long)min + (long)((ulong)rvalue));
		}

		public string ReadString()
		{
			int byteLen = (int)this.ReadVariableUInt32();
			if (byteLen == 0)
			{
				return string.Empty;
			}
			if ((this.m_readPosition & 7) == 0)
			{
				string retval = Encoding.UTF8.GetString(this.m_data, this.m_readPosition >> 3, byteLen);
				this.m_readPosition += 8 * byteLen;
				return retval;
			}
			byte[] bytes = this.ReadBytes(byteLen);
			return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}

		public bool ReadString(out string result)
		{
			uint byteLen;
			if (!this.ReadVariableUInt32(out byteLen))
			{
				result = string.Empty;
				return false;
			}
			if (byteLen == 0U)
			{
				result = string.Empty;
				return true;
			}
			if ((long)(this.m_bitLength - this.m_readPosition) < (long)((ulong)(byteLen * 8U)))
			{
				result = string.Empty;
				return false;
			}
			if ((this.m_readPosition & 7) == 0)
			{
				result = Encoding.UTF8.GetString(this.m_data, this.m_readPosition >> 3, (int)byteLen);
				this.m_readPosition += (int)(8U * byteLen);
				return true;
			}
			byte[] bytes;
			if (!this.ReadBytes((int)byteLen, out bytes))
			{
				result = string.Empty;
				return false;
			}
			result = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			return true;
		}

		public double ReadTime(NetConnection connection, bool highPrecision)
		{
			double remoteTime = (highPrecision ? this.ReadDouble() : ((double)this.ReadSingle()));
			if (connection == null)
			{
				throw new NetException("Cannot call ReadTime() on message without a connected sender (ie. unconnected messages)");
			}
			return remoteTime - connection.m_remoteTimeOffset;
		}

		public IPEndPoint ReadIPEndPoint()
		{
			byte len = this.ReadByte();
			byte[] addressBytes = this.ReadBytes((int)len);
			int port = (int)this.ReadUInt16();
			IPAddress address = new IPAddress(addressBytes);
			return new IPEndPoint(address, port);
		}

		public void SkipPadBits()
		{
			this.m_readPosition = (this.m_readPosition + 7 >> 3) * 8;
		}

		public void ReadPadBits()
		{
			this.m_readPosition = (this.m_readPosition + 7 >> 3) * 8;
		}

		public void SkipPadBits(int numberOfBits)
		{
			this.m_readPosition += numberOfBits;
		}

		public void EnsureBufferSize(int numberOfBits)
		{
			int byteLen = numberOfBits + 7 >> 3;
			if (this.m_data == null)
			{
				this.m_data = new byte[byteLen + 4];
				return;
			}
			if (this.m_data.Length < byteLen)
			{
				Array.Resize<byte>(ref this.m_data, byteLen + 4);
			}
		}

		internal void InternalEnsureBufferSize(int numberOfBits)
		{
			int byteLen = numberOfBits + 7 >> 3;
			if (this.m_data == null)
			{
				this.m_data = new byte[byteLen];
				return;
			}
			if (this.m_data.Length < byteLen)
			{
				Array.Resize<byte>(ref this.m_data, byteLen);
			}
		}

		public void Write(bool value)
		{
			this.EnsureBufferSize(this.m_bitLength + 1);
			NetBitWriter.WriteByte(value ? 1 : 0, 1, this.m_data, this.m_bitLength);
			this.m_bitLength++;
		}

		public void Write(byte source)
		{
			this.EnsureBufferSize(this.m_bitLength + 8);
			NetBitWriter.WriteByte(source, 8, this.m_data, this.m_bitLength);
			this.m_bitLength += 8;
		}

		[CLSCompliant(false)]
		public void Write(sbyte source)
		{
			this.EnsureBufferSize(this.m_bitLength + 8);
			NetBitWriter.WriteByte((byte)source, 8, this.m_data, this.m_bitLength);
			this.m_bitLength += 8;
		}

		public void Write(byte source, int numberOfBits)
		{
			this.EnsureBufferSize(this.m_bitLength + numberOfBits);
			NetBitWriter.WriteByte(source, numberOfBits, this.m_data, this.m_bitLength);
			this.m_bitLength += numberOfBits;
		}

		public void Write(byte[] source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			int bits = source.Length * 8;
			this.EnsureBufferSize(this.m_bitLength + bits);
			NetBitWriter.WriteBytes(source, 0, source.Length, this.m_data, this.m_bitLength);
			this.m_bitLength += bits;
		}

		public void Write(byte[] source, int offsetInBytes, int numberOfBytes)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			int bits = numberOfBytes * 8;
			this.EnsureBufferSize(this.m_bitLength + bits);
			NetBitWriter.WriteBytes(source, offsetInBytes, numberOfBytes, this.m_data, this.m_bitLength);
			this.m_bitLength += bits;
		}

		public void CopyBytesFrom(NetBuffer src, int numberOfBytes)
		{
			if (src == null)
			{
				throw new ArgumentNullException("source");
			}
			int bits = numberOfBytes * 8;
			this.EnsureBufferSize(this.m_bitLength + bits);
			if (src.m_bitLength - src.m_readPosition + 7 < numberOfBytes * 8)
			{
				return;
			}
			if ((this.m_bitLength & 7) == 0)
			{
				NetBitWriter.ReadBytes(src.m_data, numberOfBytes, src.m_readPosition, this.m_data, this.m_bitLength >> 3);
				src.m_readPosition += numberOfBytes * 8;
				this.m_bitLength += numberOfBytes * 8;
				return;
			}
			byte[] source = src.ReadBytes(numberOfBytes);
			if (source != null)
			{
				NetBitWriter.WriteBytes(source, 0, numberOfBytes, this.m_data, this.m_bitLength);
				this.m_bitLength += numberOfBytes * 8;
			}
		}

		[CLSCompliant(false)]
		public void Write(ushort source)
		{
			this.EnsureBufferSize(this.m_bitLength + 16);
			NetBitWriter.WriteUInt16(source, 16, this.m_data, this.m_bitLength);
			this.m_bitLength += 16;
		}

		[CLSCompliant(false)]
		public void Write(ushort source, int numberOfBits)
		{
			this.EnsureBufferSize(this.m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt16(source, numberOfBits, this.m_data, this.m_bitLength);
			this.m_bitLength += numberOfBits;
		}

		public void Write(short source)
		{
			this.EnsureBufferSize(this.m_bitLength + 16);
			NetBitWriter.WriteUInt16((ushort)source, 16, this.m_data, this.m_bitLength);
			this.m_bitLength += 16;
		}

		public void Write(int source)
		{
			this.EnsureBufferSize(this.m_bitLength + 32);
			NetBitWriter.WriteUInt32((uint)source, 32, this.m_data, this.m_bitLength);
			this.m_bitLength += 32;
		}

		[CLSCompliant(false)]
		public void Write(uint source)
		{
			this.EnsureBufferSize(this.m_bitLength + 32);
			NetBitWriter.WriteUInt32(source, 32, this.m_data, this.m_bitLength);
			this.m_bitLength += 32;
		}

		[CLSCompliant(false)]
		public void Write(uint source, int numberOfBits)
		{
			this.EnsureBufferSize(this.m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt32(source, numberOfBits, this.m_data, this.m_bitLength);
			this.m_bitLength += numberOfBits;
		}

		public void Write(int source, int numberOfBits)
		{
			this.EnsureBufferSize(this.m_bitLength + numberOfBits);
			if (numberOfBits != 32)
			{
				int signBit = 1 << numberOfBits - 1;
				if (source < 0)
				{
					source = (-source - 1) | signBit;
				}
				else
				{
					source &= ~signBit;
				}
			}
			NetBitWriter.WriteUInt32((uint)source, numberOfBits, this.m_data, this.m_bitLength);
			this.m_bitLength += numberOfBits;
		}

		[CLSCompliant(false)]
		public void Write(ulong source)
		{
			this.EnsureBufferSize(this.m_bitLength + 64);
			NetBitWriter.WriteUInt64(source, 64, this.m_data, this.m_bitLength);
			this.m_bitLength += 64;
		}

		[CLSCompliant(false)]
		public void Write(ulong source, int numberOfBits)
		{
			this.EnsureBufferSize(this.m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt64(source, numberOfBits, this.m_data, this.m_bitLength);
			this.m_bitLength += numberOfBits;
		}

		public void Write(long source)
		{
			this.EnsureBufferSize(this.m_bitLength + 64);
			NetBitWriter.WriteUInt64((ulong)source, 64, this.m_data, this.m_bitLength);
			this.m_bitLength += 64;
		}

		public void Write(long source, int numberOfBits)
		{
			this.EnsureBufferSize(this.m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt64((ulong)source, numberOfBits, this.m_data, this.m_bitLength);
			this.m_bitLength += numberOfBits;
		}

		public void Write(float source)
		{
			SingleUIntUnion su;
			su.UIntValue = 0U;
			su.SingleValue = source;
			this.Write(su.UIntValue);
		}

		public void Write(double source)
		{
			byte[] val = BitConverter.GetBytes(source);
			this.Write(val);
		}

		[CLSCompliant(false)]
		public int WriteVariableUInt32(uint value)
		{
			int retval = 1;
			uint num = value;
			while (num >= 128U)
			{
				this.Write((byte)(num | 128U));
				num >>= 7;
				retval++;
			}
			this.Write((byte)num);
			return retval;
		}

		public int WriteVariableInt32(int value)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			return this.WriteVariableUInt32(zigzag);
		}

		public int WriteVariableInt64(long value)
		{
			ulong zigzag = (ulong)((value << 1) ^ (value >> 63));
			return this.WriteVariableUInt64(zigzag);
		}

		[CLSCompliant(false)]
		public int WriteVariableUInt64(ulong value)
		{
			int retval = 1;
			ulong num = value;
			while (num >= 128UL)
			{
				this.Write((byte)(num | 128UL));
				num >>= 7;
				retval++;
			}
			this.Write((byte)num);
			return retval;
		}

		public void WriteSignedSingle(float value, int numberOfBits)
		{
			float unit = (value + 1f) * 0.5f;
			int maxVal = (1 << numberOfBits) - 1;
			uint writeVal = (uint)(unit * (float)maxVal);
			this.Write(writeVal, numberOfBits);
		}

		public void WriteUnitSingle(float value, int numberOfBits)
		{
			int maxValue = (1 << numberOfBits) - 1;
			uint writeVal = (uint)(value * (float)maxValue);
			this.Write(writeVal, numberOfBits);
		}

		public void WriteRangedSingle(float value, float min, float max, int numberOfBits)
		{
			float range = max - min;
			float unit = (value - min) / range;
			int maxVal = (1 << numberOfBits) - 1;
			this.Write((uint)((float)maxVal * unit), numberOfBits);
		}

		public int WriteRangedInteger(int min, int max, int value)
		{
			uint range = (uint)(max - min);
			int numBits = NetUtility.BitsToHoldUInt(range);
			uint rvalue = (uint)(value - min);
			this.Write(rvalue, numBits);
			return numBits;
		}

		public void Write(string source)
		{
			if (string.IsNullOrEmpty(source))
			{
				this.EnsureBufferSize(this.m_bitLength + 8);
				this.WriteVariableUInt32(0U);
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(source);
			this.EnsureBufferSize(this.m_bitLength + 8 + bytes.Length * 8);
			this.WriteVariableUInt32((uint)bytes.Length);
			this.Write(bytes);
		}

		public void Write(IPEndPoint endPoint)
		{
			byte[] bytes = endPoint.Address.GetAddressBytes();
			this.Write((byte)bytes.Length);
			this.Write(bytes);
			this.Write((ushort)endPoint.Port);
		}

		public void WriteTime(bool highPrecision)
		{
			double localTime = NetTime.Now;
			if (highPrecision)
			{
				this.Write(localTime);
				return;
			}
			this.Write((float)localTime);
		}

		public void WriteTime(double localTime, bool highPrecision)
		{
			if (highPrecision)
			{
				this.Write(localTime);
				return;
			}
			this.Write((float)localTime);
		}

		public void WritePadBits()
		{
			this.m_bitLength = (this.m_bitLength + 7 >> 3) * 8;
			this.EnsureBufferSize(this.m_bitLength);
		}

		public void WritePadBits(int numberOfBits)
		{
			this.m_bitLength += numberOfBits;
			this.EnsureBufferSize(this.m_bitLength);
		}

		public void Write(NetOutgoingMessage message)
		{
			this.EnsureBufferSize(this.m_bitLength + message.LengthBytes * 8);
			this.Write(message.m_data, 0, message.LengthBytes);
			int bitsInLastByte = message.m_bitLength % 8;
			if (bitsInLastByte != 0)
			{
				int excessBits = 8 - bitsInLastByte;
				this.m_bitLength -= excessBits;
			}
		}

		public void Write(NetIncomingMessage message)
		{
			this.EnsureBufferSize(this.m_bitLength + message.LengthBytes * 8);
			this.Write(message.m_data, 0, message.LengthBytes);
			int bitsInLastByte = message.m_bitLength % 8;
			if (bitsInLastByte != 0)
			{
				int excessBits = 8 - bitsInLastByte;
				this.m_bitLength -= excessBits;
			}
		}

		public byte[] Data
		{
			get
			{
				return this.m_data;
			}
			set
			{
				this.m_data = value;
			}
		}

		public int LengthBytes
		{
			get
			{
				return this.m_bitLength + 7 >> 3;
			}
			set
			{
				this.m_bitLength = value * 8;
				this.InternalEnsureBufferSize(this.m_bitLength);
			}
		}

		public int LengthBits
		{
			get
			{
				return this.m_bitLength;
			}
			set
			{
				this.m_bitLength = value;
				this.InternalEnsureBufferSize(this.m_bitLength);
			}
		}

		public long Position
		{
			get
			{
				return (long)this.m_readPosition;
			}
			set
			{
				this.m_readPosition = (int)value;
			}
		}

		public int PositionInBytes
		{
			get
			{
				return this.m_readPosition / 8;
			}
		}

		static NetBuffer()
		{
			typeof(byte).Assembly.GetTypes();
			NetBuffer.s_readMethods = new Dictionary<Type, MethodInfo>();
			MethodInfo[] methods = typeof(NetIncomingMessage).GetMethods(BindingFlags.Instance | BindingFlags.Public);
			foreach (MethodInfo mi in methods)
			{
				if (mi.GetParameters().Length == 0 && mi.Name.StartsWith("Read", StringComparison.InvariantCulture) && mi.Name.Substring(4) == mi.ReturnType.Name)
				{
					NetBuffer.s_readMethods[mi.ReturnType] = mi;
				}
			}
			NetBuffer.s_writeMethods = new Dictionary<Type, MethodInfo>();
			methods = typeof(NetOutgoingMessage).GetMethods(BindingFlags.Instance | BindingFlags.Public);
			foreach (MethodInfo mi2 in methods)
			{
				if (mi2.Name.Equals("Write", StringComparison.InvariantCulture))
				{
					ParameterInfo[] pis = mi2.GetParameters();
					if (pis.Length == 1)
					{
						NetBuffer.s_writeMethods[pis[0].ParameterType] = mi2;
					}
				}
			}
		}

		public void WriteAllFields(object ob)
		{
			this.WriteAllFields(ob, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public void WriteAllFields(object ob, BindingFlags flags)
		{
			if (ob == null)
			{
				return;
			}
			Type tp = ob.GetType();
			FieldInfo[] fields = tp.GetFields(flags);
			NetUtility.SortMembersList(fields);
			foreach (FieldInfo fi in fields)
			{
				object value = fi.GetValue(ob);
				MethodInfo writeMethod;
				if (!NetBuffer.s_writeMethods.TryGetValue(fi.FieldType, out writeMethod))
				{
					throw new NetException("Failed to find write method for type " + fi.FieldType);
				}
				writeMethod.Invoke(this, new object[] { value });
			}
		}

		public void WriteAllProperties(object ob)
		{
			this.WriteAllProperties(ob, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public void WriteAllProperties(object ob, BindingFlags flags)
		{
			if (ob == null)
			{
				return;
			}
			Type tp = ob.GetType();
			PropertyInfo[] fields = tp.GetProperties(flags);
			NetUtility.SortMembersList(fields);
			foreach (PropertyInfo fi in fields)
			{
				MethodInfo getMethod = fi.GetGetMethod((flags & BindingFlags.NonPublic) == BindingFlags.NonPublic);
				object value = getMethod.Invoke(ob, null);
				MethodInfo writeMethod;
				if (NetBuffer.s_writeMethods.TryGetValue(fi.PropertyType, out writeMethod))
				{
					writeMethod.Invoke(this, new object[] { value });
				}
			}
		}

		private const string c_readOverflowError = "Trying to read past the buffer size - likely caused by mismatching Write/Reads, different size or order.";

		protected const int c_overAllocateAmount = 4;

		private static readonly Dictionary<Type, MethodInfo> s_readMethods;

		private static readonly Dictionary<Type, MethodInfo> s_writeMethods;

		internal byte[] m_data;

		internal int m_bitLength;

		internal int m_readPosition;
	}
}
