using System;
using PCEFTPOS.Messaging;
using PCEFTPOS.Util;

namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>A PC-EFTPOS get last transaction request object.</summary>
	public class EFTGetLastTransactionRequest : EFTRequest
	{
		/// <summary>Constructs a default EFTGetLastTransactionRequest object.</summary>
		public EFTGetLastTransactionRequest() : base()
		{
		}

		/// <summary>Indicates where the request is to be sent to. Should normally be EFTPOS.</summary>
		/// <value>Type: <see cref="TerminalApplication"/><para>The default is <see cref="TerminalApplication.EFTPOS"/>.</para></value>
		public TerminalApplication Application { get; set; } = TerminalApplication.EFTPOS;
	}

	/// <summary>A PC-EFTPOS get last transaction response object.</summary>
	public class EFTGetLastTransactionResponse : EFTResponse
	{
		bool lastTxnSuccess;
		TransactionType txnType;
		AccountType accountType;
		decimal purchaseAmt;
		decimal cashAmt;
		decimal tipAmt;
		int authNumber;
		string txnRefNum;
		string expiryDate;
		DateTime settlementDate;
		string cardType;
		string cardPAN;
		string track2;
		string rrn;
		int cardBinNumber;
		string[] receiptText;
		TxnFlags txnFlags;
		bool balanceReceived;
		decimal balance;
		bool isTrainingMode;

		/// <summary>Constructs a default terminal transaction response object.</summary>
		public EFTGetLastTransactionResponse()
			: base()
		{
			txnType = TransactionType.PurchaseCash;
			accountType = AccountType.Default;
			purchaseAmt = 0;
			cashAmt = 0;
			tipAmt = 0;
			authNumber = 0;
			txnRefNum = "";
			expiryDate = "";
			settlementDate = DateTime.MinValue;
			cardType = "";
			cardPAN = "";
			track2 = "";
			rrn = "";
			cardBinNumber = 0;
			receiptText = new string[] { "" };
			txnFlags = new TxnFlags();
			balanceReceived = false;
			balance = 0;
			isTrainingMode = false;
		}

        /// <summary>The type of EFTPOS transaction that was requested.</summary>
        /// <value>Type: <see cref="TransactionType" /><para>Echoed from the request.</para></value>
        public TransactionType TxnType
        {
            get { return txnType; }
            set { txnType = value; }
        }

        /// <summary>The type of EFTPOS transaction that was requested.</summary>
        /// <value>Type: <see cref="TransactionType" /><para>Echoed from the request.</para></value>
        [System.Obsolete("Please use TxnType instead")]
        public TransactionType Type { get { return TxnType; } set { TxnType = value; } }

        /// <summary>Indicates the card type that was used in the transaction.</summary>
        /// <value>Type: <see cref="System.String" /></value>
        /// <remarks><seealso cref="EFTTransactionResponse.CardBIN"/></remarks>
        public string CardType
		{
			get { return cardType; }
			set { cardType = value; }
		}

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
		public int CardBIN
		{
			get { return cardBinNumber; }
			set { cardBinNumber = value; }
		}

		/// <summary>Used to retrieve the transaction from the batch.</summary>
		/// <value>Type: <see cref="System.String" /></value>
		/// <remarks>The retrieval reference number is used when performing a tip adjustment transaction.</remarks>
		public string RRN
		{
			get { return rrn; }
			set { rrn = value; }
		}

		/// <summary>Indicates which settlement batch this transaction will be included in.</summary>
		/// <value>Type: <see cref="System.DateTime" /><para>Settlement date is returned from the bank.</para></value>
		/// <remarks>Use this property to balance POS EFT totals with settlement EFT totals.</remarks>
		public DateTime SettlementDate
		{
			get { return settlementDate; }
			set { settlementDate = value; }
		}

		/// <summary>The cash amount for the transaction.</summary>
		/// <value>Type: <see cref="System.Decimal" /><para>Echoed from the request.</para></value>
		public decimal AmountCash
		{
			get { return cashAmt; }
			set { cashAmt = value; }
		}

		/// <summary>The purchase amount for the transaction.</summary>
		/// <value>Type: <see cref="System.Decimal" /><para>Echoed from the request.</para></value>
		public decimal AmountPurchase
		{
			get { return purchaseAmt; }
			set { purchaseAmt = value; }
		}

		/// <summary>The tip amount for the transaction.</summary>
		/// <value>Type: <see cref="System.Decimal" /><para>Echoed from the request.</para></value>
		public decimal AmountTip
		{
			get { return tipAmt; }
			set { tipAmt = value; }
		}

		/// <summary>The authorisation number for the transaction.</summary>
		/// <value>Type: <see cref="System.Int32" /><para>Authorization number returned by the bank.</para></value>
		public int AuthNumber
		{
			get { return authNumber; }
			set { authNumber = value; }
		}

		/// <summary>The reference number sent in the request to uniquely identify this transaction.</summary>
		/// <value>Type: <see cref="System.String" /><para>Echoed from the request.</para></value>
		public string ReferenceNumber
		{
			get { return txnRefNum; }
			set { txnRefNum = value; }
		}

		/// <summary>The PAN of the card.</summary>
		/// <value>Type: <see cref="System.String" /><para>This property contains the the partial card number used in this transaction.</para></value>
		public string CardPAN
		{
			get { return cardPAN; }
			set { cardPAN = value; }
		}

		/// <summary>The expiry date on the card.</summary>
		/// <value>Type: <see cref="System.String" /><para>This property contains the expiry date (MMYY) of the card used int this transaction.</para></value>
		public string ExpiryDate
		{
			get { return expiryDate; }
			set { expiryDate = value; }
		}

		/// <summary>The track 2 data on the magnetic stripe of the card.</summary>
		/// <value>Type: <see cref="System.String" /><para>This property contains the partial track 2 data from the card used in this transaction.</para></value>
		public string Track2
		{
			get { return track2; }
			set { track2 = value; }
		}

		/// <summary>The account used for the transaction.</summary>
		/// <value>Type: <see cref="AccountType" /><para>This is the account type selected by the customer or provided in the request.</para></value>
		public AccountType CardAccountType
		{
			get { return accountType; }
			set { accountType = value; }
		}

		/// <summary>Indicates if the last transaction was successful.</summary>
		/// <value>Type: <see cref="System.Boolean" /></value>
		public bool LastTransactionSuccess
		{
			get { return lastTxnSuccess; }
			set { lastTxnSuccess = value; }
		}

		/// <summary>Flags that indicate how the transaction was processed.</summary>
		/// <value>Type: <see cref="TxnFlags" /></value>
		public TxnFlags TxnFlags
		{
			get { return txnFlags; }
			set { txnFlags = value; }
		}

		/// <summary>Indicates if an available balance is present in the response.</summary>
		/// <value>Type: <see cref="System.Boolean" /></value>
		public bool BalanceReceived
		{
			get { return balanceReceived; }
			set { balanceReceived = value; }
		}

		/// <summary>Balance available on the processed account.</summary>
		/// <value>Type: <see cref="System.Decimal" /></value>
		public decimal AvailableBalance
		{
			get { return balance; }
			set { balance = value; }
		}

		/// <summary>Indicates if the transaction was perform as a training mode transaction.</summary>
		/// <value>Type: <see cref="System.Boolean" /></value>
		public bool IsTrainingMode
		{
			get { return isTrainingMode; }
			set { isTrainingMode = value; }
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