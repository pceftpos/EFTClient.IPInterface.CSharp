using System;

namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>Indicates the requested status type.</summary>
	public enum StatusType
	{
		/// <summary>Request the EFT status from the PIN pad.</summary>
		Standard = '0',
		/// <summary>Not supported by all PIN pads.</summary>
		TerminalAppInfo = '1',
		/// <summary>Not supported by all PIN pads.</summary>
		AppCPAT = '2',
		/// <summary>Not supported by all PIN pads.</summary>
		AppNameTable = '3',
        /// <summary>Undefined</summary>
        Undefined = '4',
        /// <summary>Not supported by all PIN pads.</summary>
        PreSwipe = '5'
	}

	/// <summary>Indicates the EFT terminal hardware type.</summary>
	public enum EFTTerminalType
	{
		/// <summary>Ingenico NPT 710 PIN pad terminal.</summary>
		IngenicoNPT710,
		/// <summary>Ingenico NPT PX328 PIN pad terminal.</summary>
		IngenicoPX328,
		/// <summary>Ingenico NPT i5110 PIN pad terminal.</summary>
		Ingenicoi5110,
		/// <summary>Ingenico NPT i3070 PIN pad terminal.</summary>
		Ingenicoi3070,
		/// <summary>Sagem PIN pad terminal.</summary>
		Sagem,
		/// <summary>Verifone PIN pad terminal.</summary>
		Verifone,
		/// <summary>Keycorp PIN pad terminal.</summary>
		Keycorp,
		/// <summary>Unknown PIN pad terminal.</summary>
		Unknown
	}

	/// <summary>PIN pad terminal supported options.</summary>
	[FlagsAttribute]
	public enum PINPadOptionFlags
	{
		/// <summary>Tipping enabled flag.</summary>
		Tipping = 0x0001,
		/// <summary>Pre-athourization enabled flag.</summary>
		PreAuth = 0x0002,
		/// <summary>Completions enabled flag.</summary>
		Completions = 0x0004,
		/// <summary>Cash-out enabled flag.</summary>
		CashOut = 0x0008,
		/// <summary>Refund enabled flag.</summary>
		Refund = 0x0010,
		/// <summary>Balance enquiry enabled flag.</summary>
		Balance = 0x0020,
		/// <summary>Deposit enabled flag.</summary>
		Deposit = 0x0040,
		/// <summary>Manual voucher enabled flag.</summary>
		Voucher = 0x0080,
		/// <summary>Mail-order/Telephone-order enabled flag.</summary>
		MOTO = 0x0100,
		/// <summary>Auto-completions enabled flag.</summary>
		AutoCompletion = 0x0200,
		/// <summary>Electronic Fallback enabled flag.</summary>
		EFB = 0x0400,
		/// <summary>EMV enabled flag.</summary>
		EMV = 0x0800,
		/// <summary>Training mode enabled flag.</summary>
		Training = 0x1000,
		/// <summary>Withdrawal enabled flag.</summary>
		Withdrawal = 0x2000,
		/// <summary>Funds transfer enabled flag.</summary>
		Transfer = 0x4000,
		/// <summary>Start cash enabled flag.</summary>
		StartCash = 0x8000
	}

	/// <summary>PIN pad terminal key handling scheme.</summary>
	public enum KeyHandlingType
	{
		/// <summary>Single-DES encryption standard.</summary>
		SingleDES = '0',
		/// <summary>Triple-DES encryption standard.</summary>
		TripleDES = '1',
		/// <summary>Unknown encryption standard.</summary>
		Unknown
	}

	/// <summary>PIN pad terminal network option.</summary>
	public enum NetworkType
	{
		/// <summary>Leased line bank connection.</summary>
		Leased = '1',
		/// <summary>Dial-up bank connection.</summary>
		Dialup = '2',
		/// <summary>Unknown bank connection.</summary>
		Unknown
	}

	/// <summary>PIN pad terminal communication option.</summary>
	public enum TerminalCommsType
	{
		/// <summary>Cable link communications.</summary>
		Cable = '0',
		/// <summary>Intrared link communications.</summary>
		Infrared = '1',
		/// <summary>Unknown link communications.</summary>
		Unknown
	}

	/// <summary>A PC-EFTPOS terminal status request object.</summary>
	public class EFTStatusRequest : EFTRequest
	{
		/// <summary>Constructs a default terminal status request object.</summary>
		public EFTStatusRequest() : base()
		{
		}

        /// <summary>Type of status to perform.</summary>
        /// <value>Type: <see cref="StatusType"/><para>The default is <see cref="StatusType.Standard" />.</para></value>
        public StatusType StatusType { get; set; } = StatusType.Standard;

        /// <summary>Indicates where the request is to be sent to. Should normally be EFTPOS.</summary>
        /// <value>Type: <see cref="TerminalApplication"/><para>The default is <see cref="TerminalApplication.EFTPOS"/>.</para></value>
        public TerminalApplication Application { get; set; } = TerminalApplication.EFTPOS;
    }

	/// <summary>A PC-EFTPOS terminal status response object.</summary>
	public class EFTStatusResponse : EFTResponse
	{
		string aiic;
		int nii;
		string terminalID;
		string merchantID;
		int timeout;
		bool loggedOn;
		string pinPadSerialNumber;
		string pinPadVersion;
		char bankCode;
		string bankDescription;
		string kvc;
		int safCount;
		NetworkType networkType;
		string hardwareSerial;
		string retailerName;
		PINPadOptionFlags optionsFlags;
		decimal safCreditLimit;
		decimal safDebitLimit;
		int maxSaf;
		KeyHandlingType keyScheme;
		decimal cashoutLimit;
		decimal refundLimit;
		string cpatVersion;
		string nameTableVersion;
		TerminalCommsType commsType;
		int cardMisreadCount;
		int totalMemory;
		int freeMemory;
		EFTTerminalType eftTerminalType;
		int numApps;
		int numLines;
		DateTime hardwareInceptDate;

		/// <summary>Constructs a default terminal status response object.</summary>
		public EFTStatusResponse()
			: base()
		{
			aiic = "";
			nii = 0;
			terminalID = "";
			merchantID = "";
			timeout = 0;
			loggedOn = false;
			pinPadSerialNumber = "";
			pinPadVersion = "";
			bankCode = ' ';
			bankDescription = "";
			kvc = "";
			safCount = 0;
			networkType = NetworkType.Unknown;
			hardwareSerial = "";
			retailerName = "";
			optionsFlags = 0;
			safCreditLimit = 0;
			safDebitLimit = 0;
			maxSaf = 0;
			keyScheme = KeyHandlingType.Unknown;
			cashoutLimit = 0;
			refundLimit = 0;
			cpatVersion = "";
			nameTableVersion = "";
			commsType = TerminalCommsType.Unknown;
			cardMisreadCount = 0;
			totalMemory = 0;
			freeMemory = 0;
			eftTerminalType = EFTTerminalType.Unknown;
			numApps = 0;
			numLines = 0;
			hardwareInceptDate = DateTime.MinValue;
		}

		/// <summary>The AIIC that is configured in the terminal.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		public string AIIC
		{
			get { return aiic; }
			set { aiic = value; }
		}

		/// <summary>The NII that is configured in the terminal.</summary>
		/// <value>Type: <see cref="System.Int32" /></value>
		public int NII
		{
			get { return nii; }
			set { nii = value; }
		}

		/// <summary>The terminal ID (CatID) that is configured in the terminal.</summary>
		/// <value>Type: <see cref="System.String" /><para>A string of length 8 characters.</para></value>
		public string TerminalID
		{
			get { return terminalID; }
			set { terminalID = value; }
		}

		/// <summary>The merchant ID (CaID) that is configured in the terminal.</summary>
		/// <value>Type: <see cref="System.String" /><para>A string of length 15 characters.</para></value>
		public string MerchantID
		{
			get { return merchantID; }
			set { merchantID = value; }
		}

		/// <summary>The bank response timeout that is configured in the terminal.</summary>
		/// <value>Type: <see cref="System.Int32" /></value>
		public int Timeout
		{
			get { return timeout; }
			set { timeout = value; }
		}

		/// <summary>Indicates if the PIN pad is currently logged on.</summary>
		/// <value>Type: <see cref="System.Boolean" /></value>
		public bool LoggedOn
		{
			get { return loggedOn; }
			set { loggedOn = value; }
		}

		/// <summary>The serial number of the terminal.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		public string PINPadSerialNumber
		{
			get { return pinPadSerialNumber; }
			set { pinPadSerialNumber = value; }
		}

		/// <summary>The software version currently loaded into the terminal.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		public string PINPadVersion
		{
			get { return pinPadVersion; }
			set { pinPadVersion = value; }
		}

		/// <summary>The bank acquirer code.</summary>
		/// <value>Type: <see cref="System.Char" /></value>
		public char BankCode
		{
			get { return bankCode; }
			set { bankCode = value; }
		}

		/// <summary>The bank description.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		public string BankDescription
		{
			get { return bankDescription; }
			set { bankDescription = value; }
		}

		/// <summary>Key verification code.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		public string KVC
		{
			get { return kvc; }
			set { kvc = value; }
		}

		/// <summary>Current number of stored transactions.</summary>
		/// <value>Type: <see cref="System.Int32" /></value>
		public int SAFCount
		{
			get { return safCount; }
			set { safCount = value; }
		}

		/// <summary>The acquirer communications type.</summary>
		/// <value>Type: <see cref="NetworkType" /></value>
		public NetworkType NetworkType
		{
			get { return networkType; }
			set { networkType = value; }
		}

		/// <summary>The hardware serial number.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		public string HardwareSerial
		{
			get { return hardwareSerial; }
			set { hardwareSerial = value; }
		}

		/// <summary>The merchant retailer name.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		public string RetailerName
		{
			get { return retailerName; }
			set { retailerName = value; }
		}

		/// <summary>PIN pad terminal supported options flags.</summary>
		/// <value>Type: <see cref="PINPadOptionFlags" /></value>
		public PINPadOptionFlags OptionsFlags
		{
			get { return optionsFlags; }
			set { optionsFlags = value; }
		}

		/// <summary>Store-and forward credit limit.</summary>
		/// <value>Type: <see cref="System.Decimal" /></value>
		public decimal SAFCreditLimit
		{
			get { return safCreditLimit; }
			set { safCreditLimit = value; }
		}

		/// <summary>Store-and-forward debit limit.</summary>
		/// <value>Type: <see cref="System.Decimal" /></value>
		public decimal SAFDebitLimit
		{
			get { return safDebitLimit; }
			set { safDebitLimit = value; }
		}

		/// <summary>The maximum number of store transactions.</summary>
		/// <value>Type: <see cref="System.Int32" /></value>
		public int MaxSAF
		{
			get { return maxSaf; }
			set { maxSaf = value; }
		}

		/// <summary>The terminal key handling scheme.</summary>
		/// <value>Type: <see cref="KeyHandlingType" /></value>
		public KeyHandlingType KeyHandlingScheme
		{
			get { return keyScheme; }
			set { keyScheme = value; }
		}

		/// <summary>The maximum cash out limit.</summary>
		/// <value>Type: <see cref="System.Decimal" /></value>
		public decimal CashoutLimit
		{
			get { return cashoutLimit; }
			set { cashoutLimit = value; }
		}

		/// <summary>The maximum refund limit.</summary>
		/// <value>Type: <see cref="System.Decimal" /></value>
		public decimal RefundLimit
		{
			get { return refundLimit; }
			set { refundLimit = value; }
		}

		/// <summary>Card prefix table version.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		public string CPATVersion
		{
			get { return cpatVersion; }
			set { cpatVersion = value; }
		}

		/// <summary>Card name table version.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		public string NameTableVersion
		{
			get { return nameTableVersion; }
			set { nameTableVersion = value; }
		}

		/// <summary>The terminal to PC communication type.</summary>
		/// <value>Type: <see cref="TerminalCommsType" /></value>
		public TerminalCommsType TerminalCommsType
		{
			get { return commsType; }
			set { commsType = value; }
		}

		/// <summary>Number of card mis-reads.</summary>
		/// <value>Type: <see cref="System.Int32" /></value>
		public int CardMisreadCount
		{
			get { return cardMisreadCount; }
			set { cardMisreadCount = value; }
		}

		/// <summary>Number of memory pages in the PIN pad terminal.</summary>
		/// <value>Type: <see cref="System.Int32" /></value>
		public int TotalMemoryInTerminal
		{
			get { return totalMemory; }
			set { totalMemory = value; }
		}

		/// <summary>Number of free memory pages in the PIN pad terminal.</summary>
		/// <value>Type: <see cref="System.Int32" /></value>
		public int FreeMemoryInTerminal
		{
			get { return freeMemory; }
			set { freeMemory = value; }
		}

		/// <summary>The type of PIN pad terminal.</summary>
		/// <value>Type: <see cref="EFTTerminalType" /></value>
		public EFTTerminalType EFTTerminalType
		{
			get { return eftTerminalType; }
			set { eftTerminalType = value; }
		}

		/// <summary>Number of applications in the terminal.</summary>
		/// <value>Type: <see cref="System.Int32" /></value>
		public int NumAppsInTerminal
		{
			get { return numApps; }
			set { numApps = value; }
		}

		/// <summary>Number of available display line on the terminal.</summary>
		/// <value>Type: <see cref="System.Int32" /></value>
		public int NumLinesOnDisplay
		{
			get { return numLines; }
			set { numLines = value; }
		}

		/// <summary>The date the hardware was incepted.</summary>
		/// <value>Type: <see cref="System.DateTime" /></value>
		public DateTime HardwareInceptionDate
		{
			get { return hardwareInceptDate; }
			set { hardwareInceptDate = value; }
		}

        /// <summary>Indicates if the request was successful.</summary>
        /// <value>Type: <see cref="System.Boolean"/></value>
        public bool Success { get; set; } = false;

        /// <summary>The response code of the request.</summary>
        /// <value>Type: <see cref="System.String"/><para>A 2 character response code. "00" indicates a successful response.</para></value>
        public string ResponseCode { get; set; } = "";

        /// <summary>The response text for the response code.</summary>
        /// <value>Type: <see cref="System.String"/></value>
        public string ResponseText { get; set; } = "";
    }
}