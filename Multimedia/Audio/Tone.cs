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
					int noteIndex = (int)Math.Ceiling((double)this._value);
					int octive = noteIndex / 12;
					return octive - 1;
				}
				int noteIndex2 = (int)Math.Floor((double)this._value);
				return noteIndex2 / 12;
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
				int noteIndex = (int)Math.Round((double)this._value);
				if (noteIndex < 0)
				{
					noteIndex = -noteIndex;
					noteIndex %= 12;
					noteIndex = 12 - noteIndex;
					return (Notes)(noteIndex % 12);
				}
				return (Notes)(noteIndex % 12);
			}
		}

		public float Detune
		{
			get
			{
				int noteIndex = (int)Math.Round((double)this._value);
				return this._value - (float)noteIndex;
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
			float value = (float)note + (float)(12 * octive);
			return new Tone(value);
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
			int index = value.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
			int octive = 5;
			string noteText;
			if (index > 0)
			{
				string octiveText = value.Substring(index);
				octive = int.Parse(octiveText);
				noteText = value.Substring(0, index);
			}
			else
			{
				noteText = value;
			}
			noteText = noteText.Trim();
			Notes noteIndex;
			try
			{
				noteIndex = (Notes)Enum.Parse(typeof(Notes), noteText, true);
			}
			catch
			{
				string text;
				if ((text = noteText) != null)
				{
					if (!(text == "C#"))
					{
						if (!(text == "D#"))
						{
							if (!(text == "F#"))
							{
								if (!(text == "G#"))
								{
									if (!(text == "A#"))
									{
										goto IL_00C6;
									}
									noteIndex = Notes.Bb;
								}
								else
								{
									noteIndex = Notes.Ab;
								}
							}
							else
							{
								noteIndex = Notes.Gb;
							}
						}
						else
						{
							noteIndex = Notes.Eb;
						}
					}
					else
					{
						noteIndex = Notes.Db;
					}
					goto IL_00D3;
				}
				IL_00C6:
				throw new FormatException("Invalid Note");
			}
			IL_00D3:
			return Tone.FromNote(noteIndex, octive);
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
