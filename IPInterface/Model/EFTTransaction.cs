using PCEFTPOS.Messaging;
using System;

namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>PC-EFTPOS terminal applications.</summary>
	public enum TerminalApplication
	{
		/// <summary>The request is for the EFTPOS application.</summary>
		EFTPOS,
		/// <summary>The request is for the Agency application.</summary>
		Agency,
		/// <summary>The request is for the GiftCard application.</summary>
		GiftCard,
		/// <summary>The request is for the Fuel application.</summary>
		Fuel,
		/// <summary>The request is for the Medicare application.</summary>
		Medicare,
		/// <summary>The request is for the Amex application.</summary>
		Amex,
		/// <summary>The request is for the ChequeAuth application.</summary>
		ChequeAuth,
		/// <summary>The request is for the Loyalty application.</summary>
		Loyalty,
		/// <summary>The request is for the PrePaidCard application.</summary>
		PrePaidCard
	}

    /// <summary>EFTPOS transaction types.</summary>
    public enum TransactionType
    {
        /// <summary>Transaction type was not set by the PIN pad (' ').</summary>
        NotSet = ' ',
        /// <summary>A purchase with optional cash-out EFT transaction type ('P').</summary>
        PurchaseCash = 'P',
        /// <summary>A cash-out only EFT transaction type ('C').</summary>
        CashOut = 'C',
        /// <summary>A refund EFT transaction type ('R').</summary>
        Refund = 'R',
        /// <summary>A pre-authorization EFT transaction type ('A').</summary>
        PreAuth = 'A',
        /// <summary>A pre-authorization / completion EFT transaction type ('L').</summary>
        PreAuthCompletion = 'L',
        /// <summary>A pre-authorization / enquiry EFT transaction type ('N').</summary>
        PreAuthEnquiry = 'N',
        /// <summary>A pre-authorization / cancel EFT transaction type ('Q').</summary>
        PreAuthCancel = 'Q',
        /// <summary>A completion EFT transaction type ('M').</summary>
        Completion = 'M',
        /// <summary>A tip adjustment EFT transaction type ('T').</summary>
        TipAdjust = 'T',
        /// <summary>A deposit EFT transaction type ('D').</summary>
        Deposit = 'D',
        /// <summary>A witdrawal EFT transaction type ('W').</summary>
        Withdrawal = 'W',
        /// <summary>A balance EFT transaction type ('B').</summary>
        Balance = 'B',
        /// <summary>A voucher EFT transaction type ('V').</summary>
        Voucher = 'V',
        /// <summary>A funds transfer EFT transaction type ('F').</summary>
        FundsTransfer = 'F',
        /// <summary>A order request EFT transaction type ('O').</summary>
        OrderRequest = 'O',
        /// <summary>A mini transaction history EFT transaction type ('H').</summary>
        MiniTransactionHistory = 'H',
        /// <summary>A auth pin EFT transaction type ('X').</summary>
        AuthPIN = 'X',
        /// <summary>A enhanced pin EFT transaction type ('K').</summary>
        EnhancedPIN = 'K',

        /// <summary>A Redemption allows the POS to use the card as a payment type. This will take the amount from the Card balance ('P').</summary>
        [Filter("ETS")]
        Redemption = 'P',
        /// <summary>A Refund to Card allows the POS to return the value of a previous sale to a  Card ('R').</summary>
        [Filter("ETS")]
        RefundToCard = 'R',
        /// <summary></summary>
        [Filter("ETS")]
        CardSaleTopUp = 'T',
        /// <summary></summary>
        [Filter("ETS")]
        CardSale = 'D',
        /// <summary>A Refund from card  allows the POS to instruct the host to take an amount from a Card ('W').</summary>
        [Filter("ETS")]
        RefundFromCard = 'W',
        /// <summary>A Balance returns the current balance of funds on the card ('B').</summary>
        [Filter("ETS")]
        CardBalance = 'B',
        /// <summary>Activates the card ('A').</summary>
        [Filter("ETS")]
        CardActivate = 'A',
        /// <summary>A de-activate returns a cards to state where the card requires activation before it can be used ('F'). </summary>
        [Filter("ETS")]
        CardDeactivate = 'F',
        /// <summary>This command will add a number of points (or dollars) to a card ('N').</summary>
        [Filter("ETS")]
        AddPointsToCard = 'N',
        /// <summary>This command will subtract a number of points (or dollars) from a card ('K').</summary>
        [Filter("ETS")]
        DecrementPointsFromCard = 'K',
        /// <summary>This command allows a POS to transfer points from a card to another source ('M').</summary>
        [Filter("ETS")]
        TransferPoints = 'M',
        /// <summary>This command will return the amount of cash that is currently on the card and decrement the entire amount from the card ('X').</summary>
        [Filter("ETS")]
        CashBackFromCard = 'X',
        /// <summary>This command will cancel or void a previous sale ('I').</summary>
        [Filter("ETS")]
        CancelVoid = 'I',
        /// <summary>This command adds a card to the card list on the Host ('L').</summary>
        [Filter("ETS")]
        AddCard = 'L',

        None = 0
    }

    public class FilterAttribute : Attribute
    {
        public FilterAttribute(string customString)
        {
            this.customString = customString;
        }
        private string customString;
        public string CustomString
        {
            get { return customString; }
            set { customString = value; }
        }
    }

    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string description)
        {
            this.description = description;
        }
        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }

    /// <summary>Supported EFTPOS account types.</summary>
    public enum AccountType
	{
		/// <summary>The default account type for a card.</summary>
		Default = ' ',
		/// <summary>The savings account type.</summary>
		Savings = '1',
		/// <summary>The cheque account type.</summary>
		Cheque = '2',
		/// <summary>The credit account type.</summary>
		Credit = '3'
	}

	/// <summary>The source of the card number.</summary>
	public enum PANSource
	{
		/// <summary>Indicates the customer will be prompted to swipe,insert or present their card.</summary>
		Default = ' ',
		/// <summary>Indicates the POS has captured the Track2 from the customer card and it is stored in the PAN property.</summary>
		POSSwiped = 'S',
		/// <summary>Indicates the POS operator has keyed in the card number and it is stored in the PAN property.</summary>
		POSKeyed = 'K',
		/// <summary>Indicates the card number was captured from the Internet and is stored in the PAN property.</summary>
		Internet = '0',
		/// <summary>Indicates the card number was captured from a telephone order and it is stored in the PAN property.</summary>
		TeleOrder = '1',
		/// <summary>Indicates the card number was captured from a mail order and it is stored in the PAN property.</summary>
		MailOrder = '2',
		/// <summary>Indicates the POS operator has keyed in the card number and it is stored in the PAN property.</summary>
		CustomerPresent = '3',
        /// <summary>Indicates the card number was captured for a recurring transaction and it is stored in the PAN property.</summary>
        RecurringTransaction = '4',
        /// <summary>Indicates the card number was captured for an installment payment and it is stored in the PAN property.</summary>
        Installment = '5'
    }

	/// <summary>The card entry type of the transaction.</summary>
	public enum CardEntryType
	{
		/// <summary>Manual entry type was not set by the PIN pad.</summary>
		NotSet = ' ',
		/// <summary>Unknown manual entry type. PIN pad may not support this flag.</summary>
		Unknown = '0',
		/// <summary>Card was swiped.</summary>
		Swiped = 'S',
		/// <summary>Card number was keyed.</summary>
		Keyed = 'K',
		/// <summary>Card number was read by a bar code scanner.</summary>
		BarCode = 'B',
		/// <summary>Card number was read from a chip card.</summary>
		ChipCard = 'E',
		/// <summary>Card number was read from a contactless reader.</summary>
		Contactless = 'C',
	}

	/// <summary>The communications method used to process the transaction.</summary>
	public enum CommsMethodType
	{
		/// <summary>Comms method type was not set by the PIN pad.</summary>
		NotSet = ' ',
		/// <summary>Transaction was sent to the bank using an unknown method.</summary>
		Unknown = '0',
		/// <summary>Transaction was sent to the bank using a P66 modem.</summary>
		P66 = '1',
		/// <summary>Transaction was sent to the bank using an Argent.</summary>
		Argent = '2',
		/// <summary>Transaction was sent to the bank using an X25.</summary>
		X25 = '3'
	}

	/// <summary>The currency conversion status for the transaction.</summary>
	public enum CurrencyStatus
	{
		/// <summary>Currency conversion status was not set by the PIN pad.</summary>
		NotSet = ' ',
		/// <summary>Transaction amount was processed in Australian Dollars.</summary>
		AUD = '0',
		/// <summary>Transaction amount was currency converted.</summary>
		Converted = '1'
	}

	/// <summary>The Pay Pass status of the transcation.</summary>
	public enum PayPassStatus
	{
		/// <summary>Pay Pass conversion status was not set by the PIN pad.</summary>
		NotSet = ' ',
		/// <summary>Pay Pass was used in the transaction.</summary>
		PayPassUsed = '1',
		/// <summary>Pay Pass was not used in the transaction.</summary>
		PayPassNotUsed = '0'
	}

    /// <summary>Flags that indicate how the transaction was processed.</summary>
    public class TxnFlags
    {
        char[] flags;

        /// <summary>Constructs a TxnFlags object with default values.</summary>
        public TxnFlags()
        {
        }

        /// <summary>Constructs a TxnFlags object.</summary>
        /// <param name="flags">A <see cref="System.Char">Char array</see> representing the flags.</param>
        public TxnFlags(char[] flags)
        {
            this.flags = new char[8] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
            System.Array.Copy(flags, 0, this.flags, 0, (flags.Length > 8) ? 8 : flags.Length);


            Offline = this.flags[0] == '1';
            ReceiptPrinted = this.flags[1] == '1';
            CardEntry = (CardEntryType)this.flags[2];
            CommsMethod = (CommsMethodType)this.flags[3];
            Currency = (CurrencyStatus)this.flags[4];
            PayPass = (PayPassStatus)this.flags[5];
            UndefinedFlag6 = this.flags[6];
            UndefinedFlag7 = this.flags[7];
        }

        /// <summary>Indicates if a receipt was printed for this transaction.</summary>
        /// <value>Type: <see cref="System.Boolean"/><para>Set to TRUE if a receipt was printed.</para></value>
        public bool ReceiptPrinted { get; set; } = false;

        /// <summary>Indicates if the transaction was approved offline.</summary>
        /// <value>Type: <see cref="System.Boolean"/><para>Set to TRUE if the transaction was approved offline.</para></value>
        public bool Offline { get; set; } = false;

        /// <summary>Indicates the card entry type.</summary>
        /// <value>Type: <see cref="CardEntryType"/></value>
        public CardEntryType CardEntry { get; set; } = CardEntryType.NotSet;

        /// <summary>Indicates the communications method used for this transaction.</summary>
        /// <value>Type: <see cref="CommsMethodType"/></value>
        public CommsMethodType CommsMethod { get; set; } = CommsMethodType.NotSet;

        /// <summary>Indicates the currency conversion status for this transaction.</summary>
        /// <value>Type: <see cref="CurrencyStatus"/></value>
        public CurrencyStatus Currency { get; set; } = CurrencyStatus.NotSet;

        /// <summary>Indicates the Pay Pass status for this transaction.</summary>
        /// <value>Type: <see cref="PayPassStatus"/></value>
        public PayPassStatus PayPass { get; set; } = PayPassStatus.NotSet;

        /// <summary>Undefined flag 6</summary>
        public char UndefinedFlag6 { get; set; } = ' ';

        /// <summary>Undefined flag 7</summary>
        public char UndefinedFlag7 { get; set; } = ' ';
    }

    /// <summary>A PC-EFTPOS transaction request object.</summary>
    public class EFTTransactionRequest : EFTRequest
    {
        /// <summary>Constructs a default EFTTransactionRequest object.</summary>
        public EFTTransactionRequest() : base()
        {
        }

        /// <summary>The type of transaction to perform.</summary>
        /// <value>Type: <see cref="TransactionType"/><para>The default is <see cref="TransactionType.PurchaseCash"></see></para></value>
        public TransactionType TxnType { get; set; } = TransactionType.PurchaseCash;

        /// <summary>The type of transaction to perform.</summary>
        /// <value>Type: <see cref="TransactionType"/><para>The default is <see cref="TransactionType.PurchaseCash"></see></para></value>
        [System.Obsolete("Please use TxnType instead")]
        public TransactionType Type { get { return TxnType; } set { TxnType = value; } }

        /// <summary>The currency code for this transaction.</summary>
        /// <value>Type: <see cref="System.String"/><para>A 3 digit ISO currency code. The default is "   ".</para></value>
        public string CurrencyCode { get; set; } = "  ";

        /// <summary>The original type of transaction for voucher entry.</summary>
        /// <value>Type: <see cref="TransactionType"/><para>The default is <see cref="TransactionType.PurchaseCash"></see></para></value>
        public TransactionType OriginalTxnType { get; set; } = TransactionType.PurchaseCash;

        /// <summary>Date. Used for voucher or completion only</summary>
        /// <value>Type: <see cref="DateTime"/><para>The default is null</para></value>
        public DateTime? Date { get; set; } = null;

        /// <summary>Time. Used for voucher or completion only</summary>
        /// <value>Type: <see cref="DateTime"/><para>The default is null</para></value>
        public DateTime? Time { get; set; } = null;

        /// <summary>Determines if the transaction is a training mode transaction.</summary>
        /// <value>Type: <see cref="System.Boolean"/><para>Set to TRUE if the transaction is to be performed in training mode. The default is FALSE.</para></value>
        public bool TrainingMode { get; set; } = false;

        /// <summary>Indicates if the transaction should be tipable.</summary>
        /// <value>Type: <see cref="System.Boolean"/><para>Set to TRUE if tipping is to be enabled for this transaction. The default is FALSE.</para></value>
        public bool EnableTipping { get; set; } = false;

        /// <summary>The cash amount for the transaction.</summary>
        /// <value>Type: <see cref="System.Decimal"/><para>The default is 0.</para></value>
        /// <remarks>This property is mandatory for a <see cref="TransactionType.CashOut"></see> transaction type.</remarks>
        public decimal AmountCash { get; set; } = 0;

        /// <summary>The purchase amount for the transaction.</summary>
        /// <value>Type: <see cref="System.Decimal"/><para>The default is 0.</para></value>
        /// <remarks>This property is mandatory for all but <see cref="TransactionType.CashOut"></see> transaction types.</remarks>
        public decimal AmountPurchase { get; set; } = 0;

        /// <summary>The authorisation number for the transaction.</summary>
        /// <value>Type: <see cref="System.Int32"/></value>
        /// <remarks>This property is required for a <see cref="TransactionType.Completion"></see> transaction type.</remarks>
        public int AuthNumber { get; set; } = 0;

        /// <summary>The reference number to attach to the transaction. This will appear on the receipt.</summary>
        /// <value>Type: <see cref="System.String"/></value>
        /// <remarks>This property is optional but it usually populated by a unique transaction identifier that can be used for retrieval.</remarks>
        public string ReferenceNumber { get; set; } = "";

        /// <summary>Indicates the source of the card number.</summary>
        /// <value>Type: <see cref="PANSource"/><para>The default is <see cref="PANSource.Default"></see>.</para></value>
        /// <remarks>Use this property for card not present transactions.</remarks>
        public PANSource CardPANSource { get; set; } = PANSource.Default;

        /// <summary>The card number to use when pan source of POS keyed is used.</summary>
        /// <value>Type: <see cref="System.String"/></value>
        /// <remarks>Use this property in conjunction with <see cref="PANSource"></see>.</remarks>
        public string CardPAN { get; set; } = "";

        /// <summary>The expiry date of the card when of POS keyed is used.</summary>
        /// <value>Type: <see cref="System.String"/><para>In MMYY format.</para></value>
        /// <remarks>Use this property in conjunction with <see cref="PANSource"></see> when passing the card expiry date to PC-EFTPOS.</remarks>
        public string ExpiryDate { get; set; } = "";

        /// <summary>The track 2 to use when of POS swiped is used.</summary>
        /// <value>Type: <see cref="System.String"/></value>
        /// <remarks>Use this property when <see cref="PANSource"></see> is set to <see cref="PANSource.POSSwiped"></see> and passing the full Track2 from the card magnetic stripe to PC-EFTPOS.</remarks>
        public string Track2 { get; set; } = "";

        /// <summary>The account to use for this transaction.</summary>
        /// <value>Type: <see cref="AccountType"/><para>Default is <see cref="AccountType.Default"></see>. Use default to prompt user to enter the account type.</para></value>
        /// <remarks>Use this property in conjunction with <see cref="PANSource"></see> when passing the account type to PC-EFTPOS.</remarks>
        public AccountType CardAccountType { get; set; } = AccountType.Default;

        /// <summary>The retrieval reference number for the transaction.</summary>
        /// <value>Type: <see cref="System.String"/></value>
        /// <remarks>This property is required for a <see cref="TransactionType.TipAdjust"></see> transaction type.</remarks>
        public string RRN { get; set; } = "";

        /// <summary>Additional information sent with the request.</summary>
        /// <value>Type: <see cref="PadField"/></value>
        public PadField PurchaseAnalysisData { get; set; } = new PadField();

        /// <summary>Indicates where the request is to be sent to. Should normally be EFTPOS.</summary>
        /// <value>Type: <see cref="TerminalApplication"/><para>The default is <see cref="TerminalApplication.EFTPOS"/>.</para></value>
        public TerminalApplication Application { get; set; } = TerminalApplication.EFTPOS;

        /// <summary>Indicates whether to trigger receipt events.</summary>
        /// <value>Type: <see cref="ReceiptPrintModeType"/><para>The default is POSPrinter.</para></value>
        public ReceiptPrintModeType ReceiptPrintMode { get; set; } = ReceiptPrintModeType.POSPrinter;

        /// <summary>Indicates whether PC-EFTPOS should cut receipts.</summary>
        /// <value>Type: <see cref="ReceiptCutModeType"/><para>The default is DontCut. This property only applies when <see cref="EFTRequest.ReceiptPrintMode"/> is set to EFTClientPrinter.</para></value>
        public ReceiptCutModeType ReceiptCutMode { get; set; } = ReceiptCutModeType.DontCut;

        /// <summary>
        /// 
        /// </summary>
        public int CVV { get; set; } = 0;
    }

	/// <summary>A PC-EFTPOS terminal transaction response object.</summary>
	public class EFTTransactionResponse : EFTResponse
	{
		/// <summary>Constructs a default terminal transaction response object.</summary>
		public EFTTransactionResponse() : base()
		{
		}

        /// <summary>The type of transaction to perform.</summary>
        /// <value>Type: <see cref="TransactionType"/><para>The default is <see cref="TransactionType.PurchaseCash"></see></para></value>
        public TransactionType TxnType { get; set; } = TransactionType.PurchaseCash;

        /// <summary>The type of transaction to perform.</summary>
        /// <value>Type: <see cref="TransactionType"/><para>The default is <see cref="TransactionType.PurchaseCash"></see></para></value>
        [System.Obsolete("Please use TxnType instead")]
        public TransactionType Type { get { return TxnType; } set { TxnType = value; } }

        /// <summary>Indicates the card type that was used in the transaction.</summary>
        /// <value>Type: <see cref="System.String" /></value>
        /// <remarks><seealso cref="EFTTransactionResponse.CardBIN"/></remarks>
        public string CardType { get; set; } = "";

        /// <summary>Indicates the card type that was used in the transaction.</summary>
        /// <value>Type: <see cref="System.Int32" /></value>
        /// <remarks><list type="table">
        /// <listheader><term>Card BIN</term><description>Card Type</description></listheader>
        ///	<item><term>0</term><description>Unknown</description></item>
        ///	<item><term>1</term><description>Debit</description></item>
        ///	<item><term>2</term><description>Bankcard</description></item>
        ///	<item><term>3</term><description>Mastercard</description></item>
        ///	<item><term>4</term><description>Visa</description></item>
        ///	<item><term>5</term><description>American Express</description></item>
        ///	<item><term>6</term><description>Diner Club</description></item>
        ///	<item><term>7</term><description>JCB</description></item>
        ///	<item><term>8</term><description>Label Card</description></item>
        ///	<item><term>9</term><description>JCB</description></item>
        ///	<item><term>11</term><description>JCB</description></item>
        ///	<item><term>12</term><description>Other</description></item></list>
        ///	</remarks>
        public int CardBIN { get; set; } = 0;

		/// <summary>Used to retrieve the transaction from the batch.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		/// <remarks>The retrieval reference number is used when performing a tip adjustment transaction.</remarks>
		public string RRN { get; set; } = "";

        /// <summary>Indicates which settlement batch this transaction will be included in.</summary>
        /// <value>Type: <see cref="System.DateTime" /><para>Settlement date is returned from the bank.</para></value>
        /// <remarks>Use this property to balance POS EFT totals with settlement EFT totals.</remarks>
        public DateTime SettlementDate { get; set; } = DateTime.MinValue;

        /// <summary>The cash amount for the transaction.</summary>
        /// <value>Type: <see cref="System.Decimal" /><para>Echoed from the request.</para></value>
        public decimal AmountCash { get; set; } = 0;

		/// <summary>The purchase amount for the transaction.</summary>
		/// <value>Type: <see cref="System.Decimal" /><para>Echoed from the request.</para></value>
		public decimal AmountPurchase { get; set; } = 0;

        /// <summary>The tip amount for the transaction.</summary>
        /// <value>Type: <see cref="System.Decimal" /><para>Echoed from the request.</para></value>
        public decimal AmountTip { get; set; } = 0;

        /// <summary>The authorisation number for the transaction.</summary>
        /// <value>Type: <see cref="System.Int32" /><para>Authorization number returned by the bank.</para></value>
        public int AuthNumber { get; set; } = 0;

        /// <summary>The reference number sent in the request to uniquely identify this transaction.</summary>
        /// <value>Type: <see cref="System.String" /><para>Echoed from the request.</para></value>
        public string ReferenceNumber { get; set; } = "";

        /// <summary>The PAN of the card.</summary>
        /// <value>Type: <see cref="System.String" /><para>This property contains the the partial card number used in this transaction.</para></value>
        public string CardPAN { get; set; } = "";

        /// <summary>The expiry date on the card.</summary>
        /// <value>Type: <see cref="System.String" /><para>This property contains the expiry date (MMYY) of the card used int this transaction.</para></value>
        public string ExpiryDate { get; set; } = "";

        /// <summary>The track 2 data on the magnetic stripe of the card.</summary>
        /// <value>Type: <see cref="System.String" /><para>This property contains the partial track 2 data from the card used in this transaction.</para></value>
        public string Track2 { get; set; } = "";

        /// <summary>The account used for the transaction.</summary>
        /// <value>Type: <see cref="AccountType" /><para>This is the account type selected by the customer or provided in the request.</para></value>
        public AccountType CardAccountType { get; set; } = AccountType.Default;

        /// <summary>Flags that indicate how the transaction was processed.</summary>
        /// <value>Type: <see cref="TxnFlags" /></value>
        public TxnFlags TxnFlags { get; set; } = new TxnFlags();

        /// <summary>Indicates if an available balance is present in the response.</summary>
        /// <value>Type: <see cref="System.Boolean" /></value>
        public bool BalanceReceived { get; set; } = false;

        /// <summary>Balance available on the processed account.</summary>
        /// <value>Type: <see cref="System.Decimal" /></value>
        public decimal AvailableBalance { get; set; } = 0;

        /// <summary>Cleared balance on the processed account.</summary>
        /// <value>Type: <see cref="System.Decimal" /></value>
        public decimal ClearedFundsBalance { get; set; } = 0;

        /// <summary>Indicates if the request was successful.</summary>
        /// <value>Type: <see cref="System.Boolean"/></value>
        public bool Success { get; set; } = false;

        /// <summary>The response code of the request.</summary>
        /// <value>Type: <see cref="System.String"/><para>A 2 character response code. "00" indicates a successful response.</para></value>
        public string ResponseCode { get; set; } = "";

        /// <summary>The response text for the response code.</summary>
        /// <value>Type: <see cref="System.String"/></value>
        public string ResponseText { get; set; } = "";

        /// <summary>Date and time of the response returned by the bank.</summary>
        /// <value>Type: <see cref="System.DateTime"/></value>
        public DateTime BankDateTime { get; set; } = DateTime.MinValue;

        /// <summary>Terminal ID configured in the PIN pad.</summary>
        /// <value>Type: <see cref="System.String"/><para>An 8 character terminal ID.</para></value>
        public string TerminalID { get; set; } = "";

        /// <summary>Merchant ID configured in the PIN pad.</summary>
        /// <value>Type: <see cref="System.String"/><para>A 15 character terminal ID.</para></value>
        public string MerchantID { get; set; } = "";

        /// <summary>System Trace Audit Number</summary>
        /// <value>Type: <see cref="System.Int32"/></value>
        public int STAN { get; set; } = 0;

        /// <summary>Additional information sent with the response.</summary>
        /// <value>Type: <see cref="PadField"/></value>
        public PadField PurchaseAnalysisData { get; set; } = new PadField();
    }
}