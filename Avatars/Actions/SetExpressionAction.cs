using System;
using DNA.Drawing;
using DNA.Net.GamerServices;

namespace DNA.Avatars.Actions
{
	public class SetExpressionAction : State
	{
		public SetExpressionAction(AvatarExpression expression)
		{
			this._expression = expression;
		}

		protected override void OnStart(Entity entity)
		{
			Avatar avatar = (Avatar)entity;
			avatar.Expression = this._expression;
			base.OnStart(entity);
		}

		private AvatarExpression _expression;
	}
}
