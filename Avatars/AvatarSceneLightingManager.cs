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
			Vector3 avatarPosition = this.GetAvatarWorldPosition(avatar);
			Vector3 finalDirection = Vector3.Zero;
			Vector3 finalAmbientColor = Vector3.Zero;
			Vector3 finalColor = Vector3.Zero;
			float totalInfluence = 0f;
			float totalAmbientInfluence = 0f;
			ReadOnlyCollection<Light> lights = scene.Lights;
			int lightCount = lights.Count;
			for (int i = 0; i < lightCount; i++)
			{
				Light light = lights[i];
				float influence = light.GetInfluence(avatarPosition);
				if (influence > 0f && this.UseLight(avatar, light))
				{
					totalInfluence += influence;
					if (light is AmbientLight)
					{
						finalAmbientColor += light.LightColor.ToVector3() * influence;
						totalAmbientInfluence += influence;
					}
					else
					{
						finalColor += light.LightColor.ToVector3() * influence;
						totalInfluence = influence;
						if (light is DirectionalLight)
						{
							DirectionalLight dl = (DirectionalLight)light;
							finalDirection += dl.LightDirection * influence;
						}
						else
						{
							Vector3 direction = avatarPosition - light.WorldPosition;
							finalDirection += direction * influence;
						}
					}
				}
			}
			if (totalAmbientInfluence < 1f)
			{
				finalAmbientColor += base.AmbientLightColor.ToVector3() * (1f - totalAmbientInfluence);
			}
			if (totalInfluence < 1f)
			{
				finalColor += base.LightColor.ToVector3() * (1f - totalInfluence);
				finalDirection += base.LightDirection * (1f - totalInfluence);
			}
			base.LightDirection.Normalize();
			this.SetAvatarLighting(avatar, finalAmbientColor, finalColor, finalDirection);
		}
	}
}
