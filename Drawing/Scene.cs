using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DNA.Drawing.Lights;
using DNA.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class Scene : Entity
	{
		public Scene()
		{
			this._lightsReadOnly = this._lights.AsReadOnly();
		}

		public ReadOnlyCollection<Light> Lights
		{
			get
			{
				return this._lightsReadOnly;
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			double totalSeconds = gameTime.ElapsedGameTime.TotalSeconds;
			for (int i = 0; i < base.Children.Count; i++)
			{
				Entity e = base.Children[i];
				if (e.Physics != null)
				{
					e.Physics.Accelerate(gameTime.ElapsedGameTime);
				}
			}
			this.Colliders.Clear();
			this.Collidees.Clear();
			for (int j = 0; j < base.Children.Count; j++)
			{
				Entity e2 = base.Children[j];
				if (e2.Collider)
				{
					this.Colliders.Add(e2);
				}
				if (e2.Collidee)
				{
					this.Collidees.Add(e2);
				}
			}
			using (Profiler.TimeSection("Collision", ProfilerThreadEnum.MAIN))
			{
				for (int k = 0; k < this.Colliders.Count; k++)
				{
					Entity collider = this.Colliders[k];
					collider.ResolveCollsions(this.Collidees, gameTime);
				}
			}
			for (int l = 0; l < base.Children.Count; l++)
			{
				Entity e3 = base.Children[l];
				if (e3.Physics != null)
				{
					e3.Physics.Move(gameTime.ElapsedGameTime);
				}
			}
			base.OnUpdate(gameTime);
		}

		private void DrawList(GraphicsDevice device, List<Entity> drawList, GameTime gameTime, Matrix projection)
		{
			int nextDrawPriority = int.MaxValue;
			int count = drawList.Count;
			for (int i = 0; i < count; i++)
			{
				nextDrawPriority = Math.Min(nextDrawPriority, drawList[i].DrawPriority);
			}
			bool finished = false;
			while (!finished)
			{
				finished = true;
				int thisDrawPriority = nextDrawPriority;
				nextDrawPriority = int.MaxValue;
				for (int j = 0; j < count; j++)
				{
					Entity e = drawList[j];
					if (thisDrawPriority == e.DrawPriority)
					{
						e.SetRenderState(device);
						e.Draw(device, gameTime, this._view, projection);
					}
					else if (e.DrawPriority > thisDrawPriority && e.DrawPriority < nextDrawPriority)
					{
						nextDrawPriority = e.DrawPriority;
						finished = false;
					}
				}
			}
		}

		public void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection, FilterCallback<Entity> filter)
		{
			if (this.comparer == null)
			{
				this.comparer = new Scene.DistanceComparer(this);
			}
			this._view = view;
			this._lights.Clear();
			this.toSort.Clear();
			this.toDraw.Clear();
			base.GetDrawList(this._lights, this.toSort, this.toDraw, filter);
			this.DrawList(device, this.toDraw, gameTime, projection);
			this.toSort.Sort(this.comparer);
			this.DrawList(device, this.toSort, gameTime, projection);
			base.Draw(device, gameTime, view, projection);
		}

		private List<Entity> Colliders = new List<Entity>();

		private List<Entity> Collidees = new List<Entity>();

		private List<Light> _lights = new List<Light>();

		private ReadOnlyCollection<Light> _lightsReadOnly;

		private Matrix _view;

		private List<Entity> toDraw = new List<Entity>();

		private List<Entity> toSort = new List<Entity>();

		private IComparer<Entity> comparer;

		private class DistanceComparer : IComparer<Entity>
		{
			public DistanceComparer(Scene owner)
			{
				this._owner = owner;
			}

			public int Compare(Entity e1, Entity e2)
			{
				Matrix m = e1.LocalToWorld * this._owner._view;
				e2.LocalToWorld * this._owner._view;
				Vector3 p = Vector3.Transform(Vector3.Zero, m);
				Vector3 p2 = Vector3.Transform(Vector3.Zero, m);
				if (p2.Z > p.Z)
				{
					return 1;
				}
				if (p2.Z < p.Z)
				{
					return -1;
				}
				return 0;
			}

			private Scene _owner;
		}
	}
}
