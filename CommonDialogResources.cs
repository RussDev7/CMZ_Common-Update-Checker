using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace DNA
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[CompilerGenerated]
	[DebuggerNonUserCode]
	internal class CommonDialogResources
	{
		internal CommonDialogResources()
		{
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(CommonDialogResources.resourceMan, null))
				{
					ResourceManager temp = new ResourceManager("DNA.CommonDialogResources", typeof(CommonDialogResources).Assembly);
					CommonDialogResources.resourceMan = temp;
				}
				return CommonDialogResources.resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return CommonDialogResources.resourceCulture;
			}
			set
			{
				CommonDialogResources.resourceCulture = value;
			}
		}

		internal static Bitmap loginwithfacebook
		{
			get
			{
				object obj = CommonDialogResources.ResourceManager.GetObject("loginwithfacebook", CommonDialogResources.resourceCulture);
				return (Bitmap)obj;
			}
		}

		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;
	}
}
