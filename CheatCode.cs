using System;
using System.Collections.Generic;
using System.IO;
using DNA.IO.Storage;

namespace DNA
{
	public class CheatCode
	{
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

		public string Cheatcode
		{
			get
			{
				return this._cheatCode;
			}
		}

		public CheatCode(CheatCode.CheatCodeManager manager, string systemName, string rewardDescription, string code, object tag)
		{
			this._manager = manager;
			this._reward = rewardDescription;
			this._systemName = systemName;
			this.Tag = tag;
			this._cheatCode = code.ToUpper();
		}

		private string _systemName;

		private string _reward;

		private bool _redeemed;

		private string _cheatCode;

		public object Tag;

		private CheatCode.CheatCodeManager _manager;

		public class CheatCodeManager
		{
			public CheatCodeManager(SaveDevice saveDevice)
			{
				this._saveDevice = saveDevice;
			}

			public CheatCode RegisterCode(string systemName, string reward, string unlockCode, object tag)
			{
				CheatCode code = new CheatCode(this, systemName, reward, unlockCode, tag);
				this.Codes[code.SystemName] = code;
				return code;
			}

			public List<CheatCode> GetRedeemedCodes()
			{
				List<CheatCode> redeemedCodes = new List<CheatCode>();
				foreach (KeyValuePair<string, CheatCode> pair in this.Codes)
				{
					if (pair.Value.Redeemed)
					{
						redeemedCodes.Add(pair.Value);
					}
				}
				return redeemedCodes;
			}

			public CheatCode GetDisplayCode(string name, string description, string unlockCode, object tag)
			{
				return new CheatCode(null, name, description, unlockCode, tag);
			}

			public void LoadCodes()
			{
				try
				{
					this._saveDevice.Load(CheatCode.CheatCodeManager.CodeFileName, delegate(Stream stream)
					{
						BinaryReader reader = new BinaryReader(stream);
						int count = reader.ReadInt32();
						for (int i = 0; i < count; i++)
						{
							string name = reader.ReadString();
							CheatCode code;
							if (this.Codes.TryGetValue(name, out code))
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
					this._saveDevice.Save(CheatCode.CheatCodeManager.CodeFileName, true, true, delegate(Stream stream)
					{
						List<CheatCode> redeemedCodes = this.GetRedeemedCodes();
						if (redeemedCodes.Count == 0)
						{
							return;
						}
						BinaryWriter writer = new BinaryWriter(stream);
						writer.Write(redeemedCodes.Count);
						foreach (CheatCode code in redeemedCodes)
						{
							writer.Write(code.SystemName);
						}
						writer.Flush();
					});
				}
				catch
				{
				}
			}

			public void Redeem(CheatCode code)
			{
				code._redeemed = true;
				this.SaveCodes();
			}

			public CheatCode Redeem(string code, out string reason)
			{
				string cheatcode = code.ToUpper();
				reason = "Invalid Code";
				foreach (KeyValuePair<string, CheatCode> pair in this.Codes)
				{
					if (cheatcode == pair.Value.Cheatcode)
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

			private static string CodeFileName = "cdat.user";

			private SaveDevice _saveDevice;

			public Dictionary<string, CheatCode> Codes = new Dictionary<string, CheatCode>();
		}
	}
}
