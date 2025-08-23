using System;
using System.Collections.Generic;
using System.IO;

namespace DNA.IO
{
	public class HTFDocument
	{
		public HTFElement Root
		{
			get
			{
				return this._root;
			}
			set
			{
				this._root = value;
			}
		}

		public List<HTFElement> Children
		{
			get
			{
				return this._root.Children;
			}
		}

		public HTFDocument()
		{
			this._root = new HTFElement();
		}

		public HTFDocument(string fileName)
		{
			using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				this.Load(stream);
			}
		}

		public HTFDocument(Stream stream)
		{
			this.Load(stream);
		}

		public void Load(Stream stream)
		{
			this._root = new HTFElement(stream);
		}

		public void Load(string path)
		{
			using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				this._root = new HTFElement(stream);
			}
		}

		public void Save(string filename)
		{
			using (FileStream stream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				this.Save(stream);
			}
		}

		public void Save(Stream stream)
		{
			StreamWriter writer = new StreamWriter(stream);
			this.Save(writer);
		}

		public void Save(StreamWriter writer)
		{
			this._root.Save(writer);
			writer.Flush();
		}

		public override string ToString()
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			this.Save(writer);
			stream.Position = 0L;
			StreamReader reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		public void LoadFromString(string data)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(data);
			writer.Flush();
			stream.Position = 0L;
			this.Load(stream);
		}

		private HTFElement _root;
	}
}
