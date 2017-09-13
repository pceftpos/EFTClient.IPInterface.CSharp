namespace PCEFTPOS.EFTClient.IPInterface.SimpleDemo
{
    public interface ISettings
    {
        string EFTClientAddress { get; set; }
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