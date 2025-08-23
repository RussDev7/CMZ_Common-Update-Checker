using System;
using System.Globalization;
using System.Text;
using DNA.Text;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerGeneralizedTime : Asn1Object
	{
		public static DerGeneralizedTime GetInstance(object obj)
		{
			if (obj == null || obj is DerGeneralizedTime)
			{
				return (DerGeneralizedTime)obj;
			}
			if (obj is Asn1OctetString)
			{
				return new DerGeneralizedTime(((Asn1OctetString)obj).GetOctets());
			}
			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name, "obj");
		}

		public static DerGeneralizedTime GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			return DerGeneralizedTime.GetInstance(obj.GetObject());
		}

		public DerGeneralizedTime(string time)
		{
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

		public DerGeneralizedTime(DateTime time)
		{
			this.time = time.ToString("yyyyMMddHHmmss\\Z");
		}

		internal DerGeneralizedTime(byte[] bytes)
		{
			this.time = ASCIIEncoder.GetString(bytes, 0, bytes.Length);
		}

		public string TimeString
		{
			get
			{
				return this.time;
			}
		}

		public string GetTime()
		{
			if (this.time[this.time.Length - 1] == 'Z')
			{
				return this.time.Substring(0, this.time.Length - 1) + "GMT+00:00";
			}
			int signPos = this.time.Length - 5;
			char sign = this.time[signPos];
			if (sign == '-' || sign == '+')
			{
				return string.Concat(new string[]
				{
					this.time.Substring(0, signPos),
					"GMT",
					this.time.Substring(signPos, 3),
					":",
					this.time.Substring(signPos + 3)
				});
			}
			signPos = this.time.Length - 3;
			sign = this.time[signPos];
			if (sign == '-' || sign == '+')
			{
				return this.time.Substring(0, signPos) + "GMT" + this.time.Substring(signPos) + ":00";
			}
			return this.time + this.CalculateGmtOffset();
		}

		private string CalculateGmtOffset()
		{
			char sign = '+';
			int minutes = TimeZone.CurrentTimeZone.GetUtcOffset(this.ToDateTime()).Minutes;
			if (minutes < 0)
			{
				sign = '-';
				minutes = -minutes;
			}
			int hours = minutes / 60;
			minutes %= 60;
			return string.Concat(new object[]
			{
				"GMT",
				sign,
				DerGeneralizedTime.Convert(hours),
				":",
				DerGeneralizedTime.Convert(minutes)
			});
		}

		private static string Convert(int time)
		{
			if (time < 10)
			{
				return "0" + time;
			}
			return time.ToString();
		}

		public DateTime ToDateTime()
		{
			string d = this.time;
			bool makeUniversal = false;
			string formatStr;
			if (d.EndsWith("Z"))
			{
				if (this.HasFractionalSeconds)
				{
					int fCount = d.Length - d.IndexOf('.') - 2;
					formatStr = "yyyyMMddHHmmss." + this.FString(fCount) + "\\Z";
				}
				else
				{
					formatStr = "yyyyMMddHHmmss\\Z";
				}
			}
			else if (this.time.IndexOf('-') > 0 || this.time.IndexOf('+') > 0)
			{
				d = this.GetTime();
				makeUniversal = true;
				if (this.HasFractionalSeconds)
				{
					int fCount2 = d.IndexOf("GMT") - 1 - d.IndexOf('.');
					formatStr = "yyyyMMddHHmmss." + this.FString(fCount2) + "'GMT'zzz";
				}
				else
				{
					formatStr = "yyyyMMddHHmmss'GMT'zzz";
				}
			}
			else if (this.HasFractionalSeconds)
			{
				int fCount3 = d.Length - 1 - d.IndexOf('.');
				formatStr = "yyyyMMddHHmmss." + this.FString(fCount3);
			}
			else
			{
				formatStr = "yyyyMMddHHmmss";
			}
			return this.ParseDateString(d, formatStr, makeUniversal);
		}

		private string FString(int count)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < count; i++)
			{
				sb.Append('f');
			}
			return sb.ToString();
		}

		private DateTime ParseDateString(string dateStr, string formatStr, bool makeUniversal)
		{
			DateTime dt = DateTime.ParseExact(dateStr, formatStr, DateTimeFormatInfo.InvariantInfo);
			if (!makeUniversal)
			{
				return dt;
			}
			return dt.ToUniversalTime();
		}

		private bool HasFractionalSeconds
		{
			get
			{
				return this.time.IndexOf('.') == 14;
			}
		}

		private byte[] GetOctets()
		{
			return ASCIIEncoder.GetBytes(this.time);
		}

		internal override void Encode(DerOutputStream derOut)
		{
			derOut.WriteEncoded(24, this.GetOctets());
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerGeneralizedTime other = asn1Object as DerGeneralizedTime;
			return other != null && this.time.Equals(other.time);
		}

		protected override int Asn1GetHashCode()
		{
			return this.time.GetHashCode();
		}

		private readonly string time;
	}
}
