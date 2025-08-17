using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace DNA.Net.Lidgren
{
	public static class XNAExtensions
	{
		public static void Write(this NetBuffer message, Point value)
		{
			message.Write(value.X);
			message.Write(value.Y);
		}

		public static Point ReadPoint(this NetBuffer message)
		{
			return new Point(message.ReadInt32(), message.ReadInt32());
		}

		public static void WriteHalfPrecision(this NetBuffer message, float value)
		{
			message.Write(new HalfSingle(value).PackedValue);
		}

		public static float ReadHalfPrecisionSingle(this NetBuffer message)
		{
			HalfSingle halfSingle = default(HalfSingle);
			halfSingle.PackedValue = message.ReadUInt16();
			return halfSingle.ToSingle();
		}

		public static void Write(this NetBuffer message, Vector2 vector)
		{
			message.Write(vector.X);
			message.Write(vector.Y);
		}

		public static Vector2 ReadVector2(this NetBuffer message)
		{
			Vector2 vector;
			vector.X = message.ReadSingle();
			vector.Y = message.ReadSingle();
			return vector;
		}

		public static void Write(this NetBuffer message, Vector3 vector)
		{
			message.Write(vector.X);
			message.Write(vector.Y);
			message.Write(vector.Z);
		}

		public static void WriteHalfPrecision(this NetBuffer message, Vector3 vector)
		{
			message.Write(new HalfSingle(vector.X).PackedValue);
			message.Write(new HalfSingle(vector.Y).PackedValue);
			message.Write(new HalfSingle(vector.Z).PackedValue);
		}

		public static Vector3 ReadVector3(this NetBuffer message)
		{
			Vector3 vector;
			vector.X = message.ReadSingle();
			vector.Y = message.ReadSingle();
			vector.Z = message.ReadSingle();
			return vector;
		}

		public static Vector3 ReadHalfPrecisionVector3(this NetBuffer message)
		{
			HalfSingle halfSingle = default(HalfSingle);
			halfSingle.PackedValue = message.ReadUInt16();
			HalfSingle halfSingle2 = default(HalfSingle);
			halfSingle2.PackedValue = message.ReadUInt16();
			HalfSingle halfSingle3 = default(HalfSingle);
			halfSingle3.PackedValue = message.ReadUInt16();
			Vector3 vector;
			vector.X = halfSingle.ToSingle();
			vector.Y = halfSingle2.ToSingle();
			vector.Z = halfSingle3.ToSingle();
			return vector;
		}

		public static void Write(this NetBuffer message, Vector4 vector)
		{
			message.Write(vector.X);
			message.Write(vector.Y);
			message.Write(vector.Z);
			message.Write(vector.W);
		}

		public static Vector4 ReadVector4(this NetBuffer message)
		{
			Vector4 vector;
			vector.X = message.ReadSingle();
			vector.Y = message.ReadSingle();
			vector.Z = message.ReadSingle();
			vector.W = message.ReadSingle();
			return vector;
		}

		public static void WriteUnitVector3(this NetBuffer message, Vector3 unitVector, int numberOfBits)
		{
			float x = unitVector.X;
			float y = unitVector.Y;
			float z = unitVector.Z;
			double num = 0.3183098861837907;
			float num2 = (float)(Math.Atan2((double)x, (double)y) * num);
			float num3 = (float)(Math.Atan2((double)z, Math.Sqrt((double)(x * x + y * y))) * (num * 2.0));
			int num4 = numberOfBits / 2;
			message.WriteSignedSingle(num2, num4);
			message.WriteSignedSingle(num3, numberOfBits - num4);
		}

		public static Vector3 ReadUnitVector3(this NetBuffer message, int numberOfBits)
		{
			int num = numberOfBits / 2;
			float num2 = message.ReadSignedSingle(num) * 3.1415927f;
			float num3 = message.ReadSignedSingle(numberOfBits - num) * 1.5707964f;
			Vector3 vector;
			vector.X = (float)(Math.Sin((double)num2) * Math.Cos((double)num3));
			vector.Y = (float)(Math.Cos((double)num2) * Math.Cos((double)num3));
			vector.Z = (float)Math.Sin((double)num3);
			return vector;
		}

		public static void WriteRotation(this NetBuffer message, Quaternion quaternion, int bitsPerElement)
		{
			if (quaternion.X > 1f)
			{
				quaternion.X = 1f;
			}
			if (quaternion.Y > 1f)
			{
				quaternion.Y = 1f;
			}
			if (quaternion.Z > 1f)
			{
				quaternion.Z = 1f;
			}
			if (quaternion.W > 1f)
			{
				quaternion.W = 1f;
			}
			if (quaternion.X < -1f)
			{
				quaternion.X = -1f;
			}
			if (quaternion.Y < -1f)
			{
				quaternion.Y = -1f;
			}
			if (quaternion.Z < -1f)
			{
				quaternion.Z = -1f;
			}
			if (quaternion.W < -1f)
			{
				quaternion.W = -1f;
			}
			message.WriteSignedSingle(quaternion.X, bitsPerElement);
			message.WriteSignedSingle(quaternion.Y, bitsPerElement);
			message.WriteSignedSingle(quaternion.Z, bitsPerElement);
			message.WriteSignedSingle(quaternion.W, bitsPerElement);
		}

		public static Quaternion ReadRotation(this NetBuffer message, int bitsPerElement)
		{
			Quaternion quaternion;
			quaternion.X = message.ReadSignedSingle(bitsPerElement);
			quaternion.Y = message.ReadSignedSingle(bitsPerElement);
			quaternion.Z = message.ReadSignedSingle(bitsPerElement);
			quaternion.W = message.ReadSignedSingle(bitsPerElement);
			return quaternion;
		}

		public static void WriteMatrix(this NetBuffer message, ref Matrix matrix)
		{
			Quaternion quaternion = Quaternion.CreateFromRotationMatrix(matrix);
			message.WriteRotation(quaternion, 24);
			message.Write(matrix.M41);
			message.Write(matrix.M42);
			message.Write(matrix.M43);
		}

		public static void WriteMatrix(this NetBuffer message, Matrix matrix)
		{
			Quaternion quaternion = Quaternion.CreateFromRotationMatrix(matrix);
			message.WriteRotation(quaternion, 24);
			message.Write(matrix.M41);
			message.Write(matrix.M42);
			message.Write(matrix.M43);
		}

		public static Matrix ReadMatrix(this NetBuffer message)
		{
			Quaternion quaternion = message.ReadRotation(24);
			Matrix matrix = Matrix.CreateFromQuaternion(quaternion);
			matrix.M41 = message.ReadSingle();
			matrix.M42 = message.ReadSingle();
			matrix.M43 = message.ReadSingle();
			return matrix;
		}

		public static void ReadMatrix(this NetBuffer message, ref Matrix destination)
		{
			Quaternion quaternion = message.ReadRotation(24);
			destination = Matrix.CreateFromQuaternion(quaternion);
			destination.M41 = message.ReadSingle();
			destination.M42 = message.ReadSingle();
			destination.M43 = message.ReadSingle();
		}

		public static void Write(this NetBuffer message, BoundingSphere bounds)
		{
			message.Write(bounds.Center.X);
			message.Write(bounds.Center.Y);
			message.Write(bounds.Center.Z);
			message.Write(bounds.Radius);
		}

		public static BoundingSphere ReadBoundingSphere(this NetBuffer message)
		{
			BoundingSphere boundingSphere;
			boundingSphere.Center.X = message.ReadSingle();
			boundingSphere.Center.Y = message.ReadSingle();
			boundingSphere.Center.Z = message.ReadSingle();
			boundingSphere.Radius = message.ReadSingle();
			return boundingSphere;
		}
	}
}
