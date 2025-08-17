using System;
using System.Collections.Generic;
using DNA.Distribution;
using Microsoft.Xna.Framework;

namespace DNA.Net.GamerServices
{
	public class GamerServicesComponent : GameComponent
	{
		internal SignedInGamerCollection SignedInGamers
		{
			get
			{
				return this._signedInGamerCollection;
			}
		}

		public GamerServicesComponent(DNAGame game, string gameName)
			: base(game)
		{
			GamerServicesComponent.Instance = this;
			this._gameName = gameName;
		}

		public SignedInGamer LoadDefaultGamer()
		{
			return new SignedInGamer(PlayerIndex.One, DNAGame.GetLocalID(), OnlineServices.Instance.Username);
		}

		public override void Initialize()
		{
			this._signedInGamers.Add(this.LoadDefaultGamer());
			this._signedInGamerCollection = new SignedInGamerCollection(this._signedInGamers);
		}

		private SignedInGamer GetCurrentPlayer()
		{
			return this._signedInGamers[0];
		}

		public override void Update(GameTime gameTime)
		{
		}

		public static GamerServicesComponent Instance;

		private List<SignedInGamer> _signedInGamers = new List<SignedInGamer>();

		private SignedInGamerCollection _signedInGamerCollection;

		private string _gameName = "DigitalDNA";
	}
}
