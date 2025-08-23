using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public class BasicPhysics : Physics
	{
		public BasicPhysics(Entity owner)
			: base(owner)
		{
		}

		public Vector3 WorldVelocity
		{
			get
			{
				return this._worldVelocity;
			}
			set
			{
				this._worldVelocity = value;
			}
		}

		public Vector3 LocalVelocity
		{
			get
			{
				return Vector3.TransformNormal(this.WorldVelocity, base.Owner.WorldToLocal);
			}
			set
			{
				this.WorldVelocity = Vector3.TransformNormal(value, base.Owner.LocalToWorld);
			}
		}

		public void Reflect(Vector3 normal, float elasticity)
		{
			this.WorldVelocity = Vector3.Reflect(this.WorldVelocity, Vector3.Normalize(normal)) * elasticity;
		}

		public override void Accelerate(TimeSpan dt)
		{
			float seconds = (float)dt.TotalSeconds;
			Entity e = base.Owner;
			Vector3 currentVel = this.WorldVelocity;
			Vector3 worldAccel = Vector3.TransformNormal(this.LocalAcceleration, e.LocalToParent) + this.WorldAcceleration;
			currentVel.LengthSquared();
			this.WorldVelocity += worldAccel * seconds;
		}

		public override void Move(TimeSpan dt)
		{
			Entity e = base.Owner;
			float seconds = (float)dt.TotalSeconds;
			float speed = this.WorldVelocity.Length();
			if (speed != 0f)
			{
				e.LocalPosition += this.WorldVelocity * seconds;
			}
		}

		public override void Simulate(TimeSpan dt)
		{
		}

		public static Vector3 Gravity = new Vector3(0f, -20f, 0f);

		private Vector3 _worldVelocity = Vector3.Zero;

		public Vector3 LocalAcceleration = Vector3.Zero;

		public Vector3 WorldAcceleration = Vector3.Zero;
	}
}
