using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.Collision
{
	public class BSPTree : CollisionMap
	{
		public BSPTree()
		{
			this._root = new BSPTree.BSPTreeNode();
		}

		public BSPTree(Model model, int percisionDigits)
		{
			this.Build(model, percisionDigits);
		}

		public void Build(IList<Triangle3D> polys, int percisionDigits)
		{
			LinkedList<Triangle3D> triList = new LinkedList<Triangle3D>();
			int degenTriangles = 0;
			foreach (Triangle3D tri in polys)
			{
				double area = Math.Round((double)tri.Area, percisionDigits);
				if (area != 0.0)
				{
					triList.AddLast(tri);
				}
				else
				{
					degenTriangles++;
				}
			}
			this._percisionDigits = percisionDigits;
			this._root = new BSPTree.BSPTreeNode();
			this._root.Build(triList, percisionDigits);
		}

		public void Build(Model model, int percisionDigits)
		{
			List<Triangle3D> polys = model.ExtractModelTris(true);
			this.Build(polys, percisionDigits);
		}

		protected override CollisionMap.RayQueryResult? CollidesWith(Ray ray, float min, float max)
		{
			return this._root.CollidesWith(ray, min, max, this._percisionDigits);
		}

		public override void FindCollisions(Ellipsoid ellipsoid, List<CollisionMap.ContactPoint> contacts)
		{
			contacts.Clear();
			this._root.FindCollisions(ellipsoid, contacts);
		}

		public void AppendCollisions(Ellipsoid ellipsoid, List<CollisionMap.ContactPoint> contacts)
		{
			this._root.FindCollisions(ellipsoid, contacts);
		}

		public override void FindCollisions(BoundingSphere sphere, List<CollisionMap.ContactPoint> contacts)
		{
			contacts.Clear();
			this._root.FindCollisions(sphere, contacts);
		}

		public override void Save(BinaryWriter writer)
		{
			writer.Write(this.FileID);
			writer.Write(1);
			writer.Write(this._percisionDigits);
			this._root.Save(writer);
		}

		public override void Load(BinaryReader reader)
		{
			string id = reader.ReadString();
			if (id != this.FileID)
			{
				throw new Exception("Bad BSP file Format");
			}
			int verison = reader.ReadInt32();
			if (verison != 1)
			{
				throw new Exception("Bad BSP version");
			}
			this._percisionDigits = reader.ReadInt32();
			this._root = BSPTree.BSPTreeNode.Load(reader);
		}

		public void GetData(List<Plane> planes, List<Triangle3D> triangles)
		{
			this._root.GetData(planes, triangles);
		}

		public override void GetTriangles(List<Triangle3D> tris)
		{
			this.GetData(new List<Plane>(), tris);
		}

		private const int Version = 1;

		private BSPTree.BSPTreeNode _root = new BSPTree.BSPTreeNode();

		private int _percisionDigits;

		private readonly string FileID = "TASBSP";

		public class BSPTreeReader : ContentTypeReader<BSPTree>
		{
			protected override BSPTree Read(ContentReader input, BSPTree existingInstance)
			{
				if (existingInstance != null)
				{
					existingInstance.Load(input);
					return existingInstance;
				}
				BSPTree tree = new BSPTree();
				tree.Load(input);
				return tree;
			}
		}

		private class BSPTreeNode
		{
			private static void ChooseDividingPolygon(LinkedList<Triangle3D> splitList)
			{
				if (splitList.Count < 3)
				{
					return;
				}
				LinkedListNode<Triangle3D>[] nodes = new LinkedListNode<Triangle3D>[splitList.Count];
				Triangle3D[] triList = new Triangle3D[splitList.Count];
				LinkedListNode<Triangle3D> currentNode = splitList.First;
				for (int i = 0; i < triList.Length; i++)
				{
					triList[i] = currentNode.Value;
					nodes[i] = currentNode;
					currentNode = currentNode.Next;
				}
				int totalProcessors = Environment.ProcessorCount;
				if (triList.Length < totalProcessors * 500)
				{
					totalProcessors = 1;
				}
				int triPreProc = triList.Length / totalProcessors + 1;
				BSPTree.BSPTreeNode.SplitCounts bestPolygon = null;
				float minRelation = 0.8f;
				bool isConvex = true;
				while (bestPolygon == null)
				{
					float bestRatio = float.MinValue;
					int leastSplits = int.MaxValue;
					List<BSPTree.BSPTreeNode.BestSplitFinder> finders = new List<BSPTree.BSPTreeNode.BestSplitFinder>();
					int startPoly = 0;
					for (int processor = 0; processor < totalProcessors; processor++)
					{
						int polyStop = startPoly + triPreProc;
						if (polyStop > triList.Length)
						{
							polyStop = triList.Length;
						}
						BSPTree.BSPTreeNode.BestSplitFinder finder = new BSPTree.BSPTreeNode.BestSplitFinder(triList, minRelation, startPoly, polyStop);
						finder.FindSplitAsync();
						finders.Add(finder);
						startPoly = polyStop + 1;
					}
					foreach (BSPTree.BSPTreeNode.BestSplitFinder finder2 in finders)
					{
						finder2.EndFindSplit();
						if (!finder2.isConvex)
						{
							isConvex = false;
						}
						if (finder2.bestPolygon != null)
						{
							BSPTree.BSPTreeNode.SplitCounts counts = finder2.bestPolygon;
							if (counts.Spanning < leastSplits || (counts.Spanning == leastSplits && counts.DivisionFactor > bestRatio))
							{
								bestPolygon = counts;
								leastSplits = counts.Spanning;
								bestRatio = counts.DivisionFactor;
							}
						}
					}
					minRelation /= 2f;
					if (isConvex)
					{
						return;
					}
				}
				splitList.Remove(nodes[bestPolygon.Node]);
				splitList.AddFirst(nodes[bestPolygon.Node]);
			}

			public static void SortList(Plane splitPlane, LinkedList<Triangle3D> triList, LinkedList<Triangle3D> frontList, LinkedList<Triangle3D> backList, int percisionDigits)
			{
				LinkedListNode<Triangle3D> currentNode = triList.First;
				while (currentNode != null)
				{
					int tempPercision = percisionDigits;
					bool retry = true;
					while (retry)
					{
						bool remove = false;
						Triangle3D tri = currentNode.Value;
						double d = (double)splitPlane.DotCoordinate(tri.A);
						double d2 = (double)splitPlane.DotCoordinate(tri.B);
						double d3 = (double)splitPlane.DotCoordinate(tri.C);
						d = Math.Round(d, tempPercision);
						d2 = Math.Round(d2, tempPercision);
						d3 = Math.Round(d3, tempPercision);
						if (d == 0.0 && d2 == 0.0 && d3 == 0.0)
						{
							Plane p2 = new Plane(tri.B, tri.A, tri.C);
							if (Vector3.Dot(splitPlane.Normal, p2.Normal) > 0f)
							{
								frontList.AddFirst(tri);
							}
							else
							{
								backList.AddLast(tri);
							}
							remove = true;
						}
						else if (d <= 0.0 && d2 <= 0.0 && d3 <= 0.0)
						{
							remove = true;
							backList.AddLast(tri);
						}
						else
						{
							if (d >= 0.0 && d2 >= 0.0)
							{
								if (d3 >= 0.0)
								{
									goto IL_01C4;
								}
							}
							try
							{
								Triangle3D[] tris = tri.Slice(splitPlane, tempPercision);
								if (tris.Length == 1)
								{
									throw new PrecisionException();
								}
								for (int i = 0; i < tris.Length; i++)
								{
									if (Math.Round((double)tris[i].Area, percisionDigits - 1) > 0.0)
									{
										triList.AddLast(tris[i]);
									}
								}
							}
							catch (PrecisionException)
							{
								tempPercision--;
								if (tempPercision < 0)
								{
									throw new Exception("Slice Error");
								}
								continue;
							}
							remove = true;
						}
						IL_01C4:
						LinkedListNode<Triangle3D> nextNode = currentNode.Next;
						if (remove)
						{
							triList.Remove(currentNode);
						}
						currentNode = nextNode;
						retry = false;
					}
				}
			}

			public void Build(LinkedList<Triangle3D> splitList, int percisionDigits)
			{
				LinkedList<Triangle3D> frontList = new LinkedList<Triangle3D>();
				LinkedList<Triangle3D> backList = new LinkedList<Triangle3D>();
				BSPTree.BSPTreeNode.ChooseDividingPolygon(splitList);
				frontList.AddFirst(splitList.First.Value);
				Triangle3D triangle = splitList.First.Value;
				splitList.RemoveFirst();
				this.SplitPlane = new Plane(triangle.B, triangle.A, triangle.C);
				this.SplitPlane.Normalize();
				BSPTree.BSPTreeNode.SortList(this.SplitPlane, splitList, frontList, backList, percisionDigits);
				this.TriList = new Triangle3D[frontList.Count];
				frontList.CopyTo(this.TriList, 0);
				Thread workerThread = null;
				bool doFrontBuild = false;
				bool doBackBuild = false;
				if (splitList.Count > 0)
				{
					this.Front = new BSPTree.BSPTreeNode();
					if (workerThread == null && splitList.Count > 1000 && splitList.Count >= backList.Count && BSPTree.BSPTreeNode.AvailibleThread > 0)
					{
						doFrontBuild = false;
						lock (BSPTree.BSPTreeNode.threadLock)
						{
							BSPTree.BSPTreeNode.AvailibleThread--;
						}
						workerThread = new Thread(delegate(object state)
						{
							object[] parms = (object[])state;
							BSPTree.BSPTreeNode node = (BSPTree.BSPTreeNode)parms[0];
							node.Build((LinkedList<Triangle3D>)parms[1], (int)parms[2]);
						});
						workerThread.Name = "BSP Front Side Worker Thread";
						workerThread.Start(new object[] { this.Front, splitList, percisionDigits });
					}
					else
					{
						doFrontBuild = true;
					}
				}
				if (backList.Count > 0)
				{
					this.Back = new BSPTree.BSPTreeNode();
					if (workerThread == null && backList.Count > 1000 && backList.Count >= splitList.Count && BSPTree.BSPTreeNode.AvailibleThread > 0)
					{
						doBackBuild = false;
						lock (BSPTree.BSPTreeNode.threadLock)
						{
							BSPTree.BSPTreeNode.AvailibleThread--;
						}
						workerThread = new Thread(delegate(object state)
						{
							object[] parms2 = (object[])state;
							BSPTree.BSPTreeNode node2 = (BSPTree.BSPTreeNode)parms2[0];
							node2.Build((LinkedList<Triangle3D>)parms2[1], (int)parms2[2]);
						});
						workerThread.Name = "BSP Front Side Worker Thread";
						workerThread.Start(new object[] { this.Back, backList, percisionDigits });
					}
					else
					{
						doBackBuild = true;
					}
				}
				if (doFrontBuild)
				{
					this.Front.Build(splitList, percisionDigits);
				}
				if (doBackBuild)
				{
					this.Back.Build(backList, percisionDigits);
				}
				if (workerThread != null)
				{
					workerThread.Join();
					lock (BSPTree.BSPTreeNode.threadLock)
					{
						BSPTree.BSPTreeNode.AvailibleThread++;
					}
				}
			}

			public static BSPTree.BSPTreeNode Load(BinaryReader reader)
			{
				int tris = reader.ReadInt32();
				if (tris == 0)
				{
					return null;
				}
				BSPTree.BSPTreeNode ret = new BSPTree.BSPTreeNode();
				ret.TriList = new Triangle3D[tris];
				for (int i = 0; i < tris; i++)
				{
					ret.TriList[i] = Triangle3D.Read(reader);
				}
				Triangle3D triangle = ret.TriList[ret.TriList.Length - 1];
				ret.SplitPlane = new Plane(triangle.B, triangle.A, triangle.C);
				ret.SplitPlane.Normalize();
				ret.Front = BSPTree.BSPTreeNode.Load(reader);
				ret.Back = BSPTree.BSPTreeNode.Load(reader);
				return ret;
			}

			public void Save(BinaryWriter writer)
			{
				if (this.TriList == null)
				{
					writer.Write(0);
					return;
				}
				writer.Write(this.TriList.Length);
				foreach (Triangle3D tri in this.TriList)
				{
					tri.Write(writer);
				}
				if (this.Front == null)
				{
					writer.Write(0);
				}
				else
				{
					this.Front.Save(writer);
				}
				if (this.Back == null)
				{
					writer.Write(0);
					return;
				}
				this.Back.Save(writer);
			}

			private Triangle3D? CollideWithTriangles(Ray ray)
			{
				for (int i = 0; i < this.TriList.Length; i++)
				{
					if (this.TriList[i].Intersects(ray) != null)
					{
						return new Triangle3D?(this.TriList[i]);
					}
				}
				return null;
			}

			public CollisionMap.RayQueryResult? CollidesWith(Ray ray, float minT, float maxT, int percisionDigits)
			{
				Vector3 rayStart = ray.Position + ray.Direction * minT;
				float distanceToPlane = this.SplitPlane.DotCoordinate(rayStart);
				float? tval = ray.Intersects(this.SplitPlane);
				if (tval != null)
				{
					tval = new float?((float)Math.Round((double)tval.Value, percisionDigits));
				}
				if (tval == null || tval.Value < minT || tval.Value > maxT)
				{
					if (distanceToPlane >= 0f)
					{
						if (this.Front == null)
						{
							return null;
						}
						return this.Front.CollidesWith(ray, minT, maxT, percisionDigits);
					}
					else
					{
						if (this.Back == null)
						{
							return null;
						}
						return this.Back.CollidesWith(ray, minT, maxT, percisionDigits);
					}
				}
				else if (distanceToPlane >= 0f)
				{
					CollisionMap.RayQueryResult? result = null;
					if (this.Front != null)
					{
						result = this.Front.CollidesWith(ray, minT, Math.Min(maxT, tval.Value), percisionDigits);
					}
					if (result != null)
					{
						return result;
					}
					Triangle3D? colTri = this.CollideWithTriangles(ray);
					if (colTri != null)
					{
						return new CollisionMap.RayQueryResult?(new CollisionMap.RayQueryResult(tval.Value, colTri.Value));
					}
					if (this.Back == null)
					{
						return null;
					}
					return this.Back.CollidesWith(ray, Math.Max(minT, tval.Value), maxT, percisionDigits);
				}
				else
				{
					CollisionMap.RayQueryResult? result2 = null;
					if (this.Back != null)
					{
						result2 = this.Back.CollidesWith(ray, minT, Math.Min(maxT, tval.Value), percisionDigits);
					}
					if (result2 != null)
					{
						return result2;
					}
					if (this.Front == null)
					{
						return null;
					}
					return this.Front.CollidesWith(ray, Math.Max(minT, tval.Value), maxT, percisionDigits);
				}
			}

			private void CollideWithTriangles(Ellipsoid ellipsoid, float distanceToPlane, List<CollisionMap.ContactPoint> contacts)
			{
				Vector3 planeNormal = this.SplitPlane.Normal;
				float sphereRadSqu = ellipsoid.Radius.X * planeNormal.X * ellipsoid.Radius.X * planeNormal.X + ellipsoid.Radius.Y * planeNormal.Y * ellipsoid.Radius.Y * planeNormal.Y + ellipsoid.Radius.Z * planeNormal.Z * ellipsoid.Radius.Z * planeNormal.Z;
				float distToPlaneSquared = distanceToPlane * distanceToPlane;
				for (int i = 0; i < this.TriList.Length; i++)
				{
					Triangle3D tri = this.TriList[i];
					Vector3 w0 = ellipsoid.Center - tri.A;
					Vector3.Dot(planeNormal, w0);
					Vector3 dt = distanceToPlane * planeNormal;
					Vector3 intersectionPoint = ellipsoid.Center + dt;
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
						if ((double)tI >= 0.0 && (double)(sI + tI) <= 1.0 && distToPlaneSquared <= sphereRadSqu)
						{
							float ellipseRad = (float)Math.Sqrt((double)sphereRadSqu);
							Vector3 pVect = (ellipseRad - distanceToPlane) * planeNormal;
							contacts.Add(new CollisionMap.ContactPoint(pVect, tri));
						}
					}
				}
			}

			private void CollideWithTriangles(Ellipsoid ellipsoid, Plane localPlane, float distanceToLocalPlane, List<CollisionMap.ContactPoint> contacts)
			{
				Triangle3D localTri = default(Triangle3D);
				for (int i = 0; i < this.TriList.Length; i++)
				{
					ellipsoid.TransformWorldSpaceTriToUnitSphereSpace(this.TriList[i], ref localTri);
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
							float penetration = ellipsoid.CalculateWorldSpacePenetration(intersectionPoint);
							contacts.Add(new CollisionMap.ContactPoint(this.SplitPlane.Normal, this.TriList[i], penetration));
							return;
						}
					}
				}
				Vector3 colNormal = Vector3.Zero;
				for (int j = 0; j < this.TriList.Length; j++)
				{
					ellipsoid.TransformWorldSpaceTriToUnitSphereSpace(this.TriList[j], ref localTri);
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
						float penetration2 = ellipsoid.CalculateWorldSpacePenetration(colNormal);
						colNormal = ellipsoid.TransformUnitSphereSpaceVectorToWorldSpace(Vector3.Negate(colNormal));
						contacts.Add(new CollisionMap.ContactPoint(colNormal, this.TriList[j], penetration2));
					}
				}
			}

			public void FindCollisions(Ellipsoid ellipsoid, List<CollisionMap.ContactPoint> contacts)
			{
				Plane localPlane = ellipsoid.TransformWorldSpacePlaneToUnitSphereSpace(this.SplitPlane);
				float distanceToPlane = localPlane.D;
				if (distanceToPlane >= 0f)
				{
					if (this.Front != null)
					{
						this.Front.FindCollisions(ellipsoid, contacts);
					}
					if (distanceToPlane <= 1f)
					{
						this.CollideWithTriangles(ellipsoid, localPlane, distanceToPlane, contacts);
						if (this.Back != null)
						{
							this.Back.FindCollisions(ellipsoid, contacts);
							return;
						}
					}
				}
				else
				{
					if (this.Back != null)
					{
						this.Back.FindCollisions(ellipsoid, contacts);
					}
					if (distanceToPlane >= -1f && this.Front != null)
					{
						this.Front.FindCollisions(ellipsoid, contacts);
					}
				}
			}

			public void FindCollisions(BoundingSphere sphere, List<CollisionMap.ContactPoint> contacts)
			{
				float distanceToPlane = this.SplitPlane.DotCoordinate(sphere.Center);
				if (distanceToPlane >= 0f)
				{
					if (this.Front != null)
					{
						this.Front.FindCollisions(sphere, contacts);
					}
					if (distanceToPlane <= sphere.Radius)
					{
						this.CollideWithTriangles(sphere, distanceToPlane, contacts);
						if (this.Back != null)
						{
							this.Back.FindCollisions(sphere, contacts);
							return;
						}
					}
				}
				else
				{
					if (this.Back != null)
					{
						this.Back.FindCollisions(sphere, contacts);
					}
					if (distanceToPlane >= -sphere.Radius && this.Front != null)
					{
						this.Front.FindCollisions(sphere, contacts);
					}
				}
			}

			private void CollideWithTriangles(BoundingSphere sphere, float distanceToPlane, List<CollisionMap.ContactPoint> contacts)
			{
				Vector3 planeNormal = this.SplitPlane.Normal;
				float distToPlaneSquared = distanceToPlane * distanceToPlane;
				float sphereRadSqu = sphere.Radius * sphere.Radius;
				for (int i = 0; i < this.TriList.Length; i++)
				{
					Triangle3D tri = this.TriList[i];
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
						if ((double)tI >= 0.0 && (double)(sI + tI) <= 1.0 && distToPlaneSquared <= sphereRadSqu)
						{
							Vector3 pVect = (sphere.Radius - distanceToPlane) * planeNormal;
							contacts.Add(new CollisionMap.ContactPoint(pVect, tri));
						}
					}
				}
			}

			public void GetData(List<Plane> planes, List<Triangle3D> triangles)
			{
				planes.Add(this.SplitPlane);
				if (this.TriList != null)
				{
					triangles.AddRange(this.TriList);
				}
				if (this.Front != null)
				{
					this.Front.GetData(planes, triangles);
				}
				if (this.Back != null)
				{
					this.Back.GetData(planes, triangles);
				}
			}

			public BSPTree.BSPTreeNode Front;

			public BSPTree.BSPTreeNode Back;

			public Plane SplitPlane;

			public Triangle3D[] TriList;

			public static int AvailibleThread = (Environment.ProcessorCount - 1) * 15;

			public static object threadLock = new object();

			private class SplitCounts
			{
				public float DivisionFactor
				{
					get
					{
						if (this.Positive > this.Negative)
						{
							return (float)this.Negative / (float)this.Positive;
						}
						if (this.Positive < this.Negative)
						{
							return (float)this.Positive / (float)this.Negative;
						}
						return 1f;
					}
				}

				public SplitCounts(int node, int positive, int negative, int spanning, int coincident)
				{
					this.Node = node;
					this.Positive = positive;
					this.Negative = negative;
					this.Spanning = spanning;
					this.Coincident = coincident;
				}

				public int Node;

				public int Positive;

				public int Negative;

				public int Spanning;

				public int Coincident;
			}

			private class BestSplitFinder
			{
				public BestSplitFinder(Triangle3D[] tlist, float minr, int sPoly, int epoly)
				{
					this.triList = tlist;
					this.minRelation = minr;
					this.startPoly = sPoly;
					this.endPoly = epoly;
				}

				public void FindSplitAsync()
				{
					if (this.startPoly == 0 && this.endPoly >= this.triList.Length)
					{
						this.FindSplit();
						return;
					}
					this._thread = new Thread(new ThreadStart(this.FindSplit));
					this._thread.Name = "Find Poly Split Worker " + this.startPoly.ToString() + "-" + this.endPoly.ToString();
					this._thread.Start();
				}

				public void EndFindSplit()
				{
					if (this._thread != null)
					{
						this._thread.Join();
					}
				}

				public void FindSplit()
				{
					for (int otri = this.startPoly; otri < this.endPoly; otri++)
					{
						int numPositive = 0;
						int numNegative = 0;
						int numSplits = 0;
						int numCoincident = 0;
						Plane plane = this.triList[otri].GetPlane();
						for (int itri = 0; itri < this.triList.Length; itri++)
						{
							if (otri != itri)
							{
								Triangle3D tri2 = this.triList[itri];
								int front = 0;
								int back = 0;
								float d = plane.DotCoordinate(tri2.A);
								if (d > 0f)
								{
									front++;
								}
								else if (d < 0f)
								{
									back++;
								}
								d = plane.DotCoordinate(tri2.B);
								if (d > 0f)
								{
									front++;
								}
								else if (d < 0f)
								{
									back++;
								}
								d = plane.DotCoordinate(tri2.C);
								if (d > 0f)
								{
									front++;
								}
								else if (d < 0f)
								{
									back++;
								}
								if (front > 0 && back == 0)
								{
									numPositive++;
								}
								else if (front == 0 && back > 0)
								{
									numNegative++;
								}
								else if (front != 0 || back != 0)
								{
									numSplits++;
								}
								else
								{
									numCoincident++;
								}
							}
						}
						BSPTree.BSPTreeNode.SplitCounts counts = new BSPTree.BSPTreeNode.SplitCounts(otri, numPositive, numNegative, numSplits, numCoincident);
						if (numNegative > 0 || numSplits > 0)
						{
							this.isConvex = false;
						}
						if (counts.DivisionFactor >= this.minRelation && (counts.Spanning < this.leastSplits || (counts.Spanning == this.leastSplits && counts.DivisionFactor > this.bestRatio)))
						{
							this.bestPolygon = counts;
							this.leastSplits = counts.Spanning;
							this.bestRatio = counts.DivisionFactor;
						}
					}
				}

				private int startPoly;

				private int endPoly;

				private float minRelation;

				public float bestRatio = float.MinValue;

				public int leastSplits = int.MaxValue;

				public BSPTree.BSPTreeNode.SplitCounts bestPolygon;

				public bool isConvex;

				private Triangle3D[] triList;

				private Thread _thread;
			}
		}
	}
}
