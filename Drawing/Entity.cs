using System;
using System.Collections.Generic;
using DNA.Collections;
using DNA.Drawing.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class Entity : Tree<Entity>
	{
		public void ApplyEffect(Effect effect, bool applyToChildren)
		{
			this.OnApplyEffect(effect);
			if (applyToChildren)
			{
				foreach (Entity entity in base.Children)
				{
					entity.ApplyEffect(effect, applyToChildren);
				}
			}
		}

		public virtual BoundingSphere GetLocalBoundingSphere()
		{
			return new BoundingSphere(new Vector3(0f, 0f, 0f), 0f);
		}

		public virtual BoundingBox GetAABB()
		{
			return new BoundingBox(this.WorldPosition, this.WorldPosition);
		}

		protected virtual void OnApplyEffect(Effect sourceEffect)
		{
		}

		public SamplerState SamplerState
		{
			get
			{
				if (this._samplerState != null)
				{
					return this._samplerState;
				}
				Entity parent = base.Parent;
				if (parent == null)
				{
					return Entity.DefaultSamplerState;
				}
				return parent.SamplerState;
			}
			set
			{
				this._samplerState = value;
			}
		}

		public BlendState BlendState
		{
			get
			{
				if (this._blendState != null)
				{
					return this._blendState;
				}
				Entity parent = base.Parent;
				if (parent == null)
				{
					return Entity.DefaultBlendState;
				}
				return parent.BlendState;
			}
			set
			{
				this._blendState = value;
			}
		}

		public RasterizerState RasterizerState
		{
			get
			{
				if (this._rasterizerState != null)
				{
					return this._rasterizerState;
				}
				Entity parent = base.Parent;
				if (parent == null)
				{
					return Entity.DefaultRasterizerState;
				}
				return parent.RasterizerState;
			}
			set
			{
				this._rasterizerState = value;
			}
		}

		public DepthStencilState DepthStencilState
		{
			get
			{
				if (this._depthStencilState != null)
				{
					return this._depthStencilState;
				}
				Entity parent = base.Parent;
				if (parent == null)
				{
					return Entity.DefaultDepthStencilState;
				}
				return parent.DepthStencilState;
			}
			set
			{
				this._depthStencilState = value;
			}
		}

		public void SetRenderState(GraphicsDevice device)
		{
			Entity parent = base.Parent;
			if (parent == null)
			{
				if (this._samplerState != null)
				{
					device.SamplerStates[0] = this._samplerState;
				}
				else
				{
					device.SamplerStates[0] = Entity.DefaultSamplerState;
				}
				if (this._blendState != null)
				{
					device.BlendState = this._blendState;
				}
				else
				{
					device.BlendState = Entity.DefaultBlendState;
				}
				if (this._rasterizerState != null)
				{
					device.RasterizerState = this._rasterizerState;
				}
				else
				{
					device.RasterizerState = Entity.DefaultRasterizerState;
				}
				if (this._depthStencilState != null)
				{
					device.DepthStencilState = this._depthStencilState;
					return;
				}
				device.DepthStencilState = Entity.DefaultDepthStencilState;
				return;
			}
			else
			{
				if (this._samplerState != null)
				{
					device.SamplerStates[0] = this._samplerState;
				}
				else
				{
					device.SamplerStates[0] = parent.SamplerState;
				}
				if (this._blendState != null)
				{
					device.BlendState = this._blendState;
				}
				else
				{
					device.BlendState = parent.BlendState;
				}
				if (this._rasterizerState != null)
				{
					device.RasterizerState = this._rasterizerState;
				}
				else
				{
					device.RasterizerState = parent.RasterizerState;
				}
				if (this._depthStencilState != null)
				{
					device.DepthStencilState = this._depthStencilState;
					return;
				}
				device.DepthStencilState = parent.DepthStencilState;
				return;
			}
		}

		public Physics Physics
		{
			get
			{
				return this._physics;
			}
			set
			{
				this._physics = value;
				if (this._physics != null && this._physics.Owner != this)
				{
					throw new Exception();
				}
			}
		}

		public Queue<State> ActionQueue
		{
			get
			{
				return this._actionQueue;
			}
		}

		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		protected void GetDrawList(List<Light> lights, List<Entity> toSort, List<Entity> toDraw, FilterCallback<Entity> filter)
		{
			for (int i = 0; i < base.Children.Count; i++)
			{
				Entity entity = base.Children[i];
				if (entity.Visible && filter(entity))
				{
					if (entity is Light)
					{
						lights.Add((Light)entity);
					}
					else if (this.AlphaSort)
					{
						if (this.AlphaSort)
						{
							toSort.Add(entity);
						}
						else
						{
							toDraw.Add(entity);
						}
						entity.GetDrawList(lights, toSort, toDraw, filter);
					}
					else
					{
						toDraw.Add(entity);
					}
					entity.GetDrawList(lights, toSort, toDraw, filter);
				}
			}
		}

		public virtual void ResolveCollsions(List<Entity> collidees, GameTime dt)
		{
			for (int i = 0; i < collidees.Count; i++)
			{
				Entity entity = collidees[i];
				Plane plane;
				if (this.CollidesAgainist(entity) && this.ResolveCollsion(entity, out plane, dt))
				{
					this.OnCollisionWith(entity, plane);
				}
			}
		}

		public virtual void OnCollisionWith(Entity e, Plane collsionPlane)
		{
		}

		public virtual bool CollidesAgainist(Entity e)
		{
			return e != this && this.Collider && e.Collidee;
		}

		public virtual bool ResolveCollsion(Entity e, out Plane collsionPlane, GameTime dt)
		{
			collsionPlane = Entity.EmptyPlane;
			return false;
		}

		public void RemoveFromParent()
		{
			if (base.Parent != null)
			{
				base.Parent.Children.Remove(this);
			}
		}

		protected virtual void OnMoved()
		{
		}

		private void DirtyLTW()
		{
			this.OnMoved();
			this._ltwDirty = true;
			for (int i = 0; i < base.Children.Count; i++)
			{
				Entity entity = base.Children[i];
				if (!entity._ltwDirty)
				{
					entity.DirtyLTW();
				}
			}
		}

		public Entity()
			: base(20)
		{
		}

		protected override void OnParentChanged(Entity oldParent, Entity newParent)
		{
			this.DirtyLTW();
			base.OnParentChanged(oldParent, newParent);
		}

		public Matrix LocalToParent
		{
			get
			{
				if (this._ltpDirty)
				{
					this._cachedLocalToParent = Matrix.CreateScale(this._localScale) * Matrix.CreateFromQuaternion(this._localRotation);
					this._cachedLocalToParent.Translation = this._localPosition;
				}
				return this._cachedLocalToParent;
			}
			set
			{
				this._cachedLocalToParent = value;
				this._ltpDirty = false;
				value.Decompose(out this._localScale, out this._localRotation, out this._localPosition);
				this.DirtyLTW();
			}
		}

		protected bool LTWDirty
		{
			get
			{
				return this._ltwDirty;
			}
		}

		public Matrix LocalToWorld
		{
			get
			{
				if (this._ltwDirty)
				{
					if (base.Parent == null)
					{
						this._localToWorld = this.LocalToParent;
					}
					else
					{
						this._localToWorld = this.LocalToParent * base.Parent.LocalToWorld;
					}
					this._ltwDirty = false;
				}
				return this._localToWorld;
			}
		}

		public Matrix WorldToLocal
		{
			get
			{
				return Matrix.Invert(this.LocalToWorld);
			}
		}

		public Vector3 WorldPosition
		{
			get
			{
				if (this._ltwDirty)
				{
					return this.LocalToWorld.Translation;
				}
				return this._localToWorld.Translation;
			}
		}

		public Vector3 LocalPosition
		{
			get
			{
				return this._localPosition;
			}
			set
			{
				this._localPosition = value;
				this._ltpDirty = true;
				this.DirtyLTW();
			}
		}

		public Vector3 LocalScale
		{
			get
			{
				return this._localScale;
			}
			set
			{
				this._localScale = value;
				this._ltpDirty = true;
				this.DirtyLTW();
			}
		}

		public Quaternion LocalRotation
		{
			get
			{
				return this._localRotation;
			}
			set
			{
				this._localRotation = value;
				this._localRotation.Normalize();
				this._ltpDirty = true;
				this.DirtyLTW();
			}
		}

		public Scene Scene
		{
			get
			{
				for (Entity entity = this; entity != null; entity = entity.Parent)
				{
					if (entity is Scene || entity == null)
					{
						return (Scene)entity;
					}
				}
				return null;
			}
		}

		public void AdoptChild(Entity child)
		{
			Vector3 worldPosition = this.WorldPosition;
			Matrix localToWorld = child.LocalToWorld;
			Matrix worldToLocal = this.WorldToLocal;
			Matrix matrix = localToWorld * worldToLocal;
			if (child.Parent != null)
			{
				child.RemoveFromParent();
			}
			base.Children.Add(child);
			Vector3 vector;
			Quaternion quaternion;
			Vector3 vector2;
			matrix.Decompose(out vector, out quaternion, out vector2);
			child.LocalPosition = vector2;
			child.LocalRotation = quaternion;
			child.LocalScale = vector;
			Vector3 worldPosition2 = this.WorldPosition;
		}

		protected virtual void OnActionStarted(State action)
		{
		}

		protected virtual void OnActionComplete(State action)
		{
		}

		public void InjectAction(State action)
		{
			this.InjectActions(new State[] { action });
		}

		public void InjectActions(IList<State> actions)
		{
			Queue<State> queue = new Queue<State>();
			for (int i = 0; i < actions.Count; i++)
			{
				queue.Enqueue(actions[i]);
			}
			if (this._currentAction != null)
			{
				queue.Enqueue(this._currentAction);
				this._currentAction = null;
			}
			foreach (State state in this._actionQueue)
			{
				queue.Enqueue(state);
			}
			this._actionQueue = queue;
		}

		private void NextAction()
		{
			if (this._currentAction == null && this._actionQueue.Count > 0)
			{
				this._currentAction = this._actionQueue.Dequeue();
				this._currentAction.Start(this);
				this.OnActionStarted(this._currentAction);
			}
		}

		public void EndCurrentAction()
		{
			if (this._currentAction != null)
			{
				this._currentAction.End(this);
				this._currentAction = null;
				this.OnActionComplete(this._currentAction);
			}
		}

		public void ResetActions()
		{
			this.EndCurrentAction();
			this._actionQueue.Clear();
		}

		public State CurrentAction
		{
			get
			{
				return this._currentAction;
			}
		}

		protected virtual void OnUpdate(GameTime gameTime)
		{
		}

		public virtual void OnPhysics(GameTime gameTime)
		{
		}

		public virtual void Update(DNAGame game, GameTime gameTime)
		{
			if (!this.DoUpdate)
			{
				return;
			}
			this.NextAction();
			if (this._currentAction != null)
			{
				this._currentAction.Tick(game, this, gameTime);
				if (this._currentAction != null && this._currentAction.Complete)
				{
					this.EndCurrentAction();
				}
			}
			this.NextAction();
			this.OnUpdate(gameTime);
			for (int i = 0; i < base.Children.Count; i++)
			{
				base.Children[i].Update(game, gameTime);
			}
		}

		public virtual void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
		}

		public void AfterFrame()
		{
			this.OnAfterFrame();
			for (int i = 0; i < base.Children.Count; i++)
			{
				base.Children[i].AfterFrame();
			}
		}

		public virtual void OnAfterFrame()
		{
		}

		public const int DefaultDrawPriority = 0;

		public Color? EntityColor;

		private Vector3 _localPosition = Vector3.Zero;

		private Quaternion _localRotation = Quaternion.Identity;

		private Vector3 _localScale = new Vector3(1f, 1f, 1f);

		private Matrix _localToWorld;

		private bool _ltwDirty = true;

		private State _currentAction;

		private Queue<State> _actionQueue = new Queue<State>();

		public int DrawPriority;

		public static SamplerState DefaultSamplerState = SamplerState.AnisotropicWrap;

		public static BlendState DefaultBlendState = BlendState.Opaque;

		public static RasterizerState DefaultRasterizerState = RasterizerState.CullCounterClockwise;

		public static DepthStencilState DefaultDepthStencilState = DepthStencilState.Default;

		private SamplerState _samplerState;

		private BlendState _blendState;

		private RasterizerState _rasterizerState;

		private DepthStencilState _depthStencilState;

		private Physics _physics;

		public bool AlphaSort;

		public bool Visible = true;

		public bool DoUpdate = true;

		public bool Collider;

		public bool Collidee;

		private string _name;

		private static Plane EmptyPlane = default(Plane);

		private bool _ltpDirty = true;

		private Matrix _cachedLocalToParent;
	}
}
