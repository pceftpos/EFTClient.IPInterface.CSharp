using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
	public enum LogType { Info, Error, Warning };

	public enum ConnectedStatus { Connected, Disconnected };

	public delegate void LogEvent(string message);
	public delegate void DisplayEvent(bool show);

	public class ClientData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public event LogEvent OnLog;
		public event DisplayEvent OnDisplay;
		public event EventHandler OnDisplayChanged;

		public ClientData()
		{
			_dctCommands.Add("Enter Slave Mode", "*@101S1004300 ");
			_dctCommands.Add("Display 'Swipe Card'", "*@103Z 0060220  D 0240000   SWIPE CARD       D 0240100                    ");
			_dctCommands.Add("Swipe Card", "*@102J1000K100810000010");
			_dctCommands.Add("Exit Slave Mode", "*@101S0000");
			_dctCommands.Add("Complete Read Card Command", "*@107S1004300 Z 0060216  D 0240000   SWIPE CARD       D 0240100                    J1000K100810000010S0000");
		}

		#region Connect
		private ConnectedStatus _connectedState = ConnectedStatus.Disconnected;
		public ConnectedStatus ConnectedState
		{
			get
			{
				return _connectedState;
			}
			set
			{
				_connectedState = value;
				NotifyPropertyChanged("ConnectedState");
			}
		}

		#endregion

		#region Logon
		public LogonType SelectedLogon { get; set; } = LogonType.Standard;
		public ObservableCollection<string> LogonList { get { return GetEnum<LogonType>(); } }

		private bool _logonTestEnabled = false;
		public bool LogonTestEnabled
		{
			set
			{
				_logonTestEnabled = value;
				NotifyPropertyChanged("LogonTestEnabled");
			}
			get
			{
				return _logonTestEnabled;
			}
		}

		#endregion

		#region Receipt Options

		public bool CutReceipt { set; get; }
		public ReceiptCutModeType CutReceiptMode
		{
			get
			{
				return (CutReceipt ? ReceiptCutModeType.Cut : ReceiptCutModeType.DontCut);
			}
		}

		public ReceiptPrintModeType PrintMode { get; set; } = ReceiptPrintModeType.PinpadPrinter;
		public ObservableCollection<string> PrintModeList { get { return GetEnum<ReceiptPrintModeType>(); } }
		#endregion

		#region Transaction
		public bool IsETS { get; set; } = false;
		public bool IsPrintTimeOut { get; set; } = false;
		public bool IsPrePrintTimeOut { get; set; } = false;
		public ObservableCollection<string> TerminalList { get { return GetEnum<TerminalApplication>(); } }

		private int _transactionCount = 0;
		public string TransactionReference
		{
			get
			{
				return string.Format("{0:ddMMyyyyHHmm}{1:D3}", DateTime.Now, _transactionCount);
			}
			set
			{
				_transactionCount++;
				NotifyPropertyChanged("TransactionReference");
			}
		}

		private string _originalTransactionReference = "";
		public string OriginalTransactionReference
		{
			get
			{
				return _originalTransactionReference;
			}
			set
			{
				_originalTransactionReference = value;
				NotifyPropertyChanged("OriginalTransactionReference");
			}
		}

		private string _lastReceiptMerchantNumber = "00";

		public string LastReceiptMerchantNumber
		{
			get
			{
				return _lastReceiptMerchantNumber;
			}
			set
			{
				_lastReceiptMerchantNumber = value;
				NotifyPropertyChanged("LastReceiptMerchantNumber");
			}
		}

		public EFTTransactionRequest TransactionRequest { get; set; } = new EFTTransactionRequest();

		public ObservableCollection<string> TransactionList { get { return GetFilteredEnum<TransactionType>(); } }
		string _selectedApplication = string.Empty;
		public string SelectedApplication
		{
			get { return _selectedApplication; }
			set { _selectedApplication = value; NotifyPropertyChanged("Application"); }
		}

		public ObservableCollection<string> CardSourceList { get { return GetEnum<PanSource>(); } }
		string _selectedCardSource = string.Empty;
		public string SelectedCardSource
		{
			get { return _selectedCardSource; }
			set { _selectedCardSource = value; NotifyPropertyChanged("SelectedCardSource"); }
		}

		public ObservableCollection<string> AccountList { get { return GetEnum<AccountType>(); } }

		ExternalDataList _track2Items = new ExternalDataList();
		public ExternalDataList Track2Items
		{
			get
			{
				return _track2Items;
			}
			set
			{
				_track2Items = value;
				NotifyPropertyChanged("Track2List");
			}
		}

		string _selectedTrack2 = string.Empty;
		public string SelectedTrack2
		{
			get { return _selectedTrack2; }
			set { _selectedTrack2 = value; NotifyPropertyChanged("SelectedTrack2"); }
		}

		public ObservableCollection<string> Track2List
		{
			get
			{
				var list = new ObservableCollection<string>();
				_track2Items.ForEach(x => list.Add(x.ToString()));
				return list;
			}
		}

		ExternalDataList _padItems = new ExternalDataList();
		public ExternalDataList PadItems
		{
			get { return _padItems; }
			set { _padItems = value; NotifyPropertyChanged("PadList"); }
		}


		public string SelectedPad { get; set; } = string.Empty;
		public ObservableCollection<string> PadList
		{
			get
			{
				var list = new ObservableCollection<string>();
				PadItems.ForEach(x => list.Add(x.ToString()));
				return list;
			}
		}
		#endregion

		#region ETS Transaction
		public ObservableCollection<string> ETSTransactionList { get { return GetFilteredEnum<TransactionType>("ETS"); } }
		#endregion

		#region Status
		public StatusType SelectedStatus { get; set; }
		public ObservableCollection<string> StatusList { get { return GetEnum<StatusType>(); } }
		#endregion

		#region ClientList
		public EFTClientListRequest ClientListRequest { get; set; } = new EFTClientListRequest();
		#endregion

		#region Configure Merchant
		public EFTConfigureMerchantRequest MerchantDetails { get; set; } = new EFTConfigureMerchantRequest();
		#endregion

		#region Settlement
		public bool ResetTotals { get; set; } = false;
		public SettlementType SelectedSettlement { get; set; }
		public ObservableCollection<string> SettlementList { get { return GetEnum<SettlementType>(); } }
		#endregion

		#region ControlPanel
		public ControlPanelType SelectedDisplay { get; set; }
		public ObservableCollection<string> ControlPanelList { get { return GetEnum<ControlPanelType>(); } }

		#endregion

		#region QueryCard
		public QueryCardType SelectedQuery { get; set; }
		public ObservableCollection<string> QueryCardList { get { return GetFilteredEnum<QueryCardType>(); } }
		#endregion

		#region Cheque Auth
		public EFTChequeAuthRequest ChequeRequest { get; set; } = new EFTChequeAuthRequest();
		public ObservableCollection<string> ChequeList { get { return GetEnum<ChequeType>(); } }
		#endregion

		#region Dialog
		public ObservableCollection<string> DialogTypeList { get { return GetEnum<DialogType>(); } }
		public ObservableCollection<string> DialogPositionList { get { return GetEnum<DialogPosition>(); } }

		public SetDialogRequest DialogRequest { get; set; } = new SetDialogRequest();
		#endregion

		#region Slave Mode
		private Dictionary<string, string> _dctCommands = new Dictionary<string, string>();

		string _commandRequest = string.Empty;
		public string CommandRequest
		{
			set
			{
				_commandRequest = value;
				NotifyPropertyChanged("CommandRequest");
			}
			get
			{
				return _commandRequest;
			}
		}

		string _command = string.Empty;
		public string SelectedCommand
		{
			set
			{
				_command = value;
				string cmdValue = string.Empty;
				_dctCommands.TryGetValue(_command, out cmdValue);
				CommandRequest = cmdValue;
			}
			get
			{
				return _command;
			}
		}
		public ObservableCollection<string> CommandsList
		{
			get
			{
				var coll = new ObservableCollection<string>();
				foreach (var item in _dctCommands.Keys)
				{
					coll.Add(item);
				}

				return coll;
			}
		}
		#endregion

		#region SendKey
		public EFTPOSKey SelectedPosKey { get; set; } = EFTPOSKey.OkCancel;
		public ObservableCollection<string> PosKeyList { get { return GetEnum<EFTPOSKey>(); } }

		public string PosData { get; set; } = string.Empty;
		bool _sendKeyEnabled = false;
		public bool SendKeyEnabled
		{
			set
			{
				_sendKeyEnabled = value;
				NotifyPropertyChanged("SendKeyEnabled");
			}
			get
			{
				return _sendKeyEnabled;
			}
		}
		#endregion

		#region Common

		public bool HasResult
		{
			set
			{
			}
			get
			{
				return (_lastTxnResult.Count > 0);
			}
		}

		private Dictionary<string, string> _lastTxnResult = new Dictionary<string, string>();
		public Dictionary<string, string> LastTxnResult
		{
			get
			{
				return _lastTxnResult;
			}
			set
			{
				_lastTxnResult = value;
				NotifyPropertyChanged("LastTxnResult");
				NotifyPropertyChanged("HasResult");
			}
		}

		private string _messages = string.Empty;
		public string LogMessages
		{
			get
			{
				return _messages;
			}
			set
			{
				_messages = value;
				NotifyPropertyChanged("LogMessages");
			}
		}

		public void Log(string message, LogType logType = LogType.Info)
		{
			try
			{
				if (Settings.IsLogShown)
				{
					//_messages += $"{DateTime.Now} [{logType}] : {message}" + Environment.NewLine;
					//OnLog(_messages);
					OnLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")} [{logType}] : {message}{Environment.NewLine}");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.WriteLine(ex.Message);
			}
		}

		public void DisplayDialog(bool show)
		{
			OnDisplay(show);
		}

		public IProgress<string> Progress { set; get; }

		protected void NotifyPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}

		private ObservableCollection<string> GetEnum<T>()
		{
			return new ObservableCollection<string>(Enum.GetNames(typeof(T)));
		}

		private ObservableCollection<string> GetFilteredEnum<T>()
		{
			var list = typeof(T).GetFields()
				.Where(x => x.IsLiteral && ((FilterAttribute[])x.GetCustomAttributes(typeof(FilterAttribute), false)).Length == 0)
				.Select(x => x.Name);
			return new ObservableCollection<string>(list);
		}

		private ObservableCollection<string> GetFilteredEnum<T>(string filter)
		{
			var list = typeof(T).GetFields()
				.Where(x => x.IsLiteral && ((FilterAttribute[])x.GetCustomAttributes(typeof(FilterAttribute), false)).Length > 0
						&& ((FilterAttribute[])x.GetCustomAttributes(typeof(FilterAttribute), false))[0].CustomString.Equals(filter))
				.Select(x => x.Name);
			return new ObservableCollection<string>(list);
		}

		#endregion

		#region Proxy Dialog
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
				NotifyPropertyChanged("DisplayDetails");
				OnDisplayChanged?.Invoke(this, EventArgs.Empty);
			}
		}
		#endregion

		#region User Settings
		public UserSettings Settings { get; set; } = new UserSettings();
		#endregion

		#region Receipts
		private string _receiptInfo = string.Empty;
		public string Receipt
		{
			get
			{
				return _receiptInfo;
			}
			set
			{
				_receiptInfo = value;
				NotifyPropertyChanged("Receipt");
			}
		}
		#endregion
	}
}
