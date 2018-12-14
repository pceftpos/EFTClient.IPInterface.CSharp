
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModels
{
	public class ProxyViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler<EFTSendKeyRequest> OnSendKey;
		public char[] trimChars = { (char)0x02, '\0', '2' };

		protected void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}

		private EFTDisplayResponse _displayDetails = new EFTDisplayResponse();
		public EFTDisplayResponse DisplayDetails
		{
			get
			{
				_displayDetails.DisplayText[0] = _displayDetails.DisplayText[0].Trim(trimChars);
				_displayDetails.DisplayText[1] = _displayDetails.DisplayText[1].Trim(trimChars);

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

		EFTSendKeyRequest _keyRequest = new EFTSendKeyRequest();
		public EFTSendKeyRequest KeyRequest { get { return _keyRequest; } set { _keyRequest = value; OnPropertyChanged(nameof(KeyRequest)); } }

		EFTPOSKey _key;
		public EFTPOSKey Key { get { return _key; } set { _key = value; OnPropertyChanged(nameof(Key)); } }

		string _keyData = "";
		public string KeyData
		{
			get { return _keyData; }
			set { _keyData = value; OnPropertyChanged(nameof(KeyData)); }
		}
		RelayCommand _sendKeyCommand;
		public RelayCommand SendKeyCommand => _sendKeyCommand ?? (_sendKeyCommand = new RelayCommand((o) =>
			  {
				  string name = o.ToString();
				  ProxyDialog window = Application.Current.Windows.OfType<ProxyDialog>().FirstOrDefault();
				  EFTPOSKey key = EFTPOSKey.OkCancel;
				  if (EnumContains(name, out key))
				  {
					  KeyRequest.Key = key;
					  KeyRequest.Data = KeyData.ToString() + window.pwordInput.Password;
					  OnSendKey?.Invoke(this, KeyRequest);
					  KeyData = "";
					  window.pwordInput.Password = "";
				  }
			  }));


		public bool ProxyWindowClosing = false;


		public void SendKeyFunc(EFTSendKeyRequest key)
		{
			OnSendKey?.Invoke(this, key);
		}


		public IEFTClientIPAsync EFTClientIPAsync { get; set; }

	}
}
