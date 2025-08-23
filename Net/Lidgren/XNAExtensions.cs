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
			HalfSingle h = default(HalfSingle);
			h.PackedValue = message.ReadUInt16();
			return h.ToSingle();
		}

		public static void Write(this NetBuffer message, Vector2 vector)
		{
			message.Write(vector.X);
			message.Write(vector.Y);
		}

		public static Vector2 ReadVector2(this NetBuffer message)
		{
			Vector2 retval;
			retval.X = message.ReadSingle();
			retval.Y = message.ReadSingle();
			return retval;
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
			Vector3 retval;
			retval.X = message.ReadSingle();
			retval.Y = message.ReadSingle();
			retval.Z = message.ReadSingle();
			return retval;
		}

		public static Vector3 ReadHalfPrecisionVector3(this NetBuffer message)
		{
			HalfSingle hx = default(HalfSingle);
			hx.PackedValue = message.ReadUInt16();
			HalfSingle hy = default(HalfSingle);
			hy.PackedValue = message.ReadUInt16();
			HalfSingle hz = default(HalfSingle);
			hz.PackedValue = message.ReadUInt16();
			Vector3 retval;
			retval.X = hx.ToSingle();
			retval.Y = hy.ToSingle();
			retval.Z = hz.ToSingle();
			return retval;
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
			Vector4 retval;
			retval.X = message.ReadSingle();
			retval.Y = message.ReadSingle();
			retval.Z = message.ReadSingle();
			retval.W = message.ReadSingle();
			return retval;
		}

		public static void WriteUnitVector3(this NetBuffer message, Vector3 unitVector, int numberOfBits)
		{
			float x = unitVector.X;
			float y = unitVector.Y;
			float z = unitVector.Z;
			double invPi = 0.3183098861837907;
			float phi = (float)(Math.Atan2((double)x, (double)y) * invPi);
			float theta = (float)(Math.Atan2((double)z, Math.Sqrt((double)(x * x + y * y))) * (invPi * 2.0));
			int halfBits = numberOfBits / 2;
			message.WriteSignedSingle(phi, halfBits);
			message.WriteSignedSingle(theta, numberOfBits - halfBits);
		}

		public static Vector3 ReadUnitVector3(this NetBuffer message, int numberOfBits)
		{
			int halfBits = numberOfBits / 2;
			float phi = message.ReadSignedSingle(halfBits) * 3.1415927f;
			float theta = message.ReadSignedSingle(numberOfBits - halfBits) * 1.5707964f;
			Vector3 retval;
			retval.X = (float)(Math.Sin((double)phi) * Math.Cos((double)theta));
			retval.Y = (float)(Math.Cos((double)phi) * Math.Cos((double)theta));
			retval.Z = (float)Math.Sin((double)theta);
			return retval;
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
			Quaternion retval;
			retval.X = message.ReadSignedSingle(bitsPerElement);
			retval.Y = message.ReadSignedSingle(bitsPerElement);
			retval.Z = message.ReadSignedSingle(bitsPerElement);
			retval.W = message.ReadSignedSingle(bitsPerElement);
			return retval;
		}

		public static void WriteMatrix(this NetBuffer message, ref Matrix matrix)
		{
			Quaternion rot = Quaternion.CreateFromRotationMatrix(matrix);
			message.WriteRotation(rot, 24);
			message.Write(matrix.M41);
			message.Write(matrix.M42);
			message.Write(matrix.M43);
		}

		public static void WriteMatrix(this NetBuffer message, Matrix matrix)
		{
			Quaternion rot = Quaternion.CreateFromRotationMatrix(matrix);
			message.WriteRotation(rot, 24);
			message.Write(matrix.M41);
			message.Write(matrix.M42);
			message.Write(matrix.M43);
		}

		public static Matrix ReadMatrix(this NetBuffer message)
		{
			Quaternion rot = message.ReadRotation(24);
			Matrix retval = Matrix.CreateFromQuaternion(rot);
			retval.M41 = message.ReadSingle();
			retval.M42 = message.ReadSingle();
			retval.M43 = message.ReadSingle();
			return retval;
		}

		public static void ReadMatrix(this NetBuffer message, ref Matrix destination)
		{
			Quaternion rot = message.ReadRotation(24);
			destination = Matrix.CreateFromQuaternion(rot);
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
			BoundingSphere retval;
			retval.Center.X = message.ReadSingle();
			retval.Center.Y = message.ReadSingle();
			retval.Center.Z = message.ReadSingle();
			retval.Radius = message.ReadSingle();
			return retval;
		}
	}
}
