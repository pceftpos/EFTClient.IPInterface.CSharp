# Upgrading from PCEFTIPInterface

The POS interface to older `PCEFTIPInterface` and the newer `PCEFTPOS.EFTClient.IPInterface` is virtually identical. In most cases updating from the older interface to the new one should only take a few minutes.

`PCEFTPOS.EFTClient.IPInterface` currently targets .NET Framework 4.5.2, and will support .NET Standard 2.0. (.NET Framework 4.5.2 will be maintained in a separate branch)

### Why upgrade
* Support for PC-EFTPOS Cloud PIN pads
* NuGet integration
* Targets a more recent .NET Framework
* Bug fixes and performance improvements

### Update namespace references
The namespace has changed. 
* Change `using PCEFTIPInterface;` to `using PCEFTPOS.EFTClient.IPInterface;`
* Build the `IPInterface` library and add it to your project, or add a reference to the `PCEFTPOS.EFTClient.IPInterface` NuGet package.

### Update object references
Some request & response objects were names inconsistently in the old `PCEFTIPInterface`.
* Prepend "EFT" to request and response objects. e.g. `QueryCardResponse` becomes `EFTQueryCardResponse`

### Change event signatures
The event signature has changed. 

```c#
void OnTransaction(object sender, EFTClientIPEventArgs e)
{
  string responseCode = e.EFTTransaction.SuccessResponse.ResponseCode;
  string responseText = e.EFTTransaction.SuccessResponse.ResponseText;
  bool success = e.EFTTransaction.SuccessResponse.Success;
  string cardType = e.EFTTransaction.CardType;
  string accountType = e.EFTTransaction.CardAccountType.ToString();
}
```

becomes 

```c#
void OnTransaction(object sender, EFTEventArgs<EFTTransactionResponse> e)
{
  string responseCode = e.Response.ResponseCode;
  string responseText = e.Response.ResponseText;
  bool success = e.Response.Success;
  string cardType = e.Response.CardType;
  string accountType = e.Response.CardAccountType.ToString();
}
```


### Update references to ReceiptAutoPrint, CutReceipt and Application.
1. Move property references from EFTClientIP into the request
2. bool ReceiptAutoPrint is now enum ReceiptPrintMode
3. bool CutReceipt is now enum ReceiptCutMode 

e.g.
```c#
// Set global properties
eftClientIP.ReceiptAutoPrint = false;
eftClientIP.CutReceipt = false;
eftClientIP.Application = TerminalApplication.EFTPOS;

// Create the request
EFTTransactionRequest request = new EFTTransactionRequest()
{
  Type = TransactionType.PurchaseCash,
  AmountPurchase = 1.00
}

// Send the request
bool successful = eftClientIP.DoTransaction(request);
```

becomes

```c#
// Create the request
EFTTransactionRequest request = new EFTTransactionRequest()
{
  Type = TransactionType.PurchaseCash, 
  AmountPurchase = 1.00,
  ReceiptPrintMode = ReceiptPrintModeType.POSPrinter,
  ReceiptCutMode = ReceiptCutModeType.Cut,
  Application = TerminalApplication.EFTPOS
}

// Send the request
bool successful = eftClientIP.DoTransaction(request);
```


### Update references to EFTClientIP.Stop();
This function name has changed. `eftClientIP.Stop();` is now `eftClientIP.Disconnect();`

### Remove references to EFTClientIP.LogFile
EFTClientIP.LogFile has been removed. The POS can now capture logs from `EFTClientIP` using the `EFTClientIP.OnLog` event.


