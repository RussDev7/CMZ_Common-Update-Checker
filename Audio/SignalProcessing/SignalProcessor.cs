﻿using System;

namespace DNA.Audio.SignalProcessing
{
	public abstract class SignalProcessor<T>
	{
		public virtual int? SampleRate
		{
			get
			{
				return null;
			}
		}

		public virtual int? Channels
		{
			get
			{
				return null;
			}
		}

		public abstract bool ProcessBlock(T data);

		public virtual void OnStart()
		{
		}

		public virtual void OnStop()
		{
		}

		public bool Active = true;
	}
}
