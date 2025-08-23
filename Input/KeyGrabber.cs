using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DNA.Input
{
	public class KeyGrabber
	{
		public static event Action<char> InboundCharEvent;

		static KeyGrabber()
		{
			Application.AddMessageFilter(new KeyGrabber.KeyFilter());
		}

		public class KeyFilter : IMessageFilter
		{
			public bool PreFilterMessage(ref Message m)
			{
				if (m.Msg == 256)
				{
					IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(m));
					Marshal.StructureToPtr(m, pointer, true);
					KeyGrabber.KeyFilter.TranslateMessage(pointer);
					return true;
				}
				if (m.Msg == 258)
				{
					char trueCharacter = (char)(int)m.WParam;
					if (KeyGrabber.InboundCharEvent != null)
					{
						KeyGrabber.InboundCharEvent(trueCharacter);
					}
				}
				return false;
			}

			[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
			public static extern bool TranslateMessage(IntPtr message);
		}
	}
}
