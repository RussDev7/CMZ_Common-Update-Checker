using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DNA.IO.Checksums;
using DNA.IO.Storage;
using DNA.Net.GamerServices;
using DNA.Security;

namespace DNA
{
	public class PromoCode
	{
		static PromoCode()
		{
			string friendlyChars = "AEFHKMNPRTUVWXYZ";
			for (int i = 0; i < 256; i++)
			{
				PromoCode.FriendlyToHexLookup[i] = (char)i;
				PromoCode.HexToFriendlyLookup[i] = (char)i;
			}
			for (int j = 0; j < 16; j++)
			{
				char hexValue = j.ToString("X1")[0];
				char freindlyValue = friendlyChars[j];
				PromoCode.FriendlyToHexLookup[(int)freindlyValue] = hexValue;
				PromoCode.HexToFriendlyLookup[(int)hexValue] = freindlyValue;
			}
		}

		public string Reward
		{
			get
			{
				return this._reward;
			}
		}

		public string SystemName
		{
			get
			{
				return this._systemName;
			}
		}

		public bool Redeemed
		{
			get
			{
				return this._redeemed;
			}
		}

		public uint AltHashCode
		{
			get
			{
				return this._altHashCode;
			}
		}

		public uint HashCode
		{
			get
			{
				return this._hashCode;
			}
		}

		public string UserCode
		{
			get
			{
				return this._userCode;
			}
		}

		public string FriendlyUserCode
		{
			get
			{
				return this._friendlyUserCode;
			}
		}

		public string AltUserCode
		{
			get
			{
				return this._altUserCode;
			}
		}

		private static string HexToFriendlyCode(string code)
		{
			StringBuilder builder = new StringBuilder();
			foreach (char c in code)
			{
				if (c < 'Ā')
				{
					c = PromoCode.HexToFriendlyLookup[(int)c];
				}
				builder.Append(c);
			}
			return builder.ToString();
		}

		private static string FriendlyToHexCode(string code)
		{
			StringBuilder builder = new StringBuilder();
			foreach (char c in code)
			{
				if (c < 'Ā')
				{
					c = PromoCode.FriendlyToHexLookup[(int)c];
				}
				builder.Append(c);
			}
			return builder.ToString();
		}

