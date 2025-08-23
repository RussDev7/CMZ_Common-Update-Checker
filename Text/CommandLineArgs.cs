using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DNA.Text
{
	public class CommandLineArgs
	{
		protected virtual void ValidateSettings()
		{
		}

		private static List<string> TokenizeCommandLine()
		{
			List<string> result = new List<string>();
			string cl = Environment.CommandLine;
			if (cl == null || cl.Length == 0)
			{
				return result;
			}
			bool escaped = false;
			bool openQuote = false;
			bool doubleQuote = false;
			bool wasQuoted = false;
			StringBuilder accumulator = new StringBuilder();
			foreach (char c in cl)
			{
				if (escaped)
				{
					accumulator.Append(c);
					escaped = false;
				}
				else if (openQuote)
				{
					if ((doubleQuote && c == '"') || (!doubleQuote && c == '\''))
					{
						openQuote = false;
						wasQuoted = true;
					}
					else
					{
						accumulator.Append(c);
					}
				}
				else if (c == '\\' && CommandLineArgs.ApplicationPath != null)
				{
					escaped = true;
				}
				else if (c == '"')
				{
					openQuote = true;
					doubleQuote = true;
				}
				else if (c == '\'')
				{
					openQuote = true;
					doubleQuote = false;
				}
				else if (!char.IsWhiteSpace(c))
				{
					accumulator.Append(c);
					wasQuoted = false;
				}
				else
				{
					string token = accumulator.ToString();
					if (CommandLineArgs.ApplicationPath == null)
					{
						if (wasQuoted || File.Exists(token) || File.Exists(token + ".exe"))
						{
							CommandLineArgs.ApplicationPath = token;
							accumulator.Length = 0;
						}
						else
						{
							accumulator.Append(c);
						}
					}
					else if (!string.IsNullOrEmpty(token))
					{
						result.Add(token);
						accumulator.Length = 0;
					}
					wasQuoted = false;
				}
			}
			if (CommandLineArgs.ApplicationPath == null)
			{
				CommandLineArgs.ApplicationPath = accumulator.ToString();
			}
			else if (accumulator.Length != 0)
			{
				result.Add(accumulator.ToString());
			}
			return result;
		}

		public static void ProcessArguments()
		{
			if (CommandLineArgs._commandLineCache != null)
			{
				return;
			}
			CommandLineArgs._sCommandLineTokens = CommandLineArgs.TokenizeCommandLine();
			CommandLineArgs._commandLineCache = new Dictionary<Type, CommandLineArgs>();
			CommandLineArgs._registeredArgs = new List<CommandLineArgs.ArgumentDefinition>();
			Type argType = typeof(CommandLineArgs);
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				try
				{
					Type[] types = assembly.GetTypes();
					foreach (Type type in types)
					{
						if (type.IsSubclassOf(argType) && !type.Equals(argType))
						{
							CommandLineArgs arg = Activator.CreateInstance(type) as CommandLineArgs;
							if (arg != null)
							{
								arg.RegisterArguments();
							}
							CommandLineArgs._commandLineCache[type] = arg;
						}
					}
				}
				catch (Exception)
				{
				}
			}
			foreach (CommandLineArgs arg2 in CommandLineArgs._commandLineCache.Values)
			{
				if (arg2 != null)
				{
					arg2.ParseArguments();
				}
			}
		}

		public static T Get<T>() where T : CommandLineArgs
		{
			CommandLineArgs result = null;
			if (CommandLineArgs._commandLineCache == null)
			{
				throw new Exception("Command line requested but arguments have not been processed");
			}
			if (!CommandLineArgs._commandLineCache.TryGetValue(typeof(T), out result))
			{
				throw new Exception("Command line type " + typeof(T).Name + " referenced but does not exist");
			}
			return result as T;
		}

		protected int AddFlag(string flag, int numargs, CommandLineArgs.ArgumentHandler handler, string description = "", string sample = "", StringComparison compType = StringComparison.OrdinalIgnoreCase)
		{
			this._argumentDefinitions.Add(new CommandLineArgs.ArgumentDefinition(flag, numargs, handler, description, sample, compType));
			return this._argumentDefinitions.Count;
		}

		public virtual string GetErrorUsageAndDescription()
		{
			StringBuilder sb = new StringBuilder();
			if (this.ErrorString != null)
			{
				sb.Append(this.ErrorString).Append("\n").Append("\n");
			}
			this.GetUsageLine(sb);
			sb.Append("\n\n");
			string extra = this.UsagePreamble;
			if (extra != null)
			{
				sb.Append(extra);
			}
			this.GetUsageDescription(sb);
			sb.Append("\n");
			extra = this.UsageEpilogue;
			if (extra != null)
			{
				sb.Append(extra);
			}
			sb.Append("\n");
			return sb.ToString();
		}

		public virtual string GetUsageLine()
		{
			StringBuilder sb = new StringBuilder();
			this.GetUsageLine(sb);
			return sb.ToString();
		}

		public virtual void GetUsageLine(StringBuilder sb)
		{
			string appName = null;
			if (!string.IsNullOrEmpty(CommandLineArgs.ApplicationPath))
			{
				appName = Path.GetFileName(CommandLineArgs.ApplicationPath);
			}
			if (string.IsNullOrEmpty(appName))
			{
				appName = "usage:";
			}
			sb.Append(appName + " ");
			CommandLineArgs.ArgumentDefinition defHandler = null;
			foreach (CommandLineArgs.ArgumentDefinition def in this._argumentDefinitions)
			{
				if (def.Flag != null)
				{
					if (!string.IsNullOrEmpty(def.Sample))
					{
						sb.Append(" ").Append(def.Sample);
					}
				}
				else
				{
					defHandler = def;
				}
			}
			if (defHandler != null && !string.IsNullOrEmpty(defHandler.Sample))
			{
				sb.Append(" ").Append(defHandler.Sample);
			}
		}

		public virtual string GetUsageDescription()
		{
			StringBuilder sb = new StringBuilder();
			this.GetUsageDescription(sb);
			return sb.ToString();
		}

		public virtual void GetUsageDescription(StringBuilder sb)
		{
			CommandLineArgs.ArgumentDefinition defHandler = null;
			foreach (CommandLineArgs.ArgumentDefinition def in this._argumentDefinitions)
			{
				if (def.Flag != null)
				{
					if (!string.IsNullOrEmpty(def.Description))
					{
						sb.Append("\t").Append(def.Flag).Append(" - ")
							.Append(def.Description)
							.Append("\n");
					}
				}
				else
				{
					defHandler = def;
				}
			}
			if (defHandler != null && !string.IsNullOrEmpty(defHandler.Sample))
			{
				sb.Append("\t").Append(defHandler.Description).Append("\n");
			}
		}

		protected virtual string UsagePreamble
		{
			get
			{
				return null;
			}
		}

		protected virtual string UsageEpilogue
		{
			get
			{
				return null;
			}
		}

		protected virtual void HandleUncaughtArg(string arg)
		{
			this.ShowUsage = true;
			this.ErrorString = "Unknown argument: " + arg;
		}

		protected virtual void RegisterArguments()
		{
			foreach (CommandLineArgs.ArgumentDefinition def in this._argumentDefinitions)
			{
				if (def.Flag != null)
				{
					bool alreadyIn = false;
					int i = 0;
					while (i < CommandLineArgs._registeredArgs.Count)
					{
						if (CommandLineArgs._registeredArgs[i].EquivilentTo(def))
						{
							if (CommandLineArgs._registeredArgs[i].NumArgs != def.NumArgs)
							{
								CommandLineArgs._registeredArgs.RemoveAt(i);
								break;
							}
							alreadyIn = true;
							break;
						}
						else
						{
							i++;
						}
					}
					if (!alreadyIn)
					{
						CommandLineArgs._registeredArgs.Add(def);
					}
				}
			}
		}

		protected virtual bool IsAFlag(string val)
		{
			foreach (CommandLineArgs.ArgumentDefinition def in CommandLineArgs._registeredArgs)
			{
				if (def.Matches(val))
				{
					return true;
				}
			}
			return false;
		}

		protected virtual bool CheckFlag(List<CommandLineArgs.ArgumentDefinition> definitions, List<string> args, bool runHandlers)
		{
			bool handled = false;
			foreach (CommandLineArgs.ArgumentDefinition def in definitions)
			{
				if (def.Matches(args[0]))
				{
					handled = true;
					string flag = args[0];
					args.RemoveAt(0);
					if (def.NumArgs == -1)
					{
						if (runHandlers)
						{
							def.Handler(flag, args);
							break;
						}
						break;
					}
					else if (def.NumArgs == -2)
					{
						List<string> newargs = null;
						while (args.Count != 0 && !this.IsAFlag(args[0]))
						{
							if (runHandlers)
							{
								if (newargs == null)
								{
									newargs = new List<string>();
								}
								newargs.Add(args[0]);
							}
							args.RemoveAt(0);
						}
						if (runHandlers)
						{
							def.Handler(flag, newargs);
							break;
						}
						break;
					}
					else
					{
						List<string> newargs2 = null;
						if (def.NumArgs > args.Count)
						{
							if (runHandlers)
							{
								this.ShowUsage = true;
								this.ErrorString = "Not enough arguments for " + flag + " flag";
								break;
							}
							break;
						}
						else
						{
							if (def.NumArgs > 0)
							{
								if (runHandlers)
								{
									newargs2 = new List<string>();
									for (int i = 0; i < def.NumArgs; i++)
									{
										newargs2.Add(args[i]);
									}
								}
								args.RemoveRange(0, def.NumArgs);
							}
							if (runHandlers)
							{
								def.Handler(flag, newargs2);
								break;
							}
							break;
						}
					}
				}
			}
			return handled;
		}

		protected virtual bool ParseArguments()
		{
			this.ShowUsage = false;
			List<string> args = new List<string>(CommandLineArgs._sCommandLineTokens);
			CommandLineArgs.ArgumentDefinition defaultDef = null;
			using (List<CommandLineArgs.ArgumentDefinition>.Enumerator enumerator = this._argumentDefinitions.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CommandLineArgs.ArgumentDefinition def = enumerator.Current;
					if (def.Flag == null)
					{
						defaultDef = def;
						break;
					}
				}
				goto IL_00AE;
			}
			IL_0050:
			if (!this.CheckFlag(this._argumentDefinitions, args, true))
			{
				if (defaultDef != null)
				{
					defaultDef.Handler(args[0], null);
					args.RemoveAt(0);
				}
				else if (!this.CheckFlag(CommandLineArgs._registeredArgs, args, false))
				{
					this.HandleUncaughtArg(args[0]);
					args.RemoveAt(0);
				}
			}
			if (this.ShowUsage)
			{
				return false;
			}
			IL_00AE:
			if (args == null || args.Count <= 0)
			{
				this.ValidateSettings();
				return !this.ShowUsage;
			}
			goto IL_0050;
		}

		protected const int cAllRemainingArgs = -1;

		protected const int cArgsUpToNextFlag = -2;

		private List<CommandLineArgs.ArgumentDefinition> _argumentDefinitions = new List<CommandLineArgs.ArgumentDefinition>();

		public bool ShowUsage = true;

		public string ErrorString;

		public static string ApplicationPath;

		private static Dictionary<Type, CommandLineArgs> _commandLineCache;

		private static List<CommandLineArgs.ArgumentDefinition> _registeredArgs;

		private static List<string> _sCommandLineTokens;

		protected delegate void ArgumentHandler(string flag, List<string> arguments);

		protected class ArgumentDefinition
		{
			public ArgumentDefinition(string flag, int numargs, CommandLineArgs.ArgumentHandler handler, string description, string sample, StringComparison comparisonType)
			{
				this.Flag = flag;
				this.NumArgs = numargs;
				this.Description = description;
				this.Sample = sample;
				this.Handler = handler;
				this.ComparisonType = comparisonType;
			}

			public bool Matches(string flag)
			{
				return this.Flag != null && this.Flag.Equals(flag, this.ComparisonType);
			}

			public bool EquivilentTo(CommandLineArgs.ArgumentDefinition def)
			{
				return this.Flag != null && def.Flag != null && (this.Flag.Equals(def.Flag, this.ComparisonType) || this.Flag.Equals(def.Flag, def.ComparisonType));
			}

			public override string ToString()
			{
				string result = ((this.Flag == null) ? "null" : this.Flag);
				string text = result;
				return string.Concat(new string[]
				{
					text,
					": ",
					this.Description,
					" (",
					this.NumArgs.ToString(),
					")"
				});
			}

			public string Flag;

			public int NumArgs;

			public string Description;

			public string Sample;

			public CommandLineArgs.ArgumentHandler Handler;

			public StringComparison ComparisonType;
		}
	}
}
