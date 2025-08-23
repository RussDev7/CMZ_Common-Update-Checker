using System;
using System.IO;
using DNA.IO;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public struct Triangle3D
	{
		public Vector3 Normal
		{
			get
			{
				return Vector3.Cross(this._b - this._a, this._c - this._a);
			}
		}

		public LineF3D AB
		{
			get
			{
				return new LineF3D(this._a, this._b);
			}
		}

		public LineF3D AC
		{
			get
			{
				return new LineF3D(this._a, this._c);
			}
		}

		public LineF3D BC
		{
			get
			{
				return new LineF3D(this._b, this._c);
			}
		}

		public Vector3 A
		{
			get
			{
				return this._a;
			}
			set
			{
				this._a = value;
			}
		}

		public Vector3 B
		{
			get
			{
				return this._b;
			}
			set
			{
				this._b = value;
			}
		}

		public Vector3 C
		{
			get
			{
				return this._c;
			}
			set
			{
				this._c = value;
			}
		}

		public Vector3 Centroid
		{
			get
			{
				return (this._a + this._b + this._c) / 3f;
			}
		}

		public float Area
		{
			get
			{
				Vector3 ab = this._a - this._b;
				Vector3 bc = this._c - this._b;
				return Vector3.Cross(ab, bc).Length();
			}
		}

		public BoundingBox GetBoundingBox()
		{
			Vector3 min = new Vector3(Math.Min(Math.Min(this._a.X, this._b.X), this._c.X), Math.Min(Math.Min(this._a.Y, this._b.Y), this._c.Y), Math.Min(Math.Min(this._a.Z, this._b.Z), this._c.Z));
			Vector3 max = new Vector3(Math.Max(Math.Max(this._a.X, this._b.X), this._c.X), Math.Max(Math.Max(this._a.Y, this._b.Y), this._c.Y), Math.Max(Math.Max(this._a.Z, this._b.Z), this._c.Z));
			return new BoundingBox(min, max);
		}

		public BoundingSphere GetBoundingSphere()
		{
			BoundingBox box = this.GetBoundingBox();
			Vector3 center = box.Min + (box.Max - box.Min) / 2f;
			float da = (this._a - center).LengthSquared();
			float db = (this._b - center).LengthSquared();
			float dc = (this._c - center).LengthSquared();
			float radius = (float)Math.Sqrt((double)Math.Max(Math.Max(da, db), dc));
			return new BoundingSphere(center, radius);
		}

		public static Triangle3D Transform(Triangle3D triangle, Matrix matrix)
		{
			return new Triangle3D(Vector3.Transform(triangle._a, matrix), Vector3.Transform(triangle._b, matrix), Vector3.Transform(triangle._c, matrix));
		}

		public static Triangle3D Read(BinaryReader reader)
		{
			return new Triangle3D(reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3());
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(this.A);
			writer.Write(this.B);
			writer.Write(this.C);
		}

		public Triangle3D(Vector3 a, Vector3 b, Vector3 c)
		{
			this._a = a;
			this._b = b;
			this._c = c;
		}

		public Vector3 BarycentricCoordinate(Vector3 point)
		{
			Vector3 baryOut = default(Vector3);
			Vector3 u = this._b - this._a;
			Vector3 v = this._c - this._a;
			Vector3 w = point - this._a;
			Vector3 a = Vector3.Cross(u, w);
			Vector3 b = Vector3.Cross(v, w);
			float denom = 1f / Vector3.Cross(u, v).Length();
			baryOut.Y = b.Length() * denom;
			baryOut.Z = a.Length() * denom;
			baryOut.X = 1f - baryOut.Y - baryOut.Z;
			return baryOut;
		}

		public Plane GetPlane()
		{
			return new Plane(this.B, this.A, this.C);
		}

		public float? Intersects(Ray ray)
		{
			Vector3 u = this._b - this._a;
			Vector3 v = this._c - this._a;
			Vector3 i = Vector3.Cross(u, v);
			Vector3 dir = ray.Direction;
			Vector3 w0 = ray.Position - this._a;
			float a = -Vector3.Dot(i, w0);
			float b = Vector3.Dot(i, dir);
			if (b == 0f)
			{
				return null;
			}
			float t = a / b;
			if (t < 0f)
			{
				return null;
			}
			Vector3 I = ray.Position + t * dir;
			float uu = Vector3.Dot(u, u);
			float uv = Vector3.Dot(u, v);
			float vv = Vector3.Dot(v, v);
			Vector3 w = I - this._a;
			float wu = Vector3.Dot(w, u);
			float wv = Vector3.Dot(w, v);
			float D = uv * uv - uu * vv;
			float sI = (uv * wv - vv * wu) / D;
			if ((double)sI < 0.0 || (double)sI > 1.0)
			{
				return null;
			}
			float tI = (uv * wu - uu * wv) / D;
			if ((double)tI < 0.0 || (double)(sI + tI) > 1.0)
			{
				return null;
			}
			return new float?(t);
		}

		public Triangle3D[] SliceHorizontal(float yValue, int precisionDigits)
		{
			Plane plane = DrawingTools.PlaneFromPointNormal(new Vector3(0f, yValue, 0f), new Vector3(0f, 1f, 0f));
			LineF3D ab = this.AB;
			LineF3D ac = this.AC;
			LineF3D bc = this.BC;
			float t;
			bool parallel;
			if (ab.Intersects(plane, out t, out parallel, precisionDigits))
			{
				if (parallel)
				{
					return new Triangle3D[] { this };
				}
				if (t == 0f)
				{
					float t2;
					if (!bc.Intersects(plane, out t2, out parallel, precisionDigits))
					{
						return new Triangle3D[] { this };
					}
					if (t2 == 0f || t2 == 1f)
					{
						return new Triangle3D[] { this };
					}
					Vector3 newPoint2 = bc.GetValue(t2);
					newPoint2.Y = yValue;
					return new Triangle3D[]
					{
						new Triangle3D(this._a, this._b, newPoint2),
						new Triangle3D(this._a, newPoint2, this._c)
					};
				}
				else if (t == 1f)
				{
					float t2;
					if (!ac.Intersects(plane, out t2, out parallel, precisionDigits))
					{
						return new Triangle3D[] { this };
					}
					Vector3 newPoint2 = ac.GetValue(t2);
					newPoint2.Y = yValue;
					if (t2 == 0f || t2 == 1f)
					{
						return new Triangle3D[]
						{
							new Triangle3D(ac.GetValue(0f), this._b, newPoint2)
						};
					}
					return new Triangle3D[]
					{
						new Triangle3D(this._b, this._c, newPoint2),
						new Triangle3D(this._b, newPoint2, this._a)
					};
				}
				else
				{
					Vector3 newPoint3 = ab.GetValue(t);
					newPoint3.Y = yValue;
					float t2;
					if (ac.Intersects(plane, out t2, out parallel, precisionDigits))
					{
						if (t2 == 1f)
						{
							return new Triangle3D[]
							{
								new Triangle3D(newPoint3, this._b, this._c),
								new Triangle3D(newPoint3, this._c, this._a)
							};
						}
						Vector3 newPoint2 = ac.GetValue(t2);
						newPoint2.Y = yValue;
						return new Triangle3D[]
						{
							new Triangle3D(newPoint2, this._a, newPoint3),
							new Triangle3D(newPoint3, this._b, this._c),
							new Triangle3D(newPoint2, newPoint3, this._c)
						};
					}
					else
					{
						if (bc.Intersects(plane, out t2, out parallel, precisionDigits))
						{
							Vector3 newPoint2 = bc.GetValue(t2);
							newPoint2.Y = yValue;
							return new Triangle3D[]
							{
								new Triangle3D(newPoint3, this._b, newPoint2),
								new Triangle3D(this._a, newPoint3, newPoint2),
								new Triangle3D(this._a, newPoint2, this._c)
							};
						}
						throw new Exception("Slice Error");
					}
				}
			}
			else
			{
				if (!bc.Intersects(plane, out t, out parallel, precisionDigits))
				{
					return new Triangle3D[] { this };
				}
				if (t == 1f)
				{
					return new Triangle3D[] { this };
				}
				Vector3 newPoint3 = bc.GetValue(t);
				newPoint3.Y = yValue;
				float t2;
				if (ac.Intersects(plane, out t2, out parallel, precisionDigits))
				{
					Vector3 newPoint2 = ac.GetValue(t2);
					newPoint2.Y = yValue;
					return new Triangle3D[]
					{
						new Triangle3D(newPoint2, this._a, newPoint3),
						new Triangle3D(newPoint3, this._b, this._a),
						new Triangle3D(newPoint2, newPoint3, this._c)
					};
				}
				throw new Exception("Slice Error");
			}
		}

		public Triangle3D[] SliceVertical(float xValue, int precisionDigits)
		{
			Plane plane = DrawingTools.PlaneFromPointNormal(new Vector3(xValue, 0f, 0f), new Vector3(1f, 0f, 0f));
			LineF3D ab = this.AB;
			LineF3D ac = this.AC;
			LineF3D bc = this.BC;
			float t;
			bool parallel;
			if (ab.Intersects(plane, out t, out parallel, precisionDigits))
			{
				if (parallel)
				{
					return new Triangle3D[] { this };
				}
				if (t == 0f)
				{
					float t2;
					if (!bc.Intersects(plane, out t2, out parallel, precisionDigits))
					{
						return new Triangle3D[] { this };
					}
					if (t2 == 0f || t2 == 1f)
					{
						return new Triangle3D[] { this };
					}
					Vector3 newPoint2 = bc.GetValue(t2);
					newPoint2.X = xValue;
					return new Triangle3D[]
					{
						new Triangle3D(this._a, this._b, newPoint2),
						new Triangle3D(this._a, newPoint2, this._c)
					};
				}
				else if (t == 1f)
				{
					float t2;
					if (!ac.Intersects(plane, out t2, out parallel, precisionDigits))
					{
						return new Triangle3D[] { this };
					}
					Vector3 newPoint2 = ac.GetValue(t2);
					newPoint2.X = xValue;
					if (t2 == 0f || t2 == 1f)
					{
						return new Triangle3D[]
						{
							new Triangle3D(ac.GetValue(0f), this._b, newPoint2)
						};
					}
					return new Triangle3D[]
					{
						new Triangle3D(this._b, this._c, newPoint2),
						new Triangle3D(this._b, newPoint2, this._a)
					};
				}
				else
				{
					Vector3 newPoint3 = ab.GetValue(t);
					newPoint3.X = xValue;
					float t2;
					if (ac.Intersects(plane, out t2, out parallel, precisionDigits))
					{
						if (t2 == 1f)
						{
							return new Triangle3D[]
							{
								new Triangle3D(newPoint3, this._b, this._c),
								new Triangle3D(newPoint3, this._c, this._a)
							};
						}
						Vector3 newPoint2 = ac.GetValue(t2);
						newPoint2.X = xValue;
						return new Triangle3D[]
						{
							new Triangle3D(newPoint2, this._a, newPoint3),
							new Triangle3D(newPoint3, this._b, this._c),
							new Triangle3D(newPoint2, newPoint3, this._c)
						};
					}
					else
					{
						if (bc.Intersects(plane, out t2, out parallel, precisionDigits))
						{
							Vector3 newPoint2 = bc.GetValue(t2);
							newPoint2.X = xValue;
							return new Triangle3D[]
							{
								new Triangle3D(newPoint3, this._b, newPoint2),
								new Triangle3D(this._a, newPoint3, newPoint2),
								new Triangle3D(this._a, newPoint2, this._c)
							};
						}
						throw new Exception("Slice Error");
					}
				}
			}
			else
			{
				if (!bc.Intersects(plane, out t, out parallel, precisionDigits))
				{
					return new Triangle3D[] { this };
				}
				if (t == 1f)
				{
					return new Triangle3D[] { this };
				}
				Vector3 newPoint3 = bc.GetValue(t);
				newPoint3.X = xValue;
				float t2;
				if (ac.Intersects(plane, out t2, out parallel, precisionDigits))
				{
					Vector3 newPoint2 = ac.GetValue(t2);
					newPoint2.X = xValue;
					return new Triangle3D[]
					{
						new Triangle3D(newPoint2, this._a, newPoint3),
						new Triangle3D(newPoint3, this._b, this._a),
						new Triangle3D(newPoint2, newPoint3, this._c)
					};
				}
				throw new Exception("Slice Error");
			}
		}

		public Triangle3D[] Slice(Plane plane, int precisionDigits)
		{
			plane.DotCoordinate(this.A);
			plane.DotCoordinate(this.B);
			plane.DotCoordinate(this.C);
			LineF3D ab = this.AB;
			LineF3D ac = this.AC;
			LineF3D bc = this.BC;
			float t;
			bool parallel;
			if (ab.Intersects(plane, out t, out parallel, precisionDigits))
			{
				if (parallel)
				{
					return new Triangle3D[] { this };
				}
				if (t == 0f)
				{
					float t2;
					if (!bc.Intersects(plane, out t2, out parallel, precisionDigits))
					{
						return new Triangle3D[] { this };
					}
					if (parallel)
					{
						throw new PrecisionException();
					}
					if (t2 == 0f || t2 == 1f)
					{
						return new Triangle3D[] { this };
					}
					Vector3 newPoint2 = bc.GetValue(t2);
					return new Triangle3D[]
					{
						new Triangle3D(this._a, this._b, newPoint2),
						new Triangle3D(this._a, newPoint2, this._c)
					};
				}
				else if (t == 1f)
				{
					float t2;
					if (!ac.Intersects(plane, out t2, out parallel, precisionDigits))
					{
						return new Triangle3D[] { this };
					}
					if (parallel)
					{
						throw new PrecisionException();
					}
					Vector3 newPoint2 = ac.GetValue(t2);
					if (t2 == 0f || t2 == 1f)
					{
						return new Triangle3D[]
						{
							new Triangle3D(ac.GetValue(0f), this._b, newPoint2)
						};
					}
					return new Triangle3D[]
					{
						new Triangle3D(this._b, this._c, newPoint2),
						new Triangle3D(this._b, newPoint2, this._a)
					};
				}
				else
				{
					Vector3 newPoint3 = ab.GetValue(t);
					float t2;
					if (ac.Intersects(plane, out t2, out parallel, precisionDigits))
					{
						if (t2 <= 0f || parallel)
						{
							throw new PrecisionException();
						}
						if (t2 == 1f)
						{
							return new Triangle3D[]
							{
								new Triangle3D(newPoint3, this._b, this._c),
								new Triangle3D(newPoint3, this._c, this._a)
							};
						}
						Vector3 newPoint2 = ac.GetValue(t2);
						return new Triangle3D[]
						{
							new Triangle3D(newPoint2, this._a, newPoint3),
							new Triangle3D(newPoint3, this._b, this._c),
							new Triangle3D(newPoint2, newPoint3, this._c)
						};
					}
					else
					{
						if (!bc.Intersects(plane, out t2, out parallel, precisionDigits))
						{
							throw new PrecisionException();
						}
						if (t2 <= 0f || t2 >= 1f || parallel)
						{
							throw new PrecisionException();
						}
						Vector3 newPoint2 = bc.GetValue(t2);
						return new Triangle3D[]
						{
							new Triangle3D(newPoint3, this._b, newPoint2),
							new Triangle3D(this._a, newPoint3, newPoint2),
							new Triangle3D(this._a, newPoint2, this._c)
						};
					}
				}
			}
			else
			{
				if (!bc.Intersects(plane, out t, out parallel, precisionDigits))
				{
					return new Triangle3D[] { this };
				}
				if (t == 0f || parallel)
				{
					throw new PrecisionException();
				}
				if (t == 1f)
				{
					return new Triangle3D[] { this };
				}
				Vector3 newPoint3 = bc.GetValue(t);
				float t2;
				if (!ac.Intersects(plane, out t2, out parallel, precisionDigits))
				{
					throw new PrecisionException();
				}
				if (t2 <= 0f || t2 >= 1f || parallel)
				{
					throw new PrecisionException();
				}
				Vector3 newPoint2 = ac.GetValue(t2);
				return new Triangle3D[]
				{
					new Triangle3D(newPoint2, this._a, newPoint3),
					new Triangle3D(newPoint3, this._a, this._b),
					new Triangle3D(newPoint2, newPoint3, this._c)
				};
			}
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.A.ToString(),
				"-",
				this.B.ToString(),
				"-",
				this.C.ToString()
			});
		}

		private Vector3 _a;

		private Vector3 _b;

		private Vector3 _c;
	}
}
