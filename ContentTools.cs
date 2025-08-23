using System;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace DNA
{
	public static class ContentTools
	{
		public static string GetLocalizedAssetName(string rootDirectory, string assetName)
		{
			string[] cultureNames = new string[]
			{
				CultureInfo.CurrentUICulture.Name,
				CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
			};
			foreach (string cultureName in cultureNames)
			{
				string localizedAssetName = assetName + '.' + cultureName;
				string localizedAssetPath = Path.Combine(rootDirectory, localizedAssetName + ".xnb");
				if (File.Exists(localizedAssetPath))
				{
					return localizedAssetName;
				}
			}
			return assetName;
		}

		public static T LoadLocalized<T>(this ContentManager content, string name)
		{
			string locName = ContentTools.GetLocalizedAssetName(content.RootDirectory, name);
			return content.Load<T>(locName);
		}
	}
}
