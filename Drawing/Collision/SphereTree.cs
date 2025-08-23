using System;
using System.Collections.Generic;
using System.IO;
using DNA.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.Collision
{
	public class SphereTree : CollisionMap
	{
		public SphereTree(Model model)
		{
			List<Triangle3D> tris = model.ExtractModelTris(false);
			this.Build(tris);
		}

		public SphereTree()
		{
			this._root = new SphereTree.SphereTreeNode();
		}

		public void Build(IList<Triangle3D> tris)
		{
			List<Vector3> points = new List<Vector3>();
			for (int i = 0; i < tris.Count; i++)
			{
				points.Add(tris[i].A);
				points.Add(tris[i].B);
				points.Add(tris[i].C);
			}
			BoundingBox bounds = BoundingBox.CreateFromPoints(points);
			Vector3 size = bounds.Max - bounds.Min;
			float largestSide = Math.Max(size.X, Math.Max(size.Y, size.Z));
			Vector3 center = bounds.Min + size / 2f;
			Vector3 newSize = new Vector3(largestSide, largestSide, largestSide);
			new BoundingBox(center - newSize / 2f, center + newSize / 2f);
			float floatdinc = largestSide / 128f;
			int gridSize = 128;
			SphereTree.SphereTreeNode[,,] nodes = new SphereTree.SphereTreeNode[gridSize, gridSize, gridSize];
			List<SphereTree.SphereTreeNode> nodeList = new List<SphereTree.SphereTreeNode>();
			foreach (Triangle3D tri in tris)
			{
				Vector3 centroid = tri.Centroid - bounds.Min;
				int xIndex = (int)(centroid.X / floatdinc);
				int yIndex = (int)(centroid.Y / floatdinc);
				int zIndex = (int)(centroid.Z / floatdinc);
				if (nodes[xIndex, yIndex, zIndex] == null)
				{
					nodeList.Add(nodes[xIndex, yIndex, zIndex] = new SphereTree.SphereTreeNode());
				}
				nodes[xIndex, yIndex, zIndex].Triangles.Add(tri);
			}
			foreach (SphereTree.SphereTreeNode node in nodeList)
			{
				node.RefineBoundingSphere();
			}
			SphereTree.SphereTreeNode[,,] newNodes;
			do
			{
				gridSize /= 2;
				newNodes = new SphereTree.SphereTreeNode[gridSize, gridSize, gridSize];
				nodeList.Clear();
				for (int x = 0; x < gridSize; x++)
				{
					for (int y = 0; y < gridSize; y++)
					{
						for (int z = 0; z < gridSize; z++)
						{
							SphereTree.SphereTreeNode node2 = new SphereTree.SphereTreeNode();
							bool isLeaf = true;
							for (int j = 0; j < 8; j++)
							{
								node2.Children[j] = nodes[x * 2 + ((j & 4) >> 2), y * 2 + ((j & 2) >> 1), z * 2 + (j & 1)];
								if (node2.Children[j] != null)
								{
									isLeaf = false;
								}
							}
							if (!isLeaf)
							{
								newNodes[x, y, z] = node2;
								nodeList.Add(node2);
							}
						}
					}
				}
				foreach (SphereTree.SphereTreeNode node3 in nodeList)
				{
					node3.RefineBoundingSphere();
				}
				nodes = newNodes;
			}
			while (gridSize > 1);
			this._root = newNodes[0, 0, 0];
		}

		public override void FindCollisions(Ellipsoid ellipsoid, List<CollisionMap.ContactPoint> contacts)
		{
			contacts.Clear();
			BoundingSphere sphere = ellipsoid.GetBoundingSphere();
			this._root.FindCollisions(sphere, ellipsoid, contacts);
		}

		public override void FindCollisions(BoundingSphere sphere, List<CollisionMap.ContactPoint> contacts)
		{
			contacts.Clear();
			this._root.FindCollisions(sphere, contacts);
		}

		public override void GetTriangles(List<Triangle3D> tris)
		{
			tris.Clear();
			this._root.GetTriangles(tris);
		}

		public override void Save(BinaryWriter writer)
		{
			writer.Write(this.FileID);
			writer.Write(1);
			this._root.Save(writer);
		}

		public override void Load(BinaryReader reader)
		{
			string id = reader.ReadString();
			if (id != this.FileID)
			{
				throw new Exception("Bad SPT file Format");
			}
			int verison = reader.ReadInt32();
			if (verison != 1)
			{
				throw new Exception("Bad SPT version");
			}
			this._root = SphereTree.SphereTreeNode.Load(reader);
		}

		protected override CollisionMap.RayQueryResult? CollidesWith(Ray ray, float min, float max)
		{
			return this._root.CollidesWith(ray, min, max);
		}

		private const float mergeBias = 0.5f;

		private const int GridSize = 128;

		private const int Version = 1;

		public SphereTree.SphereTreeNode _root;

		private readonly string FileID = "TASSPT";

		public class SphereTreeReader : ContentTypeReader<SphereTree>
		{
			protected override SphereTree Read(ContentReader input, SphereTree existingInstance)
			{
				if (existingInstance != null)
				{
					existingInstance.Load(input);
					return existingInstance;
				}
				SphereTree tree = new SphereTree();
				tree.Load(input);
				return tree;
			}
		}

		public class SphereTreeNode
		{
			public bool IsLeaf
			{
				get
				{
					for (int i = 0; i < this.Children.Length; i++)
					{
						if (this.Children[i] != null)
						{
							return false;
						}
					}
					return true;
				}
			}

			public SphereTreeNode()
			{
			}

			public static SphereTree.SphereTreeNode Load(BinaryReader reader)
			{
				Vector3 sphereCenter = reader.ReadVector3();
				float radius = reader.ReadSingle();
				SphereTree.SphereTreeNode node = new SphereTree.SphereTreeNode(new BoundingSphere(sphereCenter, radius));
				int tris = reader.ReadInt32();
				node.Triangles = new List<Triangle3D>(tris);
				for (int i = 0; i < tris; i++)
				{
					node.Triangles.Add(Triangle3D.Read(reader));
				}
				byte mask = reader.ReadByte();
				for (int j = 0; j < 8; j++)
				{
					if (((int)mask & (1 << j)) != 0)
					{
						node.Children[j] = SphereTree.SphereTreeNode.Load(reader);
					}
				}
				return node;
			}

			public void Save(BinaryWriter writer)
			{
				writer.Write(this.Sphere.Center);
				writer.Write(this.Sphere.Radius);
				writer.Write(this.Triangles.Count);
				foreach (Triangle3D tri in this.Triangles)
				{
					tri.Write(writer);
				}
				byte mask = 0;
				for (int i = 0; i < 8; i++)
				{
					if (this.Children[i] != null)
					{
						mask |= (byte)(1 << i);
					}
				}
				writer.Write(mask);
				for (int j = 0; j < 8; j++)
				{
					if (this.Children[j] != null)
					{
						this.Children[j].Save(writer);
					}
				}
			}

			public SphereTreeNode(BoundingSphere sphere, IList<Triangle3D> tris)
			{
				this.Sphere = sphere;
				this.Triangles.AddRange(tris);
			}

			public SphereTreeNode(BoundingSphere sphere, Triangle3D tri)
			{
				this.Sphere = sphere;
				this.Triangles.Add(tri);
			}

			public SphereTreeNode(BoundingSphere sphere)
			{
				this.Sphere = sphere;
			}

			public void FindCollisions(BoundingSphere sphere, List<CollisionMap.ContactPoint> contacts)
			{
				if (!this.Sphere.Intersects(sphere))
				{
					return;
				}
				for (int i = 0; i < this.Children.Length; i++)
				{
					if (this.Children[i] != null)
					{
						this.Children[i].FindCollisions(sphere, contacts);
					}
				}
				for (int j = 0; j < this.Triangles.Count; j++)
				{
					Triangle3D tri = this.Triangles[j];
					Plane plane = tri.GetPlane();
					Vector3 planeNormal = tri.Normal;
					float distanceToPlane = Math.Abs(plane.DotCoordinate(sphere.Center));
					Vector3 w0 = sphere.Center - tri.A;
					Vector3.Dot(planeNormal, w0);
					Vector3 dt = distanceToPlane * planeNormal;
					Vector3 intersectionPoint = sphere.Center + dt;
					Vector3 u = tri.B - tri.A;
					Vector3 v = tri.C - tri.A;
					float uu = Vector3.Dot(u, u);
					float uv = Vector3.Dot(u, v);
					float vv = Vector3.Dot(v, v);
					Vector3 w = intersectionPoint - tri.A;
					float wu = Vector3.Dot(w, u);
					float wv = Vector3.Dot(w, v);
					float D = uv * uv - uu * vv;
					float sI = (uv * wv - vv * wu) / D;
					if ((double)sI >= 0.0 && (double)sI <= 1.0)
					{
						float tI = (uv * wu - uu * wv) / D;
						if ((double)tI >= 0.0 && (double)(sI + tI) <= 1.0 && distanceToPlane <= sphere.Radius)
						{
							Vector3 pVect = (sphere.Radius - distanceToPlane) * planeNormal;
							contacts.Add(new CollisionMap.ContactPoint(pVect, tri));
						}
					}
				}
			}

			public void FindCollisions(BoundingSphere sphere, Ellipsoid ellipsoid, List<CollisionMap.ContactPoint> contacts)
			{
				if (!this.Sphere.Intersects(sphere))
				{
					return;
				}
				for (int i = 0; i < this.Children.Length; i++)
				{
					if (this.Children[i] != null)
					{
						this.Children[i].FindCollisions(sphere, ellipsoid, contacts);
					}
				}
				Triangle3D localTri = default(Triangle3D);
				for (int j = 0; j < this.Triangles.Count; j++)
				{
					Plane triPLane = this.Triangles[j].GetPlane();
					ellipsoid.TransformWorldSpaceTriToUnitSphereSpace(this.Triangles[j], ref localTri);
					Plane localPlane = ellipsoid.TransformWorldSpacePlaneToUnitSphereSpace(triPLane);
					float distanceToLocalPlane = localPlane.D;
					if (distanceToLocalPlane <= 1f)
					{
						Vector3.Dot(localPlane.Normal, localTri.A);
						Vector3 intersectionPoint = -distanceToLocalPlane * localPlane.Normal;
						Vector3 u = localTri.B - localTri.A;
						Vector3 v = localTri.C - localTri.A;
						float uu = Vector3.Dot(u, u);
						float uv = Vector3.Dot(u, v);
						float vv = Vector3.Dot(v, v);
						Vector3 w = intersectionPoint - localTri.A;
						float wu = Vector3.Dot(w, u);
						float wv = Vector3.Dot(w, v);
						float D = uv * uv - uu * vv;
						float sI = (uv * wv - vv * wu) / D;
						if ((double)sI >= 0.0 && (double)sI <= 1.0)
						{
							float tI = (uv * wu - uu * wv) / D;
							if ((double)tI >= 0.0 && (double)(sI + tI) <= 1.0)
							{
								contacts.Add(new CollisionMap.ContactPoint(localPlane.Normal, this.Triangles[j]));
								return;
							}
						}
						Vector3 colNormal = Vector3.Zero;
						LineF3D lineAB = new LineF3D(localTri.A, localTri.B);
						LineF3D lineAC = new LineF3D(localTri.A, localTri.C);
						LineF3D lineBC = new LineF3D(localTri.B, localTri.C);
						Vector3 p = lineAB.ClosetPointTo(Vector3.Zero);
						Vector3 p2 = lineAC.ClosetPointTo(Vector3.Zero);
						Vector3 p3 = lineBC.ClosetPointTo(Vector3.Zero);
						float dd = p.LengthSquared();
						float dd2 = p2.LengthSquared();
						float dd3 = p3.LengthSquared();
						bool foundCollsion = false;
						float closestDist = 1f;
						if (dd <= closestDist)
						{
							closestDist = dd;
							colNormal = p;
							foundCollsion = true;
						}
						if (dd2 <= closestDist)
						{
							closestDist = dd2;
							colNormal = p2;
							foundCollsion = true;
						}
						if (dd3 <= closestDist)
						{
							colNormal = p3;
							foundCollsion = true;
						}
						if (foundCollsion)
						{
							colNormal = ellipsoid.TransformUnitSphereSpaceVectorToWorldSpace(Vector3.Negate(colNormal));
							contacts.Add(new CollisionMap.ContactPoint(colNormal, this.Triangles[j]));
						}
					}
				}
			}

			public static bool RaySphereIntersection(BoundingSphere sphere, Ray ray, out float t1out, out float t2out)
			{
				Vector3 diff = ray.Position - sphere.Center;
				float a0 = Vector3.Dot(diff, diff) - sphere.Radius * sphere.Radius;
				float a = Vector3.Dot(ray.Direction, diff);
				t1out = float.MinValue;
				t2out = float.MaxValue;
				float discr;
				if (a0 <= 0f)
				{
					discr = a * a - a0;
					float root = (float)Math.Sqrt((double)discr);
					t1out = float.MinValue;
					t2out = -a + root;
					return true;
				}
				if (a >= 0f)
				{
					return false;
				}
				discr = a * a - a0;
				if (discr < 0f)
				{
					return false;
				}
				if (discr > 0f)
				{
					float root = (float)Math.Sqrt((double)discr);
					t1out = -a - root;
					t2out = -a + root;
					if (t1out > t2out)
					{
						float temp = t1out;
						t1out = t2out;
						t2out = temp;
					}
					return true;
				}
				t2out = (t1out = -a);
				return true;
			}

			public CollisionMap.RayQueryResult? CollidesWith(Ray ray, float minT, float maxT)
			{
				float t;
				float t2;
				if (!SphereTree.SphereTreeNode.RaySphereIntersection(this.Sphere, ray, out t, out t2))
				{
					return null;
				}
				if (t >= 0f && t > maxT)
				{
					return null;
				}
				CollisionMap.RayQueryResult? ret = null;
				for (int i = 0; i < this.Children.Length; i++)
				{
					if (this.Children[i] != null)
					{
						CollisionMap.RayQueryResult? result = this.Children[i].CollidesWith(ray, minT, maxT);
						if (result != null && result.Value.T < maxT)
						{
							maxT = result.Value.T;
							ret = result;
						}
					}
				}
				for (int j = 0; j < this.Triangles.Count; j++)
				{
					SphereTree.SphereTreeNode.RayTriangleTests++;
					float? tval = this.Triangles[j].Intersects(ray);
					if (tval != null)
					{
						float t3 = tval.Value;
						if (t3 >= minT && t3 < maxT)
						{
							maxT = t3;
							ret = new CollisionMap.RayQueryResult?(new CollisionMap.RayQueryResult(t3, this.Triangles[j]));
						}
					}
				}
				return ret;
			}

			public void RefineBoundingSphere()
			{
				List<Vector3> points = new List<Vector3>();
				this.GetPoints(points);
				BoundingSphere sphere = BoundingSphere.CreateFromPoints(points);
				this.Sphere = sphere;
			}

			public void GetPoints(List<Vector3> points)
			{
				foreach (Triangle3D tri in this.Triangles)
				{
					points.Add(tri.A);
					points.Add(tri.B);
					points.Add(tri.C);
				}
				foreach (SphereTree.SphereTreeNode node in this.Children)
				{
					if (node != null)
					{
						node.GetPoints(points);
					}
				}
			}

			public void GetTriangles(List<Triangle3D> tris)
			{
				tris.AddRange(this.Triangles);
				foreach (SphereTree.SphereTreeNode node in this.Children)
				{
					if (node != null)
					{
						node.GetTriangles(tris);
					}
				}
			}

			public BoundingSphere Sphere;

			public SphereTree.SphereTreeNode[] Children = new SphereTree.SphereTreeNode[8];

			public List<Triangle3D> Triangles = new List<Triangle3D>();

			public static int RayTriangleTests;
		}
	}
}
