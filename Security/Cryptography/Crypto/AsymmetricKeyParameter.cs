using System;

namespace DNA.Security.Cryptography.Crypto
{
	public class AsymmetricKeyParameter : ICipherParameters
	{
		public AsymmetricKeyParameter(bool privateKey)
		{
			this.privateKey = privateKey;
		}

		public bool IsPrivate
		{
			get
			{
				return this.privateKey;
			}
		}

		public override bool Equals(object obj)
		{
			AsymmetricKeyParameter other = obj as AsymmetricKeyParameter;
			return other != null && this.Equals(other);
		}

		protected bool Equals(AsymmetricKeyParameter other)
		{
			return this.privateKey == other.privateKey;
		}

		public override int GetHashCode()
		{
			return this.privateKey.GetHashCode();
		}

		private readonly bool privateKey;
	}
}
