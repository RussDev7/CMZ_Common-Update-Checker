using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using DNA.Audio;
using DNA.Diagnostics.IssueReporting;
using DNA.Distribution;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.Multimedia.Broadcasting;
using DNA.Net;
using DNA.Net.GamerServices;
using DNA.Profiling;
using DNA.Security.Cryptography;
using DNA.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA
{
	public class DNAGame : Game
	{
		public virtual OnlineServices LicenseServices
		{
			get
			{
				return this._licenseServices;
			}
			set
			{
				this._licenseServices = value;
			}
		}

		public Version Version
		{
			get
			{
				return this._version;
			}
		}

		public RenderTarget2D OffScreenBuffer
		{
			get
			{
				return this._offscreenBuffer;
			}
		}

		public virtual string ServerMessage
		{
			get
			{
				if (this._networkSession != null)
				{
					return this._networkSession.ServerMessage;
				}
				return null;
			}
			set
			{
				if (this._networkSession != null)
				{
					this._networkSession.ServerMessage = value;
				}
			}
		}

		public void CreateOffscreenBuffer(int width, int height)
		{
			if (this._offscreenBuffer != null)
			{
				this._offscreenBuffer.Dispose();
				this._offscreenBuffer = null;
			}
			PresentationParameters pp = base.GraphicsDevice.PresentationParameters;
			this._offscreenBuffer = new RenderTarget2D(base.GraphicsDevice, width, height, false, pp.BackBufferFormat, pp.DepthStencilFormat, 1, RenderTargetUsage.PlatformContents);
		}

		public ScreenGroup ScreenManager
		{
			get
			{
				return this._screenManager;
			}
		}

		public bool Loading
		{
			get
			{
				return this._loadStatus != DNAGame.LoadStatus.Complete;
			}
		}

		public GameTime CurrentGameTime
		{
			get
			{
				return this._currentGameTime;
			}
			set
			{
				GameTime v;
				if (!this.LimitElapsedGameTime || value.ElapsedGameTime <= TimeSpan.FromSeconds(0.1))
				{
					v = value;
				}
				else
				{
					v = new GameTime(value.TotalGameTime, TimeSpan.FromSeconds(0.1), true);
				}
				Interlocked.Exchange<GameTime>(ref this._currentGameTime, v);
			}
		}

		public void WaitforSave()
		{
		}

		public NetworkSession CurrentNetworkSession
		{
			get
			{
				return this._networkSession;
			}
		}

		protected void StartVoiceChat(LocalNetworkGamer gamer)
		{
			this._voiceChat = new VoiceChat(gamer);
		}

		protected void ShowSignIn()
		{
			this.DialogManager.ShowSignIn(false);
		}

		public void ShowMarketPlace()
		{
			this.DialogManager.ShowMarketPlace(Screen.SelectedPlayerIndex.Value);
		}

		public void ShowMarketPlace(PlayerIndex player)
		{
			this.DialogManager.ShowMarketPlace(player);
		}

		private string GetLocalizedAssetName(string assetName)
		{
			string[] cultureNames = new string[]
			{
				CultureInfo.CurrentCulture.Name,
				CultureInfo.CurrentCulture.TwoLetterISOLanguageName
			};
			foreach (string cultureName in cultureNames)
			{
				string localizedAssetName = assetName + '.' + cultureName;
				string localizedAssetPath = Path.Combine(base.Content.RootDirectory, localizedAssetName + ".xnb");
				if (File.Exists(localizedAssetPath))
				{
					return localizedAssetName;
				}
			}
			return assetName;
		}

		public Texture2D LoadLocalizedImage(string name)
		{
			string locName = this.GetLocalizedAssetName(name);
			return base.Content.Load<Texture2D>(locName);
		}

		public virtual void StartGamerServices()
		{
			base.Components.Add(new GamerServicesComponent(this, "DigitalDNA"));
			this.HasGamerServices = true;
		}

		public DNAGame(Size authoredSize, bool PreferMultiSampling, Version version)
		{
			this._startupThread = Thread.CurrentThread;
			this.InputManager = new InputManager(this);
			this._version = version;
			this.TaskScheduler.ThreadException += this.TaskScheduler_ThreadException;
			this.DialogManager = new DialogManager(this);
			this.Graphics = new GraphicsDeviceManager(this);
			GraphicsDeviceLocker.Create(this.Graphics);
			base.Content.RootDirectory = "Content";
			Screen.PlayerSignedIn += this.Screen_PlayerSignedIn;
			Screen.PlayerSignedOut += this.Screen_PlayerSignedOut;
			Screen.Adjuster.AuthoredSize = new Size(1280, 720);
			this.Graphics.PreferredBackBufferWidth = authoredSize.Width;
			this.Graphics.PreferredBackBufferHeight = authoredSize.Height;
			Screen.Adjuster.ScreenSize = authoredSize;
			this.Graphics.PreferMultiSampling = PreferMultiSampling;
			base.Window.ClientSizeChanged += this.Window_ClientSizeChanged;
		}

		public void ChangeScreenSize(Size authoredSize)
		{
			this.Graphics.PreferredBackBufferWidth = authoredSize.Width;
			this.Graphics.PreferredBackBufferHeight = authoredSize.Height;
			Screen.Adjuster.ScreenSize = authoredSize;
			this.Graphics.ApplyChanges();
		}

		private void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			if (this._offscreenBuffer != null)
			{
				this.CreateOffscreenBuffer(base.GraphicsDevice.Viewport.Width, base.GraphicsDevice.Viewport.Height);
			}
			Screen.Adjuster.ScreenSize = new Size(base.GraphicsDevice.Viewport.Width, base.GraphicsDevice.Viewport.Height);
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			if (this.CurrentBroadcastStream != null)
			{
				this.CurrentBroadcastStream.Broadcasting = false;
				this.CurrentBroadcastStream.Dispose();
			}
			if (this.TaskScheduler != null)
			{
				this.TaskScheduler.Exit();
			}
			base.OnExiting(sender, args);
		}

		private void TaskScheduler_ThreadException(object sender, TaskScheduler.ExceptionEventArgs e)
		{
			this.CrashGame(e.InnerException);
		}

		protected void WantProfiling(bool fixTimeStep, bool syncRetrace)
		{
			Profiler.CreateComponent(this);
			base.IsFixedTimeStep = fixTimeStep;
			this.Graphics.SynchronizeWithVerticalRetrace = syncRetrace;
			Profiler.Profiling = true;
			Profiler.SetColor("Update", Color.DarkBlue);
			Profiler.SetColor("Physics", Color.DarkRed);
			Profiler.SetColor("Collision", Color.Chocolate);
			Profiler.SetColor("Drawing", Color.DarkGreen);
			Profiler.SetColor("UpdateTransform", Color.DarkGoldenrod);
			Profiler.SetColor("SetDefPose", Color.DarkGray);
			Profiler.SetColor("AnimPlrUpdate", Color.DarkOrange);
			Profiler.SetColor("CopyTforms", Color.DarkSlateBlue);
		}

		private void Screen_PlayerSignedOut(object sender, SignedOutEventArgs e)
		{
			this.OnPlayerSignedOut(e.Gamer);
		}

		private void Screen_PlayerSignedIn(object sender, SignedInEventArgs e)
		{
			this.OnPlayerSignedIn(e.Gamer);
		}

		protected virtual void OnPlayerSignedIn(SignedInGamer gamer)
		{
		}

		protected virtual void OnPlayerSignedOut(SignedInGamer gamer)
		{
		}

		public void LeaveGame()
		{
			bool endEvent = false;
			if (this._networkSession != null)
			{
				if (this._networkSession.AllowHostMigration)
				{
					endEvent = true;
				}
				this._networkSession.Dispose();
			}
			this._networkSession = null;
			if (endEvent)
			{
				this.OnSessionEnded(NetworkSessionEndReason.Disconnected);
			}
		}

		private void RegisterNetworkCallbacks(NetworkSession session)
		{
			session.GamerJoined += this._networkSession_GamerJoined;
			session.GamerLeft += this._networkSession_GamerLeft;
			session.GameEnded += this._networkSession_GameEnded;
			session.GameStarted += this._networkSession_GameStarted;
			session.HostChanged += this._networkSession_HostChanged;
			session.SessionEnded += this._networkSession_SessionEnded;
		}

		public void HostGame(NetworkSessionType sessionType, NetworkSessionProperties properties, IList<SignedInGamer> gamers, int maxPlayers, bool hostMigration, bool joinInprogress, SuccessCallback callback)
		{
			this.HostGame(sessionType, properties, gamers, maxPlayers, hostMigration, joinInprogress, callback, "XNAGame", 0, null, null);
		}

		public void HostGame(NetworkSessionType sessionType, NetworkSessionProperties properties, IList<SignedInGamer> gamers, int maxPlayers, bool hostMigration, bool joinInprogress, SuccessCallback callback, string gameName, int networkVersion, string serverMessage, string password)
		{
			this.processMessages = false;
			NetworkSession.BeginCreate(sessionType, gamers, maxPlayers, 0, properties, gameName, networkVersion, serverMessage, password, delegate(IAsyncResult result)
			{
				SuccessCallback clientCallback = (SuccessCallback)result.AsyncState;
				if (clientCallback != null && !result.IsCompleted)
				{
					clientCallback(false);
					this.processMessages = true;
					return;
				}
				try
				{
					this._networkSession = NetworkSession.EndCreate(result);
					this._networkSession.AllowHostMigration = hostMigration;
					this._networkSession.AllowJoinInProgress = joinInprogress;
					this.RegisterNetworkCallbacks(this._networkSession);
				}
				catch (Exception)
				{
					if (clientCallback != null)
					{
						clientCallback(false);
					}
					this.processMessages = true;
					return;
				}
				if (clientCallback != null)
				{
					clientCallback(true);
				}
				this.processMessages = true;
			}, callback);
		}

		public void JoinInvitedGame(ulong lobbyId, int version, string gameName, IList<SignedInGamer> gamers, SuccessCallbackWithMessage callback, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			string failureMessage = null;
			this.processMessages = false;
			try
			{
				NetworkSession.BeginJoinInvited(lobbyId, version, gameName, gamers, delegate(IAsyncResult result)
				{
					SuccessCallbackWithMessage clientCallback = (SuccessCallbackWithMessage)result.AsyncState;
					bool success = true;
					try
					{
						this._networkSession = NetworkSession.EndJoinInvited(result);
						this.RegisterNetworkCallbacks(this._networkSession);
					}
					catch (Exception ex)
					{
						failureMessage = ex.Message;
						this.LeaveGame();
						success = false;
					}
					if (clientCallback != null)
					{
						clientCallback(success, failureMessage);
					}
					this.processMessages = true;
				}, callback, getPasswordCallback);
			}
			catch (Exception e)
			{
				if (callback != null)
				{
					callback(false, e.Message);
				}
			}
		}

		public void JoinGame(AvailableNetworkSession session, SuccessCallback callback)
		{
			this.JoinGame(session, null, callback, "XNAGame", 0, null);
		}

		public void JoinGame(AvailableNetworkSession session, IList<SignedInGamer> gamers, SuccessCallbackWithMessage callback, string gameName, int version, string password)
		{
			this.processMessages = false;
			string failureMessage = null;
			NetworkSession.BeginJoin(session, gameName, version, password, gamers, delegate(IAsyncResult result)
			{
				bool success = true;
				SuccessCallbackWithMessage clientCallback = (SuccessCallbackWithMessage)result.AsyncState;
				try
				{
					this._networkSession = NetworkSession.EndJoin(result);
					this.RegisterNetworkCallbacks(this._networkSession);
				}
				catch (Exception ex)
				{
					failureMessage = ex.Message;
					this.LeaveGame();
					success = false;
				}
				if (clientCallback != null)
				{
					clientCallback(success, failureMessage);
				}
				this.processMessages = true;
			}, callback);
		}

		public void JoinGame(AvailableNetworkSession session, IList<SignedInGamer> gamers, SuccessCallback callback, string gameName, int version, string password)
		{
			this.processMessages = false;
			NetworkSession.BeginJoin(session, gameName, version, password, gamers, delegate(IAsyncResult result)
			{
				bool success = true;
				SuccessCallback clientCallback = (SuccessCallback)result.AsyncState;
				try
				{
					this._networkSession = NetworkSession.EndJoin(result);
					this.RegisterNetworkCallbacks(this._networkSession);
				}
				catch (Exception)
				{
					this.LeaveGame();
					success = false;
				}
				if (clientCallback != null)
				{
					clientCallback(success);
				}
				this.processMessages = true;
			}, callback);
		}

		private void _networkSession_SessionEnded(object sender, NetworkSessionEndedEventArgs e)
		{
			this.OnSessionEnded(e.EndReason);
		}

		public virtual void OnSessionEnded(NetworkSessionEndReason reason)
		{
		}

		private void _networkSession_HostChanged(object sender, HostChangedEventArgs e)
		{
			this.OnHostChanged(e.OldHost, e.NewHost);
		}

		public virtual void OnHostChanged(NetworkGamer oldHost, NetworkGamer newHost)
		{
		}

		private void _networkSession_GameStarted(object sender, GameStartedEventArgs e)
		{
			this.OnGameStarted();
		}

		public virtual void OnGameStarted()
		{
		}

		private void _networkSession_GameEnded(object sender, GameEndedEventArgs e)
		{
			this.OnGameEnded();
		}

		public virtual void OnGameEnded()
		{
		}

		private void _networkSession_GamerJoined(object sender, GamerJoinedEventArgs e)
		{
			this._currentPlayers[e.Gamer.Id] = e.Gamer;
			this.OnGamerJoined(e.Gamer);
		}

		protected virtual void OnGamerJoined(NetworkGamer gamer)
		{
		}

		private void _networkSession_GamerLeft(object sender, GamerLeftEventArgs e)
		{
			this._currentPlayers.Remove(e.Gamer.Id);
			this.OnGamerLeft(e.Gamer);
		}

		protected virtual void OnGamerLeft(NetworkGamer gamer)
		{
		}

		[DllImport("user32.dll")]
		public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

		protected override void Initialize()
		{
			if (!this._fixedWindow)
			{
				this._fixedWindow = true;
				if (this.Graphics.GraphicsDevice.DisplayMode.Width < 1281 || this.Graphics.GraphicsDevice.DisplayMode.Height < 721)
				{
					DNAGame.SendMessage(base.Window.Handle.ToInt32(), 274U, 61488, 0);
				}
			}
			KeyGrabber.InboundCharEvent += this.KeyGrabber_InboundCharEvent;
			this.DummyTexture = new Texture2D(base.GraphicsDevice, 1, 1);
			this.DummyTexture.SetData<Color>(new Color[] { Color.White });
			this.SpriteBatch = new SpriteBatch(base.GraphicsDevice);
			base.Initialize();
		}

		private void KeyGrabber_InboundCharEvent(char obj)
		{
			this._inboundKeys.Enqueue(obj);
		}

		protected override void LoadContent()
		{
			this.DebugFont = base.Content.Load<SpriteFont>("Debug");
			this.MousePointer = base.Content.Load<Texture2D>("MousePointer");
			this._loadStatus = DNAGame.LoadStatus.InProcess;
			this.TaskScheduler.QueueUserWorkItem(new ThreadStart(this.LoadThreadRoutine));
			base.LoadContent();
		}

		public void CrashGame(Exception e)
		{
			if (this.LastException == null)
			{
				this.LastException = e;
			}
			this.LastException.PreserveStackTrace();
		}

		private void LoadThreadRoutine()
		{
			Thread.CurrentThread.CurrentCulture = this._startupThread.CurrentCulture;
			Thread.CurrentThread.CurrentUICulture = this._startupThread.CurrentUICulture;
			this.SecondaryLoad();
			this._loadStatus = DNAGame.LoadStatus.Complete;
			this._doAfterLoad = true;
		}

		protected virtual void SecondaryLoad()
		{
		}

		protected virtual void SendNetworkUpdates(NetworkSession session, GameTime gameTime)
		{
		}

		protected virtual void LoadingUpdate(GameTime gameTime)
		{
		}

		public void SuspendSystemUpdates()
		{
			this._doSystemUpdates = false;
		}

		public void ResumeSystemUpdates()
		{
			this._doSystemUpdates = true;
		}

		private void FixNetworkBug()
		{
			if (this._networkSession != null)
			{
				List<KeyValuePair<byte, NetworkGamer>> toRemove = new List<KeyValuePair<byte, NetworkGamer>>();
				foreach (KeyValuePair<byte, NetworkGamer> pair in this._currentPlayers)
				{
					if (this.CurrentNetworkSession.FindGamerById(pair.Key) == null)
					{
						toRemove.Add(pair);
					}
				}
				foreach (KeyValuePair<byte, NetworkGamer> pair2 in toRemove)
				{
					this._currentPlayers.Remove(pair2.Key);
					this.OnGamerLeft(pair2.Value);
				}
			}
		}

		protected override void Update(GameTime gameTime)
		{
			if (this.LastException != null)
			{
				throw this.LastException;
			}
			if (this._doAfterLoad)
			{
				this.AfterLoad();
				this._doAfterLoad = false;
			}
			Profiler.MarkFrame();
			this.CurrentGameTime = gameTime;
			if (this.CurrentBroadcastStream != null)
			{
				this.CurrentBroadcastStream.Update(gameTime);
			}
			SoundManager.Instance.Update();
			if (this.Stop)
			{
				return;
			}
			bool guide = false;
			try
			{
				if (this.HasGamerServices && Guide.IsVisible)
				{
					guide = true;
				}
			}
			catch
			{
			}
			if (this.LicenseServices != null)
			{
				this.LicenseServices.Update(gameTime.ElapsedGameTime, gameTime.TotalGameTime);
			}
			if (!guide || !this.PauseDuringGuide)
			{
				if (this._firstFrame)
				{
					this.OnFirstFrame();
					this._firstFrame = false;
				}
				if (this.CurrentNetworkSession != null)
				{
					this.CurrentNetworkSession.Update();
					if (this.CurrentNetworkSession != null && this.processMessages)
					{
						this.ProcessNetworkMessages(this.CurrentGameTime);
						if (this._networkSession != null)
						{
							this.SendNetworkUpdates(this._networkSession, this.CurrentGameTime);
						}
					}
				}
				this.InputManager.Update();
				while (this._inboundKeys.Count != 0)
				{
					this.ScreenManager.ProcessChar(gameTime, this._inboundKeys.Dequeue());
				}
				this.ScreenManager.ProcessInput(this.InputManager, this.CurrentGameTime);
				this.ScreenManager.Update(this, this.CurrentGameTime);
				this.EvalCodes();
				if (this.LastException != null)
				{
					throw this.LastException;
				}
				if (this.ScreenManager.Exiting && !this.Loading)
				{
					base.Exit();
				}
			}
			if (this.HasGamerServices)
			{
				this.DialogManager.Update(this.CurrentGameTime);
			}
			if (this._doSystemUpdates)
			{
				base.Update(this.CurrentGameTime);
			}
			if (this.LastException != null)
			{
				throw this.LastException;
			}
		}

		public static PlayerID GetLocalID()
		{
			string address = DNAGame.GetMacAddress();
			byte[] data = Encoding.UTF8.GetBytes(address);
			MD5HashProvider provider = new MD5HashProvider();
			return new PlayerID(provider.Compute(data).Data);
		}

		private static string GetMacAddress()
		{
			string macAddresses = "";
			foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up)
				{
					macAddresses += nic.GetPhysicalAddress().ToString();
					break;
				}
			}
			return macAddresses;
		}

		protected void ShowTrialWarning(PlayerIndex player)
		{
			this.DialogManager.ShowMessageBox(player, "Full Mode Only", "This feature is only availible in the full version of the game.\n\nWould you like to purchase the game now?", new string[] { "Yes", "No" }, 0, MessageBoxIcon.Warning, delegate(int? index)
			{
				int valueOrDefault = index.GetValueOrDefault();
				if (index != null)
				{
					if (valueOrDefault != 0)
					{
						return;
					}
					this.ShowMarketPlace(player);
				}
			}, player);
		}

		private void EvalCodes()
		{
			if (Guide.IsTrialMode)
			{
				return;
			}
			DNAGame.CodeVal val = DNAGame.CodeVal.None;
			GameController controller = this.InputManager.Controllers[1];
			if (controller.PressedButtons.A)
			{
				val |= DNAGame.CodeVal.A;
			}
			if (controller.PressedButtons.B)
			{
				val |= DNAGame.CodeVal.B;
			}
			if (controller.PressedButtons.X)
			{
				val |= DNAGame.CodeVal.X;
			}
			if (controller.PressedButtons.Y)
			{
				val |= DNAGame.CodeVal.Y;
			}
			if (controller.PressedDPad.Up)
			{
				val |= DNAGame.CodeVal.Up;
			}
			if (controller.PressedDPad.Down)
			{
				val |= DNAGame.CodeVal.Down;
			}
			if (controller.PressedDPad.Left)
			{
				val |= DNAGame.CodeVal.Left;
			}
			if (controller.PressedDPad.Right)
			{
				val |= DNAGame.CodeVal.Right;
			}
			if (val != DNAGame.CodeVal.None)
			{
				this.recentCodes.Enqueue(val);
				while (this.recentCodes.Count > this.CodeLimit)
				{
					this.recentCodes.Dequeue();
				}
			}
			if (this.recentCodes.Count < this.konamiCode.Length)
			{
				return;
			}
			DNAGame.CodeVal[] codes = this.recentCodes.ToArray();
			for (int i = 0; i < this.konamiCode.Length; i++)
			{
				if (this.konamiCode[i] != codes[i])
				{
					return;
				}
			}
			this.recentCodes.Clear();
			this.CheatsEnabled = !this.CheatsEnabled;
		}

		protected virtual void OnFirstFrame()
		{
		}

		protected virtual void OnMessage(Message message)
		{
		}

		private void ProcessNetworkMessages(GameTime gameTime)
		{
			if (this._networkSession != null)
			{
				GamerCollection<LocalNetworkGamer> localGamers = this._networkSession.LocalGamers;
				if (localGamers != null)
				{
					int i = 0;
					while (this._networkSession != null && i < localGamers.Count)
					{
						LocalNetworkGamer localGamer = localGamers[i];
						while (this._networkSession != null && localGamer.IsDataAvailable)
						{
							try
							{
								Message message = Message.GetMessage(localGamer);
								if (message is VoiceChatMessage)
								{
									if (this._voiceChat != null)
									{
										this._voiceChat.ProcessMessage((VoiceChatMessage)message);
									}
								}
								else if (message.Echo || !message.Sender.IsLocal)
								{
									this.OnMessage(message);
								}
							}
							catch (InvalidMessageException ime)
							{
								if (this._networkSession.IsHost)
								{
									try
									{
										ime.Sender.Machine.RemoveFromSession();
									}
									catch
									{
									}
								}
								if (ime.Sender.IsHost)
								{
									this.LeaveGame();
								}
							}
						}
						i++;
					}
				}
			}
		}

		private void DrawBrightness()
		{
			Viewport viewport = base.GraphicsDevice.Viewport;
			this.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			this.SpriteBatch.Draw(this.DummyTexture, viewport.Bounds, new Color(this.Brightness, this.Brightness, this.Brightness));
			this.SpriteBatch.End();
		}

		private void DrawTitleSafeArea(GameTime gameTime)
		{
			Viewport viewport = base.GraphicsDevice.Viewport;
			Rectangle safeArea = viewport.TitleSafeArea;
			int viewportRight = viewport.X + viewport.Width;
			int viewportBottom = viewport.Y + viewport.Height;
			Rectangle leftBorder = new Rectangle(viewport.X, viewport.Y, safeArea.X - viewport.X, viewport.Height);
			Rectangle rightBorder = new Rectangle(safeArea.Right, viewport.Y, viewportRight - safeArea.Right, viewport.Height);
			Rectangle topBorder = new Rectangle(safeArea.Left, viewport.Y, safeArea.Width, safeArea.Top - viewport.Y);
			Rectangle bottomBorder = new Rectangle(safeArea.Left, safeArea.Bottom, safeArea.Width, viewportBottom - safeArea.Bottom);
			Color translucentRed = new Color(1f, 0f, 0f, 0.5f);
			this.SpriteBatch.Begin();
			this.SpriteBatch.Draw(this.DummyTexture, leftBorder, translucentRed);
			this.SpriteBatch.Draw(this.DummyTexture, rightBorder, translucentRed);
			this.SpriteBatch.Draw(this.DummyTexture, topBorder, translucentRed);
			this.SpriteBatch.Draw(this.DummyTexture, bottomBorder, translucentRed);
			this.frsb.Length = 0;
			int fps = (int)(1.0 / gameTime.ElapsedGameTime.TotalSeconds);
			this.frsb.Append(fps);
			this.SpriteBatch.DrawOutlinedText(this.DebugFont, this.frsb, new Vector2(10f, 10f), Color.White, Color.Black, 1);
			this.SpriteBatch.End();
		}

		protected override bool ShowMissingRequirementMessage(Exception exception)
		{
			exception.PreserveStackTrace();
			throw exception;
		}

		public static void Run<T>(string errorUrl, string name) where T : DNAGame, new()
		{
			Version version = new Version(0, 0);
			DateTime startTime = DateTime.UtcNow;
			if (Debugger.IsAttached)
			{
				using (T g = new T())
				{
					version = g.Version;
					g.Run();
					return;
				}
			}
			try
			{
				using (T g2 = new T())
				{
					version = g2.Version;
					g2.Run();
				}
			}
			catch (Exception e)
			{
				BlackScreenIssueReporter reporter = new BlackScreenIssueReporter(errorUrl, name, version, startTime);
				reporter.ReportCrash(e);
			}
		}

		public static void Run<T>(IssueReporter issueReporter, OnlineServices onlineServices) where T : DNAGame, new()
		{
			new Version(0, 0);
			if (Debugger.IsAttached)
			{
				using (T g = new T())
				{
					g.LicenseServices = onlineServices;
					Version version = g.Version;
					g.Run();
					return;
				}
			}
			try
			{
				using (T g2 = new T())
				{
					g2.LicenseServices = onlineServices;
					Version version2 = g2.Version;
					g2.Run();
				}
			}
			catch (Exception e)
			{
				issueReporter.ReportCrash(e);
			}
		}

		protected virtual void AfterLoad()
		{
		}

		public Vector2 ScreenToBuffer(Vector2 screenPoint)
		{
			if (this._offscreenBuffer == null)
			{
				return screenPoint;
			}
			return new Vector2((screenPoint.X - (float)this._bufferDestRect.Left) * (float)this._offscreenBuffer.Width / (float)this._bufferDestRect.Width, (screenPoint.Y - (float)this._bufferDestRect.Top) * (float)this._offscreenBuffer.Height / (float)this._bufferDestRect.Height);
		}

		public Vector2 BufferToScreen(Vector2 bufferPoint)
		{
			if (this._offscreenBuffer == null)
			{
				return bufferPoint;
			}
			return new Vector2(bufferPoint.X * (float)this._bufferDestRect.Width / (float)this._offscreenBuffer.Width + (float)this._bufferDestRect.X, bufferPoint.Y * (float)this._bufferDestRect.Height / (float)this._offscreenBuffer.Height + (float)this._bufferDestRect.Top);
		}

		public Point ScreenToBuffer(Point screenPoint)
		{
			if (this._offscreenBuffer == null)
			{
				return screenPoint;
			}
			if (this._bufferDestRect.Width == 0 || this._bufferDestRect.Height == 0)
			{
				return Point.Zero;
			}
			return new Point((screenPoint.X - this._bufferDestRect.Left) * this._offscreenBuffer.Width / this._bufferDestRect.Width, (screenPoint.Y - this._bufferDestRect.Top) * this._offscreenBuffer.Height / this._bufferDestRect.Height);
		}

		public Point BufferToScreen(Point bufferPoint)
		{
			if (this._offscreenBuffer == null)
			{
				return bufferPoint;
			}
			return new Point(bufferPoint.X * this._bufferDestRect.Width / this._offscreenBuffer.Width + this._bufferDestRect.X, bufferPoint.Y * this._bufferDestRect.Height / this._offscreenBuffer.Height + this._bufferDestRect.Top);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.GraphicsDevice.SetRenderTarget(this._offscreenBuffer);
			if (this.Stop)
			{
				base.GraphicsDevice.Clear(Color.Black);
				return;
			}
			if (this.Loading)
			{
				base.GraphicsDevice.Clear(Color.Black);
			}
			this.ScreenManager.Draw(base.GraphicsDevice, this.SpriteBatch, gameTime);
			this.DrawBrightness();
			if (this.CurrentBroadcastStream != null)
			{
				if (this._offscreenBuffer == null)
				{
					throw new Exception("You must create an offscreen buffer, to use a Video Stream");
				}
				this.CurrentBroadcastStream.SubmitFrame(this._offscreenBuffer);
			}
			base.GraphicsDevice.SetRenderTarget(null);
			if (this._offscreenBuffer != null)
			{
				base.GraphicsDevice.Clear(Color.Black);
				this.SpriteBatch.Begin();
				float screenAspect = base.GraphicsDevice.Viewport.AspectRatio;
				float offscreenAspect = (float)this._offscreenBuffer.Width / (float)this._offscreenBuffer.Height;
				if (screenAspect > offscreenAspect)
				{
					int destHeight = base.GraphicsDevice.Viewport.Height;
					int destWidth = base.GraphicsDevice.Viewport.Height * this._offscreenBuffer.Width / this._offscreenBuffer.Height;
					this._bufferDestRect = new Rectangle((base.GraphicsDevice.Viewport.Width - destWidth) / 2, 0, destWidth, destHeight);
				}
				else
				{
					int destHeight2 = base.GraphicsDevice.Viewport.Width * this._offscreenBuffer.Height / this._offscreenBuffer.Width;
					int destWidth2 = base.GraphicsDevice.Viewport.Width;
					this._bufferDestRect = new Rectangle(0, (base.GraphicsDevice.Viewport.Height - destHeight2) / 2, destWidth2, destHeight2);
				}
				this.SpriteBatch.Draw(this._offscreenBuffer, this._bufferDestRect, Color.White);
				this.SpriteBatch.End();
			}
			if (this.ScreenManager.ShowMouseCursor)
			{
				this.SpriteBatch.Begin();
				this.SpriteBatch.Draw(this.MousePointer, new Vector2((float)this.InputManager.Mouse.CurrentState.X, (float)this.InputManager.Mouse.CurrentState.Y), Color.White);
				this.SpriteBatch.End();
			}
			base.Draw(gameTime);
		}

		public const int WM_SYSCOMMAND = 274;

		public const int SC_MAXIMIZE = 61488;

		private bool processMessages;

		private OnlineServices _licenseServices;

		public static Random Random = new Random();

		private Version _version;

		private static DateTime _gameStartTime = DateTime.UtcNow;

		public BroadcastStream CurrentBroadcastStream;

		public Texture2D MousePointer;

		public bool PauseDuringGuide = true;

		public bool Stop;

		public DialogManager DialogManager;

		protected GraphicsDeviceManager Graphics;

		protected ScreenGroup _screenManager = new ScreenGroup(false);

		protected SpriteBatch SpriteBatch;

		protected InputManager InputManager;

		private bool _firstFrame = true;

		private NetworkSession _networkSession;

		private VoiceChat _voiceChat;

		public Texture2D DummyTexture;

		private GameTime _currentGameTime = new GameTime();

		public bool LimitElapsedGameTime = true;

		private RenderTarget2D _offscreenBuffer;

		public TaskScheduler TaskScheduler = new TaskScheduler();

		private DNAGame.LoadStatus _loadStatus;

		public bool ShowTitleSafeArea = true;

		public SpriteFont DebugFont;

		public bool CheatsEnabled;

		private DNAGame.CodeVal[] konamiCode = new DNAGame.CodeVal[]
		{
			DNAGame.CodeVal.Up,
			DNAGame.CodeVal.Up,
			DNAGame.CodeVal.Down,
			DNAGame.CodeVal.Down,
			DNAGame.CodeVal.Left,
			DNAGame.CodeVal.Right,
			DNAGame.CodeVal.Left,
			DNAGame.CodeVal.Right,
			DNAGame.CodeVal.B,
			DNAGame.CodeVal.A
		};

		private int CodeLimit = 10;

		private Queue<DNAGame.CodeVal> recentCodes = new Queue<DNAGame.CodeVal>();

		protected bool HasGamerServices;

		protected GVerifier _gVerifier = new GVerifier();

		private Thread _startupThread;

		private Dictionary<byte, NetworkGamer> _currentPlayers = new Dictionary<byte, NetworkGamer>();

		private bool _fixedWindow;

		private Queue<char> _inboundKeys = new Queue<char>();

		private bool _doAfterLoad;

		private Exception LastException;

		private bool _doSystemUpdates = true;

		public float Brightness;

		private StringBuilder frsb = new StringBuilder();

		private Rectangle _bufferDestRect;

		private enum LoadStatus
		{
			NotStarted,
			InProcess,
			Complete
		}

		public enum ScreenModes
		{
			Mode1080,
			Mode720
		}

		private enum CodeVal
		{
			None,
			Up,
			Down,
			Left = 4,
			Right = 8,
			A = 16,
			B = 32,
			X = 64,
			Y = 128
		}
	}
}
