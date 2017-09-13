namespace PCEFTPOS.EFTClient.IPInterface.SimpleDemoAsync
{

    public interface ISettings
    {
        string EFTClientAddress { get; set; }
        bool EnableCloud { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string PairingCode { get; set; }
        void Save();
        void Load();
    }

    class Settings : ISettings
    {
        public string EFTClientAddress
        {
            get
            {
                return Properties.Settings.Default.EFTClientAddress;
            }
            set
            {
                Properties.Settings.Default.EFTClientAddress = value;
            }
        }

        public bool EnableCloud
        {
            get
            {
                return Properties.Settings.Default.EnableCloud;
            }
            set
            {
                Properties.Settings.Default.EnableCloud = value;
            }
        }

        public string Username
        {
            get
            {
                return Properties.Settings.Default.Username;
            }
            set
            {
                Properties.Settings.Default.Username = value;
            }
        }

        public string Password
        {
            get
            {
                return Properties.Settings.Default.Password;
            }
            set
            {
                Properties.Settings.Default.Password = value;
            }
        }

        public string PairingCode
        {
            get
            {
                return Properties.Settings.Default.PairingCode;
            }
            set
            {
                Properties.Settings.Default.PairingCode = value;
            }
        }

        public void Load()
        {
            Properties.Settings.Default.Reload();
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}