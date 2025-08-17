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
			string[] array = new string[]
			{
				CultureInfo.CurrentUICulture.Name,
				CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
			};
			foreach (string text in array)
			{
				string text2 = assetName + '.' + text;
				string text3 = Path.Combine(rootDirectory, text2 + ".xnb");
				if (File.Exists(text3))
				{
					return text2;
				}
			}
			return assetName;
		}

		public static T LoadLocalized<T>(this ContentManager content, string name)
		{
			string localizedAssetName = ContentTools.GetLocalizedAssetName(content.RootDirectory, name);
			return content.Load<T>(localizedAssetName);
		}
	}
}
