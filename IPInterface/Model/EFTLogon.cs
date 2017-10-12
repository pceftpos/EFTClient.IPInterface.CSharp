using System;
using PCEFTPOS.Messaging;

namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>Indicates the type of logon to perform.</summary>
    public enum LogonType
    {
        /// <summary>Standard EFT logon to the bank.</summary>
        Standard = ' ',
        /// <summary>Standard EFT logon to the bank.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        RSA = '4',
        /// <summary>Standard EFT logon to the bank.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        TMSFull = '5',
        /// <summary>Standard EFT logon to the bank.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        TMSParams = '6',
        /// <summary>Standard EFT logon to the bank.</summary>
        /// <remarks>Not supported by all PIN pads.</remarks>
        TMSSoftware = '7',
        /// <exclude/>
        Logoff = '8',
        /// <summary>Enables diagnostics.</summary>
        Diagnostics = '1'
    }

    /// <summary>A PC-EFTPOS terminal logon request object.</summary>
    public class EFTLogonRequest : EFTRequest
    {
        /// <summary>Constructs a default terminal logon request object.</summary>
        public EFTLogonRequest() : base()
        {
        }

        /// <summary>Constructs a terminal logon request object.</summary>
        /// <param name="LogonType">The logon type to perform.</param>
        public EFTLogonRequest(LogonType LogonType) : base()
        {
            this.LogonType = LogonType;
        }

        /// <summary>type of logon to perform.</summary>
        /// <value>Type: <see cref="LogonType" /><para>The default is <see cref="LogonType.Standard" />.</para></value>
        public LogonType LogonType { get; set; } = LogonType.Standard;

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
    }

    /// <summary>A PC-EFTPOS terminal logon response object.</summary>
    public class EFTLogonResponse : EFTResponse
    {
        /// <summary>Constructs a default terminal logon response object.</summary>
        public EFTLogonResponse() : base()
        {
        }

        /// <summary>PIN pad software version.</summary>
        /// <value>Type: <see cref="System.String" /></value>
        public string PinpadVersion { get; set; } = "";

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