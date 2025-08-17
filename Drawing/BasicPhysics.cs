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
			float num = (float)dt.TotalSeconds;
			Entity owner = base.Owner;
			Vector3 worldVelocity = this.WorldVelocity;
			Vector3 vector = Vector3.TransformNormal(this.LocalAcceleration, owner.LocalToParent) + this.WorldAcceleration;
			worldVelocity.LengthSquared();
			this.WorldVelocity += vector * num;
		}

		public override void Move(TimeSpan dt)
		{
			Entity owner = base.Owner;
			float num = (float)dt.TotalSeconds;
			float num2 = this.WorldVelocity.Length();
			if (num2 != 0f)
			{
				owner.LocalPosition += this.WorldVelocity * num;
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
