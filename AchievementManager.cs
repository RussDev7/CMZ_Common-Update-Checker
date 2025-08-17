using System;
using System.Collections.Generic;

namespace DNA
{
	public abstract class AchievementManager<T> where T : PlayerStats
	{
		protected T PlayerStats
		{
			get
			{
				return this._stats;
			}
		}

		public AchievementManager<T>.Achievement this[int index]
		{
			get
			{
				return this._acheviements[index];
			}
		}

		public int Count
		{
			get
			{
				return this._acheviements.Count;
			}
		}

		public int AddAcheivement(AchievementManager<T>.Achievement achievement)
		{
			this._acheviements.Add(achievement);
			return this._acheviements.Count - 1;
		}

		public AchievementManager(T stats)
		{
			this._stats = stats;
			this.CreateAcheivements();
			for (int i = 0; i < this._acheviements.Count; i++)
			{
				this._acheviements[i].Update();
			}
		}

		public abstract void CreateAcheivements();

		public void Update()
		{
			for (int i = 0; i < this._acheviements.Count; i++)
			{
				if (this._acheviements[i].Update())
				{
					this.OnAchieved(this._acheviements[i]);
					if (this.Achieved != null)
					{
						this.Achieved(this, new AchievementManager<T>.AcheimentEventArgs(this._acheviements[i]));
					}
				}
			}
		}

		public virtual void OnAchieved(AchievementManager<T>.Achievement acheivement)
		{
		}

		public event EventHandler<AchievementManager<T>.AcheimentEventArgs> Achieved;

		private T _stats;

		private List<AchievementManager<T>.Achievement> _acheviements = new List<AchievementManager<T>.Achievement>();

		public abstract class Achievement
		{
			protected T PlayerStats
			{
				get
				{
					return this._stats;
				}
			}

			public string HowToUnlock
			{
				get
				{
					return this._howToUnlock;
				}
			}

			public string Reward
			{
				get
				{
					return this._reward;
				}
			}

			public Achievement(string apiName, AchievementManager<T> manager, string name, string howToUnlock)
			{
				this._apiName = apiName;
				this._name = name;
				this._howToUnlock = howToUnlock;
				this._stats = manager._stats;
			}

			public Achievement(string apiName, AchievementManager<T> manager, string name, string howToUnlock, string reward)
			{
				this._apiName = apiName;
				this._name = name;
				this._howToUnlock = howToUnlock;
				this._stats = manager._stats;
				this._reward = reward;
			}

			public Achievement(AchievementManager<T> manager, string name, string howToUnlock)
			{
				this._apiName = name;
				this._name = name;
				this._howToUnlock = howToUnlock;
				this._stats = manager._stats;
			}

			public Achievement(AchievementManager<T> manager, string name, string howToUnlock, string reward)
			{
				this._apiName = name;
				this._name = name;
				this._howToUnlock = howToUnlock;
				this._stats = manager._stats;
				this._reward = reward;
			}

			public string APIName
			{
				get
				{
					return this._apiName;
				}
			}

			public virtual string Name
			{
				get
				{
					return this._name;
				}
			}

			public virtual bool Acheived
			{
				get
				{
					return this._acheived;
				}
			}

			protected abstract bool IsSastified { get; }

			public abstract string ProgressTowardsUnlockMessage { get; }

			public virtual float ProgressTowardsUnlock
			{
				get
				{
					return 0f;
				}
			}

			public virtual bool Update()
			{
				if (!this._acheived && this.IsSastified)
				{
					this._acheived = true;
					return true;
				}
				return false;
			}

			private string _apiName;

			private string _name;

			private string _howToUnlock;

			protected bool _acheived;

			private T _stats;

			private string _reward;

			public object Tag;
		}

		public class AcheimentEventArgs : EventArgs
		{
			public AcheimentEventArgs(AchievementManager<T>.Achievement achievement)
			{
				this.Achievement = achievement;
			}

			public AchievementManager<T>.Achievement Achievement;
		}
	}
}
