using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DNA.Security;
using DNA.Security.Cryptography;
using DNA.Text;
using Microsoft.Win32;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Distribution
{
	public class LauncherControl : UserControl
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			ComponentResourceManager resources = new ComponentResourceManager(typeof(LauncherControl));
			this.webBrowser1 = new WebBrowser();
			this.userNameBox = new TextBox();
			this.passworrdBox = new TextBox();
			this.rememberPassBox = new CheckBox();
			this.label1 = new Label();
			this.label2 = new Label();
			this.optionsButton = new Button();
			this.loginButton = new Button();
			this.NeedAccountLabel = new LinkLabel();
			this.facebookLoginButton = new Button();
			base.SuspendLayout();
			this.webBrowser1.AllowWebBrowserDrop = false;
			resources.ApplyResources(this.webBrowser1, "webBrowser1");
			this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
			this.webBrowser1.MinimumSize = new Size(20, 20);
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.Url = new Uri("", UriKind.Relative);
			resources.ApplyResources(this.userNameBox, "userNameBox");
			this.userNameBox.Name = "userNameBox";
			resources.ApplyResources(this.passworrdBox, "passworrdBox");
			this.passworrdBox.Name = "passworrdBox";
			resources.ApplyResources(this.rememberPassBox, "rememberPassBox");
			this.rememberPassBox.Name = "rememberPassBox";
			this.rememberPassBox.UseVisualStyleBackColor = true;
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			resources.ApplyResources(this.optionsButton, "optionsButton");
			this.optionsButton.Name = "optionsButton";
			this.optionsButton.UseVisualStyleBackColor = true;
			this.optionsButton.Click += this.optionsButton_Click;
			resources.ApplyResources(this.loginButton, "loginButton");
			this.loginButton.Name = "loginButton";
			this.loginButton.UseVisualStyleBackColor = true;
			this.loginButton.Click += this.loginButton_Click;
			resources.ApplyResources(this.NeedAccountLabel, "NeedAccountLabel");
			this.NeedAccountLabel.Name = "NeedAccountLabel";
			this.NeedAccountLabel.TabStop = true;
			this.NeedAccountLabel.LinkClicked += this.NeedAccountLabel_LinkClicked;
			resources.ApplyResources(this.facebookLoginButton, "facebookLoginButton");
			this.facebookLoginButton.BackColor = Color.CornflowerBlue;
			this.facebookLoginButton.ForeColor = Color.White;
			this.facebookLoginButton.Image = CommonDialogResources.loginwithfacebook;
			this.facebookLoginButton.Name = "facebookLoginButton";
			this.facebookLoginButton.UseVisualStyleBackColor = false;
			this.facebookLoginButton.Click += this.facebookLoginButton_Click;
			resources.ApplyResources(this, "$this");
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.facebookLoginButton);
			base.Controls.Add(this.NeedAccountLabel);
			base.Controls.Add(this.loginButton);
			base.Controls.Add(this.optionsButton);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.rememberPassBox);
			base.Controls.Add(this.passworrdBox);
			base.Controls.Add(this.userNameBox);
			base.Controls.Add(this.webBrowser1);
			base.Name = "LauncherControl";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		public event EventHandler<EventArgs> GameLaunched;

		public event EventHandler<EventArgs> OptionsClicked;

		public event EventHandler<EventArgs> FacebookLoginClicked;

		public event EventHandler<EventArgs> BeforeGameLaunch;

		public string Username
		{
			get
			{
				return this.userNameBox.Text;
			}
		}

		public string Password
		{
			get
			{
				return this.passworrdBox.Text;
			}
		}

		public LauncherControl()
		{
			this.InitializeComponent();
		}

		public bool TrialMode
		{
			get
			{
				return this._trialMode;
			}
		}

		private bool CheckHardware()
		{
			if (GraphicsAdapter.DefaultAdapter.IsProfileSupported(GraphicsProfile.HiDef) || GraphicsAdapter.DefaultAdapter.IsProfileSupported(GraphicsProfile.Reach))
			{
				return true;
			}
			MessageBox.Show(this, CommonResources.We_re_sorry_but_your_video_hardware_is_not_currently_supported__We_are_currently_working_on_supporting_more_hardware__and_we_will_have_a_solution_for_you_soon_, CommonResources.Unsupported_Hardware, MessageBoxButtons.OK);
			return false;
		}

		private void LoadSettings()
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey(this._keyLoc + this.Services.ProductID.ToString(), false);
			if (key != null)
			{
				bool savePassword = bool.Parse((string)key.GetValue("SavePassword", "false"));
				this.rememberPassBox.Checked = savePassword;
				string username = (string)key.GetValue("Username", "");
				this.userNameBox.Text = username;
				if (savePassword)
				{
					try
					{
						string encodedpassword = (string)key.GetValue("Password", "");
						if (!string.IsNullOrEmpty(encodedpassword))
						{
							MD5HashProvider hasher = new MD5HashProvider();
							byte[] encoderKey = hasher.Compute(Encoding.UTF8.GetBytes(username + "DDNA12345" + this.Services.ProductID.ToString())).Data;
							string password = SecurityTools.DecryptString(encoderKey, TextConverter.FromBase32String(encodedpassword));
							this.passworrdBox.Text = password;
						}
					}
					catch
					{
						this.passworrdBox.Text = "";
					}
				}
			}
		}

		private void SaveSettings()
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey(this._keyLoc + this.Services.ProductID.ToString(), true);
			if (key == null)
			{
				key = Registry.CurrentUser.CreateSubKey(this._keyLoc + this.Services.ProductID.ToString());
			}
			key.SetValue("SavePassword", this.rememberPassBox.Checked);
			key.SetValue("Username", this.userNameBox.Text);
			if (this.rememberPassBox.Checked)
			{
				MD5HashProvider hasher = new MD5HashProvider();
				byte[] encoderKey = hasher.Compute(Encoding.UTF8.GetBytes(this.userNameBox.Text + "DDNA12345" + this.Services.ProductID.ToString())).Data;
				byte[] passData = SecurityTools.EncryptString(encoderKey, this.passworrdBox.Text);
				string encodedpassword = TextConverter.ToBase32String(passData);
				key.SetValue("Password", encodedpassword);
			}
			else
			{
				try
				{
					key.DeleteValue("Password");
				}
				catch
				{
				}
			}
			key.Close();
		}

		protected override void OnLoad(EventArgs e)
		{
			if (this.Services != null)
			{
				this.landerURL = new Uri(this.Services.GetLauncherPage());
				this.webBrowser1.Navigate(this.landerURL);
				this.webBrowser1.DocumentCompleted += this.webBrowser1_DocumentCompleted;
				this.webBrowser1.Navigating += this.webBrowser1_Navigating;
				this.LoadSettings();
			}
			base.OnLoad(e);
		}

		private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			if (this.landerURL == e.Url)
			{
				this.docComplete = true;
			}
		}

		private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			if (this.docComplete)
			{
				e.Cancel = true;
				Process.Start(e.Url.ToString());
			}
		}

		private void optionsButton_Click(object sender, EventArgs e)
		{
			if (this.OptionsClicked != null)
			{
				this.OptionsClicked(this, new EventArgs());
			}
		}

		public void RegisterFacebook(string facebookID, string accessToken, string email)
		{
			string reason;
			this.Services.RegisterFacebook(facebookID, accessToken, email, this.Username, this.Password, out reason);
		}

		public void ValidateLicenseFacebook(string facebookID, string accessToken, string email, string facebookUsername, Uri logoutURL, string facebookName)
		{
			Cursor.Current = Cursors.WaitCursor;
			string username;
			string reason;
			if (this.Services.ValidateLicenseFacebook(facebookID, accessToken, out username, out reason))
			{
				this._trialMode = false;
				if (this.CheckHardware())
				{
					if (this.BeforeGameLaunch != null)
					{
						this.BeforeGameLaunch(this, new EventArgs());
					}
					if (this.GameLaunched != null)
					{
						this.GameLaunched(this, new EventArgs());
					}
				}
				else
				{
					this.HardwareInvalid = true;
					base.ParentForm.Close();
				}
			}
			else if (reason == "terms not accepted")
			{
				AcceptTOSForm tosForm = new AcceptTOSForm();
				DialogResult result = tosForm.ShowDialog();
				if (result == DialogResult.Yes)
				{
					this.Services.AcceptTermsFacebook(facebookID);
					this.ValidateLicenseFacebook(facebookID, accessToken, email, facebookUsername, logoutURL, facebookName);
				}
			}
			else if (reason == "invalid license")
			{
				if (this.CheckHardware())
				{
					this._trialMode = true;
					if (this.BeforeGameLaunch != null)
					{
						this.BeforeGameLaunch(this, new EventArgs());
					}
					if (this.GameLaunched != null)
					{
						this.GameLaunched(this, new EventArgs());
					}
				}
				else
				{
					this.HardwareInvalid = true;
					base.ParentForm.Close();
				}
			}
			else if (reason == "not registered")
			{
				FacebookRegisterForm fbForm = new FacebookRegisterForm(facebookUsername, facebookID, accessToken, email, facebookName, logoutURL, this.Services);
				DialogResult result2 = fbForm.ShowDialog(this);
				if (result2 == DialogResult.OK)
				{
					this.ValidateLicenseFacebook(facebookID, accessToken, email, facebookUsername, logoutURL, facebookName);
				}
			}
			else if (reason == "invalid access token")
			{
				this.passworrdBox.Text = "";
				MessageBox.Show(CommonResources.LauncherControl_ValidateLicenseFacebook_There_was_a_problem_authenticating_your_facebook_account_, CommonResources.Error, MessageBoxButtons.OK);
			}
			else
			{
				this.passworrdBox.Text = "";
				MessageBox.Show(CommonResources.There_was_an_error_, CommonResources.Error, MessageBoxButtons.OK);
			}
			Cursor.Current = Cursors.Default;
		}

		private void loginButton_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			this.userNameBox.Text = this.userNameBox.Text.Trim();
			string reason;
			if (this.Services.ValidateLicense(this.userNameBox.Text, this.passworrdBox.Text, out reason))
			{
				this.SaveSettings();
				if (this.CheckHardware())
				{
					this._trialMode = false;
					if (this.BeforeGameLaunch != null)
					{
						this.BeforeGameLaunch(this, new EventArgs());
					}
					if (this.GameLaunched != null)
					{
						this.GameLaunched(this, new EventArgs());
					}
				}
				else
				{
					this.HardwareInvalid = true;
					base.ParentForm.Close();
				}
			}
			else if (reason == "terms not accepted")
			{
				AcceptTOSForm tosForm = new AcceptTOSForm();
				DialogResult result = tosForm.ShowDialog();
				if (result == DialogResult.Yes)
				{
					this.Services.AcceptTerms(this.userNameBox.Text, this.passworrdBox.Text);
					this.loginButton_Click(sender, e);
				}
			}
			else if (reason == "invalid license")
			{
				this.SaveSettings();
				if (this.CheckHardware())
				{
					this._trialMode = true;
					if (this.BeforeGameLaunch != null)
					{
						this.BeforeGameLaunch(this, new EventArgs());
					}
					if (this.GameLaunched != null)
					{
						this.GameLaunched(this, new EventArgs());
					}
				}
				else
				{
					this.HardwareInvalid = true;
					base.ParentForm.Close();
				}
			}
			else if (reason == "invalid user")
			{
				this.passworrdBox.Text = "";
				InvalidLoginForm form = new InvalidLoginForm();
				DialogResult result2 = form.ShowDialog(this);
				if (result2 == DialogResult.Yes)
				{
					Process.Start("https://www.digitaldnagames.com/Account/Register.aspx");
				}
			}
			else if (reason == "not verified")
			{
				this.passworrdBox.Text = "";
				Process.Start("https://www.digitaldnagames.com/Account/VerifyEmail.aspx");
			}
			else
			{
				this.passworrdBox.Text = "";
				MessageBox.Show(CommonResources.There_was_an_error_, CommonResources.Error, MessageBoxButtons.OK);
			}
			Cursor.Current = Cursors.Default;
		}

		private void NeedAccountLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://www.digitaldnagames.com/Account/Register.aspx");
		}

		private void facebookLoginButton_Click(object sender, EventArgs e)
		{
			if (this.FacebookLoginClicked != null)
			{
				this.FacebookLoginClicked(this, new EventArgs());
			}
		}

		private IContainer components;

		private WebBrowser webBrowser1;

		private TextBox userNameBox;

		private TextBox passworrdBox;

		private CheckBox rememberPassBox;

		private Label label1;

		private Label label2;

		private Button optionsButton;

		private Button loginButton;

		private LinkLabel NeedAccountLabel;

		private Button facebookLoginButton;

		public OnlineServices Services;

		private bool _trialMode = true;

		public bool HardwareInvalid;

		private readonly string _keyLoc = "Software\\DigitalDNAGames\\";

		private Uri landerURL;

		private bool docComplete;
	}
}
