using System;
using System.Globalization;
using DNA.Text;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerUtcTime : Asn1Object
	{
		public static DerUtcTime GetInstance(object obj)
		{
			if (obj == null || obj is DerUtcTime)
			{
				return (DerUtcTime)obj;
			}
			if (obj is Asn1OctetString)
			{
				return new DerUtcTime(((Asn1OctetString)obj).GetOctets());
			}
			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
		}

		public static DerUtcTime GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			return DerUtcTime.GetInstance(obj.GetObject());
		}

		public DerUtcTime(string time)
		{
			if (time == null)
			{
				throw new ArgumentNullException("time");
			}
			this.time = time;
			try
			{
				this.ToDateTime();
			}
			catch (FormatException e)
			{
				throw new ArgumentException("invalid date string: " + e.Message);
			}
		}

		public DerUtcTime(DateTime time)
		{
			this.time = time.ToString("yyMMddHHmmss") + "Z";
		}

		internal DerUtcTime(byte[] bytes)
		{
			this.time = ASCIIEncoder.GetString(bytes, 0, bytes.Length);
		}

		public DateTime ToDateTime()
		{
			return this.ParseDateString(this.TimeString, "yyMMddHHmmss'GMT'zzz");
		}

		public DateTime ToAdjustedDateTime()
		{
			return this.ParseDateString(this.AdjustedTimeString, "yyyyMMddHHmmss'GMT'zzz");
		}

		private DateTime ParseDateString(string dateStr, string formatStr)
		{
			return DateTime.ParseExact(dateStr, formatStr, DateTimeFormatInfo.InvariantInfo).ToUniversalTime();
		}

		public string TimeString
		{
			get
			{
				if (this.time.IndexOf('-') < 0 && this.time.IndexOf('+') < 0)
				{
					if (this.time.Length == 11)
					{
						return this.time.Substring(0, 10) + "00GMT+00:00";
					}
					return this.time.Substring(0, 12) + "GMT+00:00";
				}
				else
				{
					int index = this.time.IndexOf('-');
					if (index < 0)
					{
						index = this.time.IndexOf('+');
					}
					string d = this.time;
					if (index == this.time.Length - 3)
					{
						d += "00";
					}
					if (index == 10)
					{
						return string.Concat(new string[]
						{
							d.Substring(0, 10),
							"00GMT",
							d.Substring(10, 3),
							":",
							d.Substring(13, 2)
						});
					}
					return string.Concat(new string[]
					{
						d.Substring(0, 12),
						"GMT",
						d.Substring(12, 3),
						":",
						d.Substring(15, 2)
					});
				}
			}
		}

		[Obsolete("Use 'AdjustedTimeString' property instead")]
		public string AdjustedTime
		{
			get
			{
				return this.AdjustedTimeString;
			}
		}

		public string AdjustedTimeString
		{
			get
			{
				string d = this.TimeString;
				string c = ((d[0] < '5') ? "20" : "19");
				return c + d;
			}
		}

		private byte[] GetOctets()
		{
			return ASCIIEncoder.GetBytes(this.time);
		}

		internal override void Encode(DerOutputStream derOut)
		{
			derOut.WriteEncoded(23, this.GetOctets());
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerUtcTime other = asn1Object as DerUtcTime;
			return other != null && this.time.Equals(other.time);
		}

		protected override int Asn1GetHashCode()
		{
			return this.time.GetHashCode();
		}

		public override string ToString()
		{
			return this.time;
		}

		private readonly string time;
	}
}
