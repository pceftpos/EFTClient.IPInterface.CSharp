# IPInterface - a wrapper for the EFT-Client TCP/IP interface

## Project structure

| Folder                      | Description   
| ---------------------------  | -------------
| IPInterface                 | A project with both event (`EFTClientIP`) and async/await based (`EFTClientIPAsync`) wrapper objects for the EFT-Client TCP/IP interface.
| IPInterface.SimpleDemoAsync | A basic sample app using the `EFTClientIPAsync` component
| IPInterface.SimpleDemo      | A basic sample app using the `EFTClientIP` component
| IPInterface.TestPOS         | A full featured sample app using the `EFTClientIPAsync` component

## Getting started
* Clone this repository or grab the [PCEFTPOS.EFTClient.IPInterface](https://www.nuget.org/packages/PCEFTPOS.EFTClient.IPInterface/) package from NuGet
* Decide which component to use. `EFTClientIP` is event based pattern and `EFTClientIPAsync` uses the async/await pattern
* Look at the `SimpleDemo` and `SimpleDemoAsync` samples. There are also some simple examples listed below.

##### Example usage for EFTClientIP
``` csharp
class EFTClientIPDemo
    {
        ManualResetEvent txnFired = new ManualResetEvent(false);

        public void Run()
        {
            // Create new connection to EFT-Client
            var eft = new EFTClientIP()
            {
                HostName = "127.0.0.1",
                HostPort = 2011,
                UseSSL = false
            };
            // Hook up events
            eft.OnReceipt += Eft_OnReceipt;
            eft.OnTransaction += Eft_OnTransaction;
            eft.OnTerminated += Eft_OnTerminated;
            // Connect
            if (!eft.Connect())
            {
                // Handle failed connection
                Console.WriteLine("Connect failed");
                return;
            }

            // Build transaction request
            var r = new EFTTransactionRequest()
            {
                // TxnType is required
                TxnType = TransactionType.PurchaseCash,
                // Set TxnRef to something unique
                TxnRef = DateTime.Now.ToString("YYMMddHHmmsszzz"),
                // Set AmtCash for cash out, and AmtPurchase for purchase/refund
                AmtPurchase = 1.00M,
                AmtCash = 0.00M,
                // Set POS or pinpad printer
                ReceiptPrintMode = ReceiptPrintModeType.POSPrinter,
                // Set application. Used for gift card & 3rd party payment
                Application = TerminalApplication.EFTPOS
            };
            // Send transaction
            if (!eft.DoTransaction(r))
            {
                // Handle failed send
                Console.WriteLine("Send failed");
                return;
            }

            txnFired.WaitOne();
            eft.Disconnect();
            eft.Dispose();
        }

        private void Eft_OnTerminated(object sender, SocketEventArgs e)
        {
            // Handle socket close
            Console.WriteLine($"Socket closed");
            txnFired.Reset();
        }

        private void Eft_OnReceipt(object sender, EFTEventArgs<EFTReceiptResponse> e)
        {
            // Handle receipt
            Console.WriteLine($"{e.Response.Type} receipt");
            Console.WriteLine($"{e.Response.ReceiptText}");
        }

        private void Eft_OnTransaction(object sender, EFTEventArgs<EFTTransactionResponse> e)
        {
            // Handle transaction event 
            var displayText = e.Response.Success ? "successful" : "unsuccessful";
            Console.WriteLine($"Transaction was {displayText}");
            txnFired.Set();
        }
    }
    
    class Program
    {
        static async void Main(string[] args)
        {
            (new EFTClientIPDemo()).Run();
            Console.WriteLine("Press any key to quit");
            Console.ReadLine();
        }
    }    
```

##### Example usage for EFTClientIPAsync
``` csharp
class EFTClientIPDemoAsync
    {
        public async Task RunAsync()
        {
            // Create new connection to EFT-Client
            var eft = new EFTClientIPAsync();
            var connected = await eft.ConnectAsync("127.0.0.1", 2011, false);
            if(!connected)
            {
                // Handle failed connection
                Console.WriteLine("Connect failed"); 
            }

            // Build transaction request
            var r = new EFTTransactionRequest()
            {
                // TxnType is required
                TxnType = TransactionType.PurchaseCash,
                // Set TxnRef to something unique
                TxnRef = DateTime.Now.ToString("YYMMddHHmmsszzz"),
                // Set AmtCash for cash out, and AmtPurchase for purchase/refund
                AmtPurchase = 1.00M,
                AmtCash = 0.00M,
                // Set POS or pinpad printer
                ReceiptPrintMode = ReceiptPrintModeType.POSPrinter,
                // Set application. Used for gift card & 3rd party payment
                Application = TerminalApplication.EFTPOS
            };
            
            // Send transaction
            if (await eft.WriteRequestAsync(r) == false)
            {
                // Handle failed send
                Console.WriteLine("Send failed");
                return;
            }

            // Wait for response
            var waitingForResponse = true;
            do
            {
                EFTResponse eftResponse = null;
                try
                {
                    var timeoutToken = new CancellationTokenSource(new TimeSpan(0, 5, 0)).Token; // 5 minute timeout
                    eftResponse = await eft.ReadResponseAsync(timeoutToken);

                    switch(eftResponse)
                    {
                        case EFTReceiptResponse eftReceiptResponse:
                            Console.WriteLine($"{eftReceiptResponse.Type} receipt");
                            Console.WriteLine($"{eftReceiptResponse.ReceiptText}");
                            break;
                        case EFTTransactionResponse eftTransactionResponse:
                            var displayText = eftTransactionResponse.Success ? "successful" : "unsuccessful";
                            Console.WriteLine($"Transaction was {displayText}");
                            waitingForResponse = false;
                            break;
                        case null:
                            Console.WriteLine("Error reading response");
                            break;
                    }
                }
                catch(TaskCanceledException)
                {
                    Console.WriteLine("EFT-Client timeout waiting for response");
                    waitingForResponse = false;
                }
                catch(ConnectionException)
                {
                    Console.WriteLine("Socket closed");
                    waitingForResponse = false;
                }
                catch(Exception)
                {
                    Console.WriteLine("Unhandled exception");
                    waitingForResponse = false;
                }
            }
            while (waitingForResponse); 

            eft.Disconnect();
        }
    }

    class Program
    {
        static async void Main(string[] args)
        {
            await (new EFTClientIPDemoAsync()).RunAsync();
            Console.WriteLine("Press any key to quit");
            Console.ReadLine();
        }
    }
```

## Release notes

### 1.4.5.0 (2018-12-14)
• Added in Void transaction type...
• Added in a check on msg length for parsing Duplicate Receipt responses so it can handle TPP duplicate responses
• Fixed 'Display Swipe Card' slave command
• Added in support for Input On POS display requests
• Added in MerchantNumber field for GetLastReceipt

### 1.4.3.0 (2018-10-09)
* Deleted a hard-coded TxnRef in TestPOS GetLast and ReprintReceipt command
* Fixed bug in MessageParser that padded the TxnRef rather than leaving it blank, so the EFTClient didn't like it

### 1.4.2.0 (2018-09-19)
* Added new ReceiptAutoPrint modes for EFTRequests
* Updated MessageParser to use non-deprecated properties
* Updated TestPOS ClientViewModel to do the same

### 1.4.1.3 (2018-09-12)
* Fixed for EFTTransactionResponse and typo

### 1.4.1.2 (2018-09-12)
* Changes to fields ReceiptAutoPrint, CutReceipt, AccountType and DateSettlement

### 1.4.1.1 (2018-08-29)
* Added support for EFTGetLastTransactionRequest by TxnRef

### 1.4.1.0 (2018-07-17)
* Updated PadField to support IList<PadTag>

### 1.4.0.0 (2018-04-30)
* Added IDialogUIHandler for easier handling of POS custom dialogs.
* Updated MessageParser to allow for custom parsing.

### 1.3.5.0 (2018-02-16)
* Added support for .NET Standard 2.0
* Added support for basket data API
* Updated some property names to bring EFTClientIP more inline with the existing ActiveX interface. Old property names have been marked obsolete, but are still supported.

### 1.3.3.0 (2017-10-26)
* Changed internal namespaces from `PCEFTPOS.*` (`PCEFTPOS.Net`, `PCEFTPOS.Messaging` etc) to `PCEFTPOS.EFTClient.IPInterface`. This was causing issues when combining the EFTClientIP Nuget package with the actual PCEFTPOS lib. EFTClientIP needs to remain totally self-contained. 

### 1.3.2.0 (2017-19-09)
* Updated nuspec for v1.3.2.0 release.

### 1.3.1.0 (2017-09-13)
* Changed namespace from `PCEFTPOS.API.IPInterface` to `PCEFTPOS.EFTClient.IPInterface` for new package
* Created signed NuGet package

### 1.2.1.0 (2017-07-11)
* Added CloudLogon event to EFTClientIP

### 1.1.0.0 (2017-06-30)
* Fixed a bug that would cause the component to hang if an unknown message was received 
* Improved handling of messages received across multiple IP packets
* Added support for Pay at Table

### 1.0.0.1 (2016-10-28)
* Initial release