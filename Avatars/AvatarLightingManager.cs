﻿using System;
using Microsoft.Xna.Framework;

namespace DNA.Avatars
{
	public abstract class AvatarLightingManager
	{
		public Vector3 LightDirection
		{
			get
			{
				return this._lightDirection;
			}
			set
			{
				this._lightDirection = value;
			}
		}

		public Color LightColor
		{
			get
			{
				return this._lightColor;
			}
			set
			{
				this._lightColor = value;
			}
		}

		public Color AmbientLightColor
		{
			get
			{
				return this._ambientLightColor;
			}
			set
			{
				this._ambientLightColor = value;
			}
		}

		protected virtual void SetAvatarLighting(Avatar avatar, Vector3 ambientLightColor, Vector3 LightColor, Vector3 LightDirection)
		{
			avatar.AvatarRenderer.LightDirection = LightDirection;
			avatar.AvatarRenderer.LightColor = LightColor;
			avatar.AvatarRenderer.AmbientLightColor = ambientLightColor;
		}

		public virtual void SetAvatarLighting(Avatar avatar)
		{
			this.SetAvatarLighting(avatar, this.AmbientLightColor.ToVector3(), this.LightColor.ToVector3(), this.LightDirection);
		}

		private Vector3 _lightDirection = new Vector3(-0.5f, -0.6123f, -0.6123f);

		private Color _lightColor = new Color(0.4f, 0.4f, 0.4f);

		private Color _ambientLightColor = new Color(0.55f, 0.55f, 0.55f);
	}
}
