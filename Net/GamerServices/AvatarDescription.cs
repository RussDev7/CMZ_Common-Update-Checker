using System;

namespace DNA.Net.GamerServices
{
	public class AvatarDescription
	{
		public AvatarDescription(byte[] data)
		{
			this._data = data;
		}

		public AvatarBodyType BodyType
		{
			get
			{
				return this._bodyType;
			}
		}

		public byte[] Description
		{
			get
			{
				return this._data;
			}
		}

		public float Height
		{
			get
			{
				return this._height;
			}
		}

		public bool IsValid
		{
			get
			{
				return this._data != null;
			}
		}

		public event EventHandler<EventArgs> Changed;

		public static IAsyncResult BeginGetFromGamer(Gamer gamer, AsyncCallback callback, object state)
		{
			throw new NotImplementedException();
		}

		public static AvatarDescription CreateRandom()
		{
			throw new NotImplementedException();
		}

		public static AvatarDescription CreateRandom(AvatarBodyType bodyType)
		{
			throw new NotImplementedException();
		}

		public static AvatarDescription EndGetFromGamer(IAsyncResult result)
		{
			throw new NotImplementedException();
		}

		private byte[] _data;

		private AvatarBodyType _bodyType = AvatarBodyType.Male;

		private float _height = 1.6f;
	}
}
