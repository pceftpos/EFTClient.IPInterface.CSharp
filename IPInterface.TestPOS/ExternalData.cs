
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    public class ExternalData : IDataErrorInfo, INotifyPropertyChanged
    {
        public ExternalData()
        {
        }

        public ExternalData(string name, string value)
        {
            _name = name;
            _value = value;
        }

        string _name = string.Empty;
        public string Name
        {
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
            get
            {
                return _name;
            }
        }

        string _value = string.Empty;
        public string Value
        {
            set
            {
                _value = value;
                NotifyPropertyChanged("Value");
            }
            get
            {
                return _value;
            }
        }

        public PadFields Fields { get; set; } = new PadFields();

        public string Error
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return "error";
                else
                    return string.Empty;
            }
        }

        public string this[string columnName]
        {
            get
            {
                string msg = string.Empty;
                switch (columnName)
                {
                    case "Name":
                        {
                            if (string.IsNullOrEmpty(Name))
                            {
                                msg = "Name must not be empty.";
                            }
                            break;
                        }
                    case "Value":
                        {
                            if (string.IsNullOrEmpty(Value))
                            {
                                msg = "PAD content must not be empty.";
                            }
                            break;
                        }
                }

                return msg;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public override string ToString()
        {
            return $"{Name} | {Value}";
        }

    }

    public class FieldTag : PadTag
    {
        public FieldTag()
        {
        }

        public FieldTag(string name, string data) :
            base(name, data)
        {
        }

        public override string ToString()
        {
            return $"{Name} | {Data}"; 
        }
    }

    public class PadFields : List<PadTag>
    {
        public override string ToString()
        {
            var s = string.Empty;
            ForEach(x => s += x.Name + x.Data);
            return s;
        }
    }

    public class ExternalDataList : List<ExternalData>
    {
    }

    public class CloudData
    {
        public bool IsAutoLogin { get; set; } = false;
        
        public string ClientId { get; set; } = "";
        public string PairingCode { get; set; } = "";
        [JsonIgnore]
        public string Password = string.Empty;

        [JsonProperty]
        ICredentialLocker _locker = new CredentialLocker();

        public void ClearLoginDetails()
        {
            ClientId = string.Empty;
            PairingCode = string.Empty;
            IsAutoLogin = false;
        }

        public void LoadCredentials()
        {
            _locker.LoadCredentials();
            Password = _locker.Password;
        }

        public void SaveCredentials()
        {
            _locker.SaveCredentials(Password);
        }
    }

    public class UserSettings
    {
        public CloudData CloudInfo { get; set; } = new CloudData();
        public bool IsSettingsShown { get; set; } = true;
        public bool IsUtilitiesShown { get; set; } = true;
        public bool IsLogShown { get; set; } = true;
        public DemoDialogMode DemoDialogOption { get; set; } = DemoDialogMode.Hide;

        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 2011;
        public bool UseSSL { get; set; } = false;
        public string Notes { get; set; } = string.Empty;
    }

    public enum DemoDialogMode
    {
        AlwaysShow,
        ShowOnEvents,
        Hide
    }
}
