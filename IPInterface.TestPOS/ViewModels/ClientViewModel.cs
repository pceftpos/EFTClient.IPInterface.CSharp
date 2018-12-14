
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{

	public class ClientViewModel : INotifyPropertyChanged
	{
		const string QPAD_FILENAME = "QueryCard_List.json";
		const string TRACK2_FILENAME = "Track2_List.json";
		const string SETTINGS_FILENAME = "settings.json";

		ClientData _data = new ClientData();
		EftWrapper _eftw = null;
		ProxyDialog _proxy = new ProxyDialog();

		public event LogEvent OnLog;

		public ClientViewModel()
		{
			// initialize external data
			PadEditor editor = new PadEditor(TRACK2_FILENAME);
			_data.Track2Items = editor.ViewModel.UpdatedExternalData;
			editor = new PadEditor(QPAD_FILENAME);
			_data.PadItems = editor.ViewModel.UpdatedExternalData;

			// setup data
			_data.PropertyChanged += _data_PropertyChanged;
			_eftw = new EftWrapper(_data);

			_data.OnLog += _data_OnLog;
			_data.OnDisplay += _data_OnDisplay;
			_data.OnDisplayChanged += _data_OnDisplayChanged;

			_proxyVM.OnSendKey += _proxyVM_OnSendKey;
		}

		private void _data_OnDisplayChanged(object sender, EventArgs e)
		{
			ProxyVM.DisplayDetails = _data.DisplayDetails;
		}

		private async void _proxyVM_OnSendKey(object sender, EFTSendKeyRequest e)
		{
			await _eftw.SendKey(e.Key, e.Data);
		}

		private void _data_OnDisplay(bool show)
		{
			ShowProxyDialog(show);
		}

		public async void Initialize()
		{
			// auto-login to cloud if user chose it
			LoadSettings();
			if (_data.Settings != null && _data.Settings.CloudInfo != null && _data.Settings.CloudInfo.IsAutoLogin && _data.Settings.UseSSL)
			{
				if (!string.IsNullOrEmpty(_data.Settings.CloudInfo.ClientId)
					&& !string.IsNullOrEmpty(_data.Settings.CloudInfo.Password)
					&& !string.IsNullOrEmpty(_data.Settings.CloudInfo.PairingCode))
				{
					await DoCloudLogon(_data.Settings.CloudInfo.Password, true);
				}
				else
				{
					_data_OnLog("Cloud logon details are empty.");
				}
			}
		}

		private void _data_OnLog(string message)
		{
			OnLog?.Invoke(message);
		}

		#region Common
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}

		private void _data_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(e.PropertyName);
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

		#endregion

		#region Properties
		public ClientData Data
		{
			get { return _data; }
		}

		#endregion

		#region Commmands

		#region Common
		public RelayCommand ClearResult
		{
			get
			{
				return new RelayCommand((o) =>
				{
					Data.LastTxnResult = new Dictionary<string, string>();
				});
			}
		}

		private void LoadSettings()
		{
			try
			{
				UserSettings data = new UserSettings();

				JsonWriter settings = new JsonWriter();
				settings.Load(SETTINGS_FILENAME, out data);

				if (data != default(UserSettings))
				{
					data.CloudInfo.LoadCredentials();
					Data.Settings = data;
				}
			}
			catch
			{
			}
		}

		public void SaveSettings()
		{
			try
			{
				Data.Settings.CloudInfo.SaveCredentials();

				JsonWriter settings = new JsonWriter();
				settings.Save(Data.Settings, SETTINGS_FILENAME);
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message);
			}
		}

		#endregion

		#region Connection
		public async Task ConnectAsync()
		{
			await _eftw.Connect(Data.Settings.Host, Data.Settings.Port, Data.Settings.UseSSL);
		}

		public void Disconnect()
		{
			_eftw.Disconnect();
		}

		public RelayCommand Connect
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					if (Data.ConnectedState == ConnectedStatus.Disconnected)
					{
						if (Data.Settings.UseSSL)
						{
							string p = (string)o;
							await DoCloudLogon(p);
						}
						else
						{
							await _eftw.Connect(Data.Settings.Host, Data.Settings.Port, Data.Settings.UseSSL);
						}
					}
					else
					{
						_eftw.Disconnect();
					}
				});
			}
		}
		#endregion

		#region Logon
		public RelayCommand Logon
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.Logon(Data.SelectedLogon, Data.CutReceiptMode, Data.PrintMode, false);
				});
			}
		}

		public RelayCommand CloudLogon
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					string p = (string)o;
					await DoCloudLogon(p);
				});
			}
		}

		public async Task DoCloudLogon(string password, bool autoLogin = false)
		{
			try
			{
				if (_data.ConnectedState == ConnectedStatus.Disconnected)
				{
					var r = await _eftw.Connect(Data.Settings.Host, Data.Settings.Port, Data.Settings.UseSSL);
					if (r == false)
						return;
				}

				Data.Settings.CloudInfo.Password = password;
				await _eftw.CloudLogon(Data.Settings.CloudInfo.ClientId, Data.Settings.CloudInfo.Password, Data.Settings.CloudInfo.PairingCode);

				if (Data.Settings.CloudInfo.IsAutoLogin && !autoLogin)
				{
					SaveSettings();
				}
			}
			catch (Exception ex)
			{
				_data_OnLog(ex.Message);
			}
		}

		public RelayCommand ToggleLogon
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					if (!Data.LogonTestEnabled)
					{
						Data.LogonTestEnabled = true;
						await _eftw.StartLogonTest(Data.SelectedLogon, Data.CutReceiptMode, Data.PrintMode);
					}
					else
					{
						Data.LogonTestEnabled = false;
						_eftw.StopLogonTest();
					}
				});
			}
		}

		#endregion

		#region Transaction
		public RelayCommand Transaction
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					try
					{
						if (!string.IsNullOrEmpty(Data.SelectedPad))
						{
							string pad = Data.PadItems.Find(x => x.ToString().Equals(Data.SelectedPad)).Value;
							Data.TransactionRequest.PurchaseAnalysisData = new PadField(pad);
						}

						if (!string.IsNullOrEmpty(Data.SelectedTrack2) && Data.TransactionRequest.PanSource == PanSource.POSSwiped)
						{
							var selectedTrack = Data.Track2Items.Find(x => x.ToString().Equals(Data.SelectedTrack2));
							Data.TransactionRequest.Track2 = (selectedTrack != null) ? selectedTrack.Value : Data.SelectedTrack2;
						}
						else
						{
							Data.SelectedTrack2 = string.Empty;
						}

						if (!Data.IsETS)
						{
							Data.TransactionRequest.Application = TerminalApplication.EFTPOS;
						}

						Data.TransactionRequest.TxnRef = Data.TransactionReference;
						Data.TransactionRequest.ReceiptAutoPrint = Data.PrintMode;
						Data.TransactionRequest.CutReceipt = Data.CutReceiptMode;

						await _eftw.DoTransaction(Data.TransactionRequest);
					}
					catch (Exception ex)
					{
						Data.Log(ex.Message);
					}
				});
			}
		}

		public RelayCommand QueryTransaction
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					try
					{
						PadField p = new PadField();
						if (!string.IsNullOrEmpty(Data.SelectedPad))
						{
							string pad = Data.PadItems.Find(x => x.ToString().Equals(Data.SelectedPad)).Value;
							p = new PadField(pad);
						}

						if (!Data.IsETS)
						{
							Data.TransactionRequest.Application = TerminalApplication.EFTPOS;
						}

						Data.TransactionRequest.TxnRef = Data.TransactionReference;

						await _eftw.QueryCard(p, QueryCardType.ReadCard);

						string output = string.Empty;
						Data.LastTxnResult.TryGetValue("Success", out output);
						if (output != null && output.Equals("True"))
						{
							if (!string.IsNullOrEmpty(Data.SelectedTrack2))
							{
								Data.SelectedCardSource = PanSource.POSSwiped.ToString();
								Data.TransactionRequest.PanSource = PanSource.POSSwiped;
								Data.TransactionRequest.Track2 = Data.SelectedTrack2;
								await _eftw.DoTransaction(Data.TransactionRequest);
							}
						}
					}
					catch (Exception ex)
					{
						Data.Log(ex.Message);
					}
				});
			}
		}

		public RelayCommand LaunchTrack2
		{
			get
			{
				return new RelayCommand((o) =>
				{
					PadEditor editor = new PadEditor(TRACK2_FILENAME, "Track");
					if (editor.ShowDialog() == true)
					{
						Data.Track2Items = editor.ViewModel.UpdatedExternalData;
					}
				});
			}
		}

		public RelayCommand LaunchPad
		{
			get
			{
				return new RelayCommand((o) =>
				{
					PadEditor editor = new PadEditor(QPAD_FILENAME);
					if (editor.ShowDialog() == true)
					{
						Data.PadItems = editor.ViewModel.UpdatedExternalData;
					}
				});
			}
		}

		#endregion

		#region Status
		public RelayCommand Status
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.GetStatus(Data.SelectedStatus);
				});
			}
		}

		#endregion

		#region ClientList

		public RelayCommand ClientList
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.DoClientList(new EFTClientListRequest());
				});
			}
		}

		#endregion

		#region Configure Merchant
		public RelayCommand ConfigureMerchant
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.ConfigureMerchant(Data.MerchantDetails);
				});
			}
		}
		#endregion

		#region Settlement
		public RelayCommand Settlement
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					var pad = "";
					if (!string.IsNullOrEmpty(Data.SelectedPad))
					{
						pad = Data.PadItems.Find(x => x.ToString().Equals(Data.SelectedPad)).Value;
					}

					await _eftw.DoSettlement(Data.SelectedSettlement, Data.CutReceiptMode, new PadField(pad), Data.PrintMode, Data.ResetTotals);
				});
			}
		}
		#endregion

		#region Control Panel

		public RelayCommand DisplayControlPanel
		{
			get { return new RelayCommand(async (o) => await _eftw.DisplayControlPanel(Data.SelectedDisplay, Data.CutReceiptMode, Data.PrintMode)); }
		}

		#endregion

		#region Query Card

		public RelayCommand QueryCard
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					string pad = string.Empty;
					if (!string.IsNullOrEmpty(Data.SelectedPad))
					{
						pad = Data.PadItems.Find(x => x.ToString().Equals(Data.SelectedPad)).Value;
					}

					await _eftw.QueryCard(new PadField(pad), Data.SelectedQuery);
				});
			}
		}


		#endregion

		#region Last Transaction

		public RelayCommand LastTransaction
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.GetLastTransaction();
				});
			}
		}


		#endregion

		#region Re-print Receipt

		public RelayCommand ReprintReceipt
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.LastReceipt(Data.CutReceiptMode, Data.PrintMode, ReprintType.Reprint);
				});
			}
		}

		public RelayCommand GetLastReceipt
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.LastReceipt(Data.CutReceiptMode, Data.PrintMode, ReprintType.GetLast);
				});
			}
		}

		#endregion

		#region Verify Cheque

		public RelayCommand VerifyCheque
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.DoVerifyCheque(Data.ChequeRequest);
				});
			}
		}


		#endregion

		#region Set Dialog
		public RelayCommand SetDialog
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.SetDialog(Data.DialogRequest);
				});
			}
		}

		public RelayCommand HideDialog
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					DialogType t = ((bool)o ? DialogType.Hidden : DialogType.Standard);
					SetDialogRequest r = new SetDialogRequest
					{
						DialogX = Data.DialogRequest.DialogX,
						DialogY = Data.DialogRequest.DialogY,
						DialogPosition = Data.DialogRequest.DialogPosition,
						DialogTitle = Data.DialogRequest.DialogTitle,
						EnableTopmost = Data.DialogRequest.EnableTopmost,
						DialogType = t
					};

					await _eftw.SetDialog(r);
				});
			}
		}


		#endregion

		#region Slave Mode
		public RelayCommand SlaveMode
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.DoSlaveMode(Data.CommandRequest);
				});
			}
		}
		#endregion

		#region Send Key
		public RelayCommand SendKey
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.SendKey(Data.SelectedPosKey, Data.PosData);
				});
			}
		}

		public RelayCommand ToggleSendKey
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					if (!Data.SendKeyEnabled)
					{
						Data.SendKeyEnabled = true;
						await _eftw.StartSendKeysTest(Data.SelectedPosKey);
					}
					else
					{
						Data.SendKeyEnabled = false;
						_eftw.StopSendKeysTest();

					}
				});
			}
		}

		public async void ToggleStop()
		{
			if (!Data.SendKeyEnabled)
			{
				Data.SendKeyEnabled = true;
				await _eftw.StartSendKeysTest(Data.SelectedPosKey);
			}
			else
			{
				Data.SendKeyEnabled = false;
				_eftw.StopSendKeysTest();

			}
		}

		//RelayCommand _proxySendKey;
		//public RelayCommand ProxySendKey => _proxySendKey ?? (_proxySendKey = new RelayCommand(async (o) =>
		//{
		//    string name = o.ToString();
		//    EFTPOSKey key = EFTPOSKey.OkCancel;

		//    if (EnumContains(name, out key))
		//    {
		//        await _eftw.SendKey(key, (key == EFTPOSKey.Authorise) ? _data.PosData : string.Empty);
		//        ShowProxyDialog(false);
		//    }
		//}));

		//public RelayCommand ProxySendKey
		//{
		//    get
		//    {
		//        return new RelayCommand(async (o) =>
		//        {
		//            string name = o.ToString();
		//            EFTPOSKey key = EFTPOSKey.OkCancel;

		//            if (EnumContains(name, out key))
		//            {
		//                await _eftw.SendKey(key, (key == EFTPOSKey.Authorise) ? _data.PosData : string.Empty);
		//                ShowProxyDialog(false);
		//            }
		//        });
		//    }
		//}

		//public async Task ProxySendKeyFunc(EFTPOSKey key)
		//{
		//    await _eftw.SendKey(key);
		//}

		#endregion

		#region PIN
		public RelayCommand AuthPin
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.AuthPin();
				});
			}
		}

		public RelayCommand ChangePin
		{
			get
			{
				return new RelayCommand(async (o) =>
				{
					await _eftw.ChangePin();
				});
			}
		}
		#endregion

		#region Proxy Dialog

		//public bool ProxyWindowClosing = false;
		public RelayCommand UseProxyDialog
		{
			get
			{
				return new RelayCommand((o) =>
				{
					ShowProxyDialog(true);
				});
			}
		}

		ViewModels.ProxyViewModel _proxyVM = new ViewModels.ProxyViewModel();
		public ViewModels.ProxyViewModel ProxyVM
		{
			get { return _proxyVM; }
			set
			{
				_proxyVM = value; NotifyPropertyChanged(nameof(ProxyVM));
			}
		}

		public void ShowProxyDialog(bool show)
		{
			if (show)
			{
				if (!_proxy.IsVisible)
				{
					ProxyVM.ProxyWindowClosing = true;
					_proxy.Close();
					_proxy = new ProxyDialog
					{
						DataContext = ProxyVM // this;
					};
					_proxy.Show();
					ProxyVM.ProxyWindowClosing = false;
				}
			}
			else
			{
				if (_data.Settings.DemoDialogOption != DemoDialogMode.AlwaysShow)
				{
					_proxy.Hide();
				}
			}
		}

		#endregion

		#endregion
	}

}
