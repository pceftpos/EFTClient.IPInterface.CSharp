using System;

namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary> 
    /// Receipt mode (pos, windows or pinpad printer).
    /// Sometimes called the "ReceiptAutoPrint" flag
    /// </summary>
    public enum ReceiptPrintModeType
    {
        /// <summary> Receipts will be passed back to the POS in the PrintReceipt event </summary>
        POSPrinter = '0',
        /// <summary> The EFT-Client will attempt to print using the printer configured in the EFT-Client (Windows only) </summary>
        EFTClientPrinter = '1',
        /// <summary> Receipts will be printed using the pinpad printer </summary>
        PinpadPrinter = '9'
    }

    /// <summary> 
    /// Receipt cut mode (cut or don't cut). Used when the EFT-Client is handling receipts (ReceiptPrintMode = ReceiptPrintModeType.EFTClientPrinter)
    /// Sometimes called the "CutReceipt" flag
    /// </summary>
    public enum ReceiptCutModeType
    {
        /// <summary> Don't cut receipts </summary>
        DontCut = '0',
        /// <summary> Cut receipts </summary>
        Cut = '1'
    }

    /// <summary>Abstract base class for EFT client requests.</summary>
    public abstract class EFTRequest
    {
    }

    /// <summary>Abstract base class for EFT client responses.</summary>
    public abstract class EFTResponse
    {
    }
}
