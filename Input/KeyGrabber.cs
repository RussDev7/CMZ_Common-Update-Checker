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
					IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(m));
					Marshal.StructureToPtr(m, intPtr, true);
					KeyGrabber.KeyFilter.TranslateMessage(intPtr);
					return true;
				}
				if (m.Msg == 258)
				{
					char c = (char)(int)m.WParam;
					if (KeyGrabber.InboundCharEvent != null)
					{
						KeyGrabber.InboundCharEvent(c);
					}
				}
				return false;
			}

			[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
			public static extern bool TranslateMessage(IntPtr message);
		}
	}
}
