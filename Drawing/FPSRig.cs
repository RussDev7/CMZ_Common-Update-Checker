using System;
using DNA.Input;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public class FPSRig : Entity
	{
		public BasicPhysics PlayerPhysics
		{
			get
			{
				return (BasicPhysics)base.Physics;
			}
		}

		public Quaternion RecoilRotation
		{
			get
			{
				return this.recoilPiviot.LocalRotation;
			}
			set
			{
				this.recoilPiviot.LocalRotation = value;
			}
		}

		protected virtual bool CanJump
		{
			get
			{
				return this.InContact || this.m_jumpCount < this.JumpCountLimit;
			}
		}

		protected virtual void SetInContact(bool contact)
		{
			this.InContact = contact;
			if (this.InContact)
			{
				this.m_jumpCount = 0;
			}
		}

		public FPSRig()
		{
			this.FPSCamera.FieldOfView = Angle.FromDegrees(73f);
			base.Physics = new BasicPhysics(this);
			new GameTime(TimeSpan.FromSeconds(0.01), TimeSpan.FromSeconds(0.01));
			base.Children.Add(this.pitchPiviot);
			this.pitchPiviot.LocalPosition = new Vector3(0f, 1.4f, 0f);
			this.pitchPiviot.Children.Add(this.recoilPiviot);
			this.recoilPiviot.Children.Add(this.FPSCamera);
		}

		public virtual void Jump()
		{
			if (this.CanJump)
			{
				float num = Vector3.Dot(this.GroundNormal, Vector3.Up);
				Vector3 worldVelocity = this.PlayerPhysics.WorldVelocity;
				worldVelocity.Y += this.JumpImpulse * num;
				this.PlayerPhysics.WorldVelocity = worldVelocity;
				this.m_jumpCount++;
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			this.pitchPiviot.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, this.TorsoPitch.Radians);
			base.OnUpdate(gameTime);
		}

		protected virtual void UpdateRotation(FPSControllerMapping input, GameTime gameTime)
		{
			this.TorsoPitch += Angle.FromRadians(3.1415927f * input.Aiming.Y * this.ControlSensitivity);
			if (this.TorsoPitch > Angle.FromDegrees(89f))
			{
				this.TorsoPitch = Angle.FromDegrees(89f);
			}
			if (this.TorsoPitch < Angle.FromDegrees(-89f))
			{
				this.TorsoPitch = Angle.FromDegrees(-89f);
			}
			base.LocalRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, -3.1415927f * input.Aiming.X * this.ControlSensitivity);
		}

		protected virtual void UpdateVelocity(FPSControllerMapping input, GameTime gameTime)
		{
			double totalSeconds = gameTime.ElapsedGameTime.TotalSeconds;
			this.PlayerPhysics.LocalVelocity = new Vector3(input.Movement.X * this.Speed, this.PlayerPhysics.LocalVelocity.Y, -input.Movement.Y * this.Speed);
		}

		public virtual void ProcessInput(FPSControllerMapping input, GameTime gameTime)
		{
			this.UpdateRotation(input, gameTime);
			this.UpdateVelocity(input, gameTime);
			if (input.Jump.Pressed)
			{
				this.Jump();
			}
		}

		public const float EyePointHeight = 1.4f;

		private Entity pitchPiviot = new Entity();

		private Entity recoilPiviot = new Entity();

		public PerspectiveCamera FPSCamera = new PerspectiveCamera();

		public Angle TorsoPitch = Angle.Zero;

		public float Speed = 5f;

		public bool InContact = true;

		public Vector3 GroundNormal = Vector3.Up;

		public float JumpImpulse = 10f;

		public float ControlSensitivity = 1f;

		public int JumpCountLimit = 1;

		protected int m_jumpCount;
	}
}
