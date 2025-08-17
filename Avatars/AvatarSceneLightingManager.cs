using System;
using System.Collections.ObjectModel;
using DNA.Drawing;
using DNA.Drawing.Lights;
using Microsoft.Xna.Framework;

namespace DNA.Avatars
{
	public class AvatarSceneLightingManager : AvatarLightingManager
	{
		public virtual bool UseLight(Avatar avatar, Light light)
		{
			return true;
		}

		protected virtual Vector3 GetAvatarWorldPosition(Avatar avatar)
		{
			return avatar.WorldPosition + new Vector3(0f, 1f, 0f);
		}

		protected virtual Scene GetScene(Avatar avatar)
		{
			return avatar.Scene;
		}

		public override void SetAvatarLighting(Avatar avatar)
		{
			Scene scene = this.GetScene(avatar);
			Vector3 avatarWorldPosition = this.GetAvatarWorldPosition(avatar);
			Vector3 vector = Vector3.Zero;
			Vector3 vector2 = Vector3.Zero;
			Vector3 vector3 = Vector3.Zero;
			float num = 0f;
			float num2 = 0f;
			ReadOnlyCollection<Light> lights = scene.Lights;
			int count = lights.Count;
			for (int i = 0; i < count; i++)
			{
				Light light = lights[i];
				float influence = light.GetInfluence(avatarWorldPosition);
				if (influence > 0f && this.UseLight(avatar, light))
				{
					num += influence;
					if (light is AmbientLight)
					{
						vector2 += light.LightColor.ToVector3() * influence;
						num2 += influence;
					}
					else
					{
						vector3 += light.LightColor.ToVector3() * influence;
						num = influence;
						if (light is DirectionalLight)
						{
							DirectionalLight directionalLight = (DirectionalLight)light;
							vector += directionalLight.LightDirection * influence;
						}
						else
						{
							Vector3 vector4 = avatarWorldPosition - light.WorldPosition;
							vector += vector4 * influence;
						}
					}
				}
			}
			if (num2 < 1f)
			{
				vector2 += base.AmbientLightColor.ToVector3() * (1f - num2);
			}
			if (num < 1f)
			{
				vector3 += base.LightColor.ToVector3() * (1f - num);
				vector += base.LightDirection * (1f - num);
			}
			base.LightDirection.Normalize();
			this.SetAvatarLighting(avatar, vector2, vector3, vector);
		}
	}
}
