﻿using System;
using System.Diagnostics;

namespace DNA.Net.Lidgren
{
	public static class NetTime
	{
		public static double Now
		{
			get
			{
				return (double)(Stopwatch.GetTimestamp() - NetTime.s_timeInitialized) * NetTime.s_dInvFreq;
			}
		}

		public static string ToReadable(double seconds)
		{
			if (seconds > 60.0)
			{
				return TimeSpan.FromSeconds(seconds).ToString();
			}
			return (seconds * 1000.0).ToString("N2") + " ms";
		}

		private static readonly long s_timeInitialized = Stopwatch.GetTimestamp();

		private static readonly double s_dInvFreq = 1.0 / (double)Stopwatch.Frequency;
	}
}
