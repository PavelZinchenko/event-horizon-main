namespace Services.Settings
{
    public interface IGameSettings
    {
        public float MusicVolume { get; set; }
        public float SoundVolume { get; set; }
        public float CameraZoom { get; set; }
        public string Language { get; set; }
        public bool RateButtonClicked { get; set; }
        public bool SignedIn { get; set; }
        public string ControlsLayout { get; set; }
        public bool SlideToMove { get; set; }
        public bool ThrustWidthJoystick { get; set; }
        public bool StopWhenWeaponActive { get; set; }
        public int QualityMode { get; set; }
        public bool RunInBackground { get; set; }
        public int AppStartCounter { get; set; }
        public bool CenterOnPlayer { get; set; }
		public bool ShowDamage { get; set; }
		public bool ShowEnemyMessages { get; set; }
		public bool AutoSave { get; set; }
        public string EditorText { get; set; }
        public string ActiveMod { get; set; }
        public int LastFacebookPostDate { get; set; }
        public int LastDailyRewardDate { get; set; }
        public int DontAskAgainId { get; set; }
        public string KeyBindings { get; set; }
        public bool UseMouse { get; set; }
    }
}