		private static uint ParshHash(string str)
		{
			str = str.ToUpper();
			str = str.Replace(" ", "");
			str = str.Replace("-", "");
			uint hexNum;
			if (uint.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out hexNum))
			{
				return hexNum;
			}
			str = PromoCode.FriendlyToHexCode(str);
			if (uint.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out hexNum))
			{
				return hexNum;
			}
			return 0U;
		}

		public PromoCode(PromoCode.PromoCodeManager manager, string systemName, string rewardDescription, string gamerTag, object tag)
		{
			this._manager = manager;
			this._reward = rewardDescription;
			this._systemName = systemName;
			this.Tag = tag;
			string encStr = SecurityTools.DecryptString(PromoCode.Key, PromoCode.Code) + this.SystemName + gamerTag;
			byte[] data = Encoding.UTF8.GetBytes(encStr);
			Crc32 crc = new Crc32();
			crc.Update(data);
			this._altHashCode = crc.Value;
			this._altUserCode = this._altHashCode.ToString("X8");
			this._altUserCode = this._altUserCode.Substring(0, 4) + "-" + this._altUserCode.Substring(4, 4);
			encStr = SecurityTools.DecryptString(PromoCode.Key, PromoCode.Code) + this.SystemName + gamerTag.ToLower();
			data = Encoding.UTF8.GetBytes(encStr);
			crc = new Crc32();
			crc.Update(data);
			this._hashCode = crc.Value;
			this._userCode = this._hashCode.ToString("X8");
			this._userCode = this._userCode.Substring(0, 4) + "-" + this._userCode.Substring(4, 4);
			this._friendlyUserCode = PromoCode.HexToFriendlyCode(this._userCode);
		}

		private static byte[] Key = new byte[]
		{
			139, 82, 60, 111, 51, 59, 131, 183, 231, 245,
			94, 184, 156, 205, 144, 40, 162, 242, 237, 111,
			47, 165, 165, 151, 60, 233, 179, 58, 208, 152,
			219, 0
		};

		private static byte[] Code = new byte[]
		{
			166, 148, 102, 129, 240, 5, 112, 15, 237, 81,
			251, 96, 55, 147, byte.MaxValue, 180
		};

		private static char[] FriendlyToHexLookup = new char[256];

		private static char[] HexToFriendlyLookup = new char[256];

		private string _systemName;

		private string _reward;

		private bool _redeemed;

		private uint _altHashCode;

		private uint _hashCode;

		private string _friendlyUserCode;

		private string _userCode;

		private string _altUserCode;

		public object Tag;

		private PromoCode.PromoCodeManager _manager;

		public class PromoCodeManager
		{
			public PromoCodeManager(SignedInGamer gamer, SaveDevice saveDevice)
			{
				this._saveDevice = saveDevice;
				this._gamer = gamer;
			}

			public PromoCode RegisterCode(string systemName, string reward, object tag)
			{
				PromoCode code = new PromoCode(this, systemName, reward, this._gamer.Gamertag, tag);
				this.Codes[code.SystemName] = code;
				return code;
			}

			public List<PromoCode> GetRedeemedCodes()
			{
				List<PromoCode> redeemedCodes = new List<PromoCode>();
				foreach (KeyValuePair<string, PromoCode> pair in this.Codes)
				{
					if (pair.Value.Redeemed)
					{
						redeemedCodes.Add(pair.Value);
					}
				}
				return redeemedCodes;
			}

			public PromoCode GetDisplayCode(string name, string description, object tag)
			{
				return new PromoCode(null, name, description, this._gamer.Gamertag, tag);
			}

			public void LoadCodes()
			{
				try
				{
					this._saveDevice.Load(PromoCode.PromoCodeManager.CodeFileName, delegate(Stream stream)
					{
						BinaryReader reader = new BinaryReader(stream);
						int count = reader.ReadInt32();
						for (int i = 0; i < count; i++)
						{
							string name = reader.ReadString();
							uint hash = reader.ReadUInt32();
							PromoCode code;
							if (this.Codes.TryGetValue(name, out code) && (code.HashCode == hash || code.AltHashCode == hash))
							{
								code._redeemed = true;
							}
						}
					});
				}
				catch
				{
				}
			}

			private void SaveCodes()
			{
				try
				{
					this._saveDevice.Save(PromoCode.PromoCodeManager.CodeFileName, true, true, delegate(Stream stream)
					{
						List<PromoCode> redeemedCodes = this.GetRedeemedCodes();
						if (redeemedCodes.Count == 0)
						{
							return;
						}
						BinaryWriter writer = new BinaryWriter(stream);
						writer.Write(redeemedCodes.Count);
						foreach (PromoCode code in redeemedCodes)
						{
							writer.Write(code.SystemName);
							writer.Write(code.HashCode);
						}
						writer.Flush();
					});
				}
				catch
				{
				}
			}

			public void Redeem(PromoCode code)
			{
				code._redeemed = true;
				this.SaveCodes();
			}

			public PromoCode Redeem(string code, out string reason)
			{
				uint hash = PromoCode.ParshHash(code);
				reason = "Invalid Code";
				foreach (KeyValuePair<string, PromoCode> pair in this.Codes)
				{
					if (hash == pair.Value.HashCode || hash == pair.Value.AltHashCode)
					{
						if (!pair.Value.Redeemed)
						{
							pair.Value._redeemed = true;
							reason = "Success";
							this.SaveCodes();
							return pair.Value;
						}
						reason = "Code Already Redeemed";
					}
				}
				return null;
			}

			private static string CodeFileName = "pdat.user";

			private SignedInGamer _gamer;

			private SaveDevice _saveDevice;

			public Dictionary<string, PromoCode> Codes = new Dictionary<string, PromoCode>();
		}
	}
}
