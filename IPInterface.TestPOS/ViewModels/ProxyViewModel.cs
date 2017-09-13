using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModels
{
    public class ProxyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EFTPOSKey> OnSendKey;

        protected void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private EFTDisplayResponse _displayDetails = new EFTDisplayResponse();
        public EFTDisplayResponse DisplayDetails
        {
            get
            {
                _displayDetails.DisplayText[0] = _displayDetails.DisplayText[0].Trim();
                _displayDetails.DisplayText[1] = _displayDetails.DisplayText[1].Trim();

                return _displayDetails;
            }
            set
            {
                _displayDetails = value;
                OnPropertyChanged(nameof(DisplayDetails));
            }
        }

        private bool EnumContains<T>(string val, out T item)
        {
            bool result = false;
            item = default(T);

            foreach (var i in Enum.GetValues(typeof(T)))
            {
                string s = i.ToString();
                if (s.ToUpper().Contains(val.ToUpper()))
                {
                    T x = (T)Enum.Parse(typeof(T), s);
                    item = x;
                    result = true;
                    break;
                }
            }

            return result;
        }

        EFTPOSKey _key;
        public EFTPOSKey Key { get { return _key; } set { _key = value; OnPropertyChanged(nameof(Key)); } }

        RelayCommand _sendKeyCommand;
        public RelayCommand SendKeyCommand => _sendKeyCommand ?? (_sendKeyCommand = new RelayCommand((o) =>
              {
                  string name = o.ToString();
                  EFTPOSKey key = EFTPOSKey.OkCancel;

                  if (EnumContains(name, out key))
                  {
                      OnSendKey?.Invoke(this, key);
                  }
              }));

        public bool ProxyWindowClosing = false;


        public void SendKeyFunc(EFTPOSKey key)
        {
            OnSendKey?.Invoke(this, key);
        }
    }
}
