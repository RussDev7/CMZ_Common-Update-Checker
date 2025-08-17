using System;

namespace DNA.Input
{
	public abstract class ControllerMapping
	{
		public InputBinding Binding
		{
			get
			{
				return this._binding;
			}
		}

		public abstract void ProcessInput(KeyboardInput keyboard, MouseInput mouse, GameController controller);

		private InputBinding _binding = new InputBinding();
	}
}
