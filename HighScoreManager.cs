using System;
using System.Collections.Generic;
using System.IO;

namespace DNA
{
	public class HighScoreManager<T> where T : PlayerStats, new()
	{
		public IList<T> Scores
		{
			get
			{
				return this._scores;
			}
		}

		public HighScoreManager(int maxScores, Comparison<T> comparer)
		{
			this.CompareScores = comparer;
		}

		public void DeleteGamer(string gamertag)
		{
			for (int i = 0; i < this._scores.Count; i++)
			{
				if (this._scores[i].GamerTag == gamertag)
				{
					this._scores.RemoveAt(i);
					i--;
				}
			}
		}

		public void UpdateScores(IList<T> newScores, T currentStats)
		{
			List<T> list = new List<T>();
			list.AddRange(newScores);
			currentStats.DateRecorded = DateTime.UtcNow;
			list.Add(currentStats);
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			for (int i = 0; i < this._scores.Count; i++)
			{
				dictionary[this._scores[i].GamerTag] = i;
			}
			for (int j = 0; j < list.Count; j++)
			{
				T t = list[j];
				int num;
				if (dictionary.TryGetValue(t.GamerTag, out num))
				{
					if (this._scores[num].DateRecorded < t.DateRecorded)
					{
						this._scores[num] = t;
					}
				}
				else
				{
					this._scores.Add(t);
					dictionary[t.GamerTag] = this._scores.Count - 1;
				}
			}
			this._scores.Sort(this.CompareScores);
			if (this._scores.Count > this.MaxScores)
			{
				this._scores.RemoveRange(this.MaxScores, this._scores.Count - this.MaxScores);
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(this._scores.Count);
			for (int i = 0; i < this.Scores.Count; i++)
			{
				T t = this._scores[i];
				t.Save(writer);
			}
		}

		public void Load(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			this._scores.Clear();
			for (int i = 0; i < num; i++)
			{
				T t = new T();
				t.Load(reader);
				this._scores.Add(t);
			}
		}

		private int MaxScores = 100;

		private Comparison<T> CompareScores;

		private List<T> _scores = new List<T>();
	}
}
