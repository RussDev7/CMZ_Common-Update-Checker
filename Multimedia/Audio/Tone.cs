using System;
using DNA.Data.Units;

namespace DNA.Multimedia.Audio
{
	public struct Tone
	{
		public override string ToString()
		{
			return this.BaseNote.ToString() + this.Octive.ToString();
		}

		public int Octive
		{
			get
			{
				if (this._value < 0f)
				{
					int num = (int)Math.Ceiling((double)this._value);
					int num2 = num / 12;
					return num2 - 1;
				}
				int num3 = (int)Math.Floor((double)this._value);
				return num3 / 12;
			}
		}

		public string NoteName
		{
			get
			{
				switch (this.BaseNote)
				{
				case Notes.A:
					return "A";
				case Notes.Bb:
					return "B♭";
				case Notes.B:
					return "B";
				case Notes.C:
					return "C";
				case Notes.Db:
					return "C♯";
				case Notes.D:
					return "D";
				case Notes.Eb:
					return "E♭";
				case Notes.E:
					return "E";
				case Notes.F:
					return "F";
				case Notes.Gb:
					return "F♯";
				case Notes.G:
					return "G";
				case Notes.Ab:
					return "G♯";
				default:
					return "";
				}
			}
		}

		public Notes BaseNote
		{
			get
			{
				int num = (int)Math.Round((double)this._value);
				if (num < 0)
				{
					num = -num;
					num %= 12;
					num = 12 - num;
					return (Notes)(num % 12);
				}
				return (Notes)(num % 12);
			}
		}

		public float Detune
		{
			get
			{
				int num = (int)Math.Round((double)this._value);
				return this._value - (float)num;
			}
		}

		public float Value
		{
			get
			{
				return this._value;
			}
		}

		public int KeyValue
		{
			get
			{
				return (int)this._value;
			}
		}

		public Frequency Frequency
		{
			get
			{
				return Tone.GetNoteFrequency(this._value);
			}
		}

		public static Tone FromKeyIndex(int value)
		{
			return new Tone((float)value);
		}

		public static Tone FromNote(Notes note, int octive)
		{
			if (octive < 0)
			{
				octive++;
			}
			float num = (float)note + (float)(12 * octive);
			return new Tone(num);
		}

		private static float NoteFromFrequency(Frequency frequency)
		{
			return (float)(12.0 * Math.Log((double)(frequency.Hertz / 440f), 2.0));
		}

		public static Tone FromFrequency(Frequency frequency)
		{
			return new Tone(Tone.NoteFromFrequency(frequency));
		}

		public static Tone Parse(string value)
		{
			int num = value.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
			int num2 = 5;
			string text2;
			if (num > 0)
			{
				string text = value.Substring(num);
				num2 = int.Parse(text);
				text2 = value.Substring(0, num);
			}
			else
			{
				text2 = value;
			}
			text2 = text2.Trim();
			Notes notes;
			try
			{
				notes = (Notes)Enum.Parse(typeof(Notes), text2, true);
			}
			catch
			{
				string text3;
				if ((text3 = text2) != null)
				{
					if (!(text3 == "C#"))
					{
						if (!(text3 == "D#"))
						{
							if (!(text3 == "F#"))
							{
								if (!(text3 == "G#"))
								{
									if (!(text3 == "A#"))
									{
										goto IL_00C6;
									}
									notes = Notes.Bb;
								}
								else
								{
									notes = Notes.Ab;
								}
							}
							else
							{
								notes = Notes.Gb;
							}
						}
						else
						{
							notes = Notes.Eb;
						}
					}
					else
					{
						notes = Notes.Db;
					}
					goto IL_00D3;
				}
				IL_00C6:
				throw new FormatException("Invalid Note");
			}
			IL_00D3:
			return Tone.FromNote(notes, num2);
		}

		private Tone(float noteNumber)
		{
			if (float.IsNaN(noteNumber))
			{
				this._value = 0f;
			}
			this._value = noteNumber;
		}

		private static Frequency GetNoteFrequency(float note)
		{
			return Frequency.FromHertz((float)(440.0 * Math.Pow(2.0, (double)note / 12.0)));
		}

		public override int GetHashCode()
		{
			return this._value.GetHashCode();
		}

		public bool Equals(Tone other)
		{
			return this._value == other._value;
		}

		public override bool Equals(object obj)
		{
			return obj.GetType() == typeof(Tone) && this.Equals((Tone)obj);
		}

		public static bool operator ==(Tone a, Tone b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Tone a, Tone b)
		{
			return !a.Equals(b);
		}

		private const int NotesPerOctive = 12;

		private const float BaseTone = 440f;

		private float _value;
	}
}
