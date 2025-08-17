using System;

namespace DNA.Drawing
{
	public abstract class Physics
	{
		public Entity Owner
		{
			get
			{
				return this._owner;
			}
		}

		public Physics(Entity owner)
		{
			this._owner = owner;
		}

		public abstract void Accelerate(TimeSpan dt);

		public abstract void Move(TimeSpan dt);

		public abstract void Simulate(TimeSpan dt);

		private Entity _owner;
	}
}
