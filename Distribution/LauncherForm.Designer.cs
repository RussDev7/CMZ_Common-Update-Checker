namespace DNA.Distribution
{
	public partial class LauncherForm : global::System.Windows.Forms.Form
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
			global::System.ComponentModel.ComponentResourceManager resources = new global::System.ComponentModel.ComponentResourceManager(typeof(global::DNA.Distribution.LauncherForm));
			this.launcherControl1 = new global::DNA.Distribution.LauncherControl();
			base.SuspendLayout();
			resources.ApplyResources(this.launcherControl1, "launcherControl1");
			this.launcherControl1.Name = "launcherControl1";
			this.launcherControl1.GameLaunched += new global::System.EventHandler<global::System.EventArgs>(this.launcherControl1_GameLaunched);
			this.launcherControl1.OptionsClicked += new global::System.EventHandler<global::System.EventArgs>(this.launcherControl1_OptionsClicked);
			this.launcherControl1.FacebookLoginClicked += new global::System.EventHandler<global::System.EventArgs>(this.launcherControl1_FacebookLoginClicked);
			this.launcherControl1.BeforeGameLaunch += new global::System.EventHandler<global::System.EventArgs>(this.launcherControl1_BeforeGameLaunch);
			resources.ApplyResources(this, "$this");
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this.launcherControl1);
			base.Name = "LauncherForm";
			base.ResumeLayout(false);
		}

		private global::System.ComponentModel.IContainer components;

		private global::DNA.Distribution.LauncherControl launcherControl1;
	}
}
