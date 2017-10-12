# IPInterface - a wrapper for the EFT-Client TCP/IP interface

## Project structure

| Folder                      | Description   
| ---------------------------  | -------------
| IPInterface                 | A project with both event (`EFTClientIP`) and async/await based (`EFTClientIPAsync`) wrapper objects for the EFT-Client TCP/IP interface.
| IPInterface.SimpleDemoAsync | A basic sample app using the `EFTClientIPAsync` component
| IPInterface.SimpleDemo      | A basic sample app using the `EFTClientIP` component
| IPInterface.TestPOS         | A full featured sample app using the `EFTClientIPAsync` component

## Getting started

> If you are updating from the older `PCEFTIPInterface`, please refer to [Upgrading from PCEFTIPInterface](UPDATE.md)

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
                // Set ReferenceNumber to something unique
                ReferenceNumber = DateTime.Now.ToString("YYMMddHHmmsszzz"),
                // Set AmountCash for cash out, and AmountPurchase for purchase/refund
                AmountPurchase = 1.00M,
                AmountCash = 0.00M,
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
                // Set ReferenceNumber to something unique
                ReferenceNumber = DateTime.Now.ToString("YYMMddHHmmsszzz"),
                // Set AmountCash for cash out, and AmountPurchase for purchase/refund
                AmountPurchase = 1.00M,
                AmountCash = 0.00M,
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

### 1.3.2.0 (2017-09-19)
* Signed assembly.

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
