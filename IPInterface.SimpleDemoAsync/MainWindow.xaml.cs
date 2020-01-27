using PCEFTPOS.EFTClient.IPInterface;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace PCEFTPOS.EFTClient.IPInterface.SimpleDemoAsync
{
    [DataContract]
    public class EFTBasketItemCustom: EFTBasketItem
    {
        [DataMember(Name = "customValue")]
        public string CustomItemTest { get; set; } = "This is a test";
    }

    public class ApiResponse<T>
    {
        public string SessionId { get; set; }
        public string ResponseType { get; set; }
        public T Response { get; set; }
    }

    public class TransactionResponse
    {
        public bool Success { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
    }

    public class TokenResponse
    {
        public string Token { get; set; }
        public int ExpirySeconds { get; set; }
    }

    public class PairingResponse
    {
        public string Secret { get; set; }
    }

    public partial class MainWindow : Window
    {

        IEFTClientIPAsync _eft = null;
        ISettings _settings = null;
        bool _isServerVerified = false;
        bool _navigating = false;

        //TOKENS
        private readonly string posName = "SIMPLE REST DEMO";
        private readonly string posVersion = "1.0.0";
        private readonly string posId = "7253230A-8983-4839-9744-CCA8291F9DB4";

        private readonly string baseAuthApiUri = "https://auth.sandbox.cloud.pceftpos.com/v1";
        private readonly string basePosApiUri = "https://rest.pos.sandbox.cloud.pceftpos.com/v1";

        private readonly string defaultCloudURL = "pos.sandbox.cloud.pceftpos.com:443";

        //private readonly string baseAuthApiUri = "http://localhost:53194/v1";
        //private readonly string basePosApiUri = "http://localhost:53194/v1";

        string token = null;
        private DateTime tokenExpiry = new DateTime();
        readonly HttpClient client = new HttpClient();

        enum NotificationType { Normal, Error, Success }

        public MainWindow()
        {
            _settings = new Settings();
            _settings.Load();

            InitializeComponent();
        }

        //async void TokenTxn()
        //{// Get an auth token 
        //    try
        //    {
        //        await RefreshTokenAsync();
        //
        //        ShowNotification("TRANSACTION IN PROGRESS", "", "CHECK PIN PAD FOR STATUS", NotificationType.Normal, false);
        //
        //        // TxnType is required
        //        //string txnType = GetTxnType().ToString();
        //        string txnType = cboType.Text[0].ToString(); //Hacky thing i'll fix later
        //                                                     // Set ReferenceNumber to something unique
        //        string txnRef = DateTime.Now.ToString("YYMMddHHmmssfff");
        //        // Set AmountCash for cash out, and AmountPurchase for purchase/refund
        //        int amtPurchase = (txnType == "C") ? 0 : (int)(decimal.Parse(txtAmount.Text) * 100);
        //        int amtCash = (txnType == "C") ? (int)(decimal.Parse(txtAmount.Text) * 100) : 0;
        //
        //        var requestContent = new { Request = new { txnType, amtPurchase, amtCash, txnRef } };
        //        var currentSessionId = Guid.NewGuid().ToString();
        //
        //
        //        var request = new HttpRequestMessage(HttpMethod.Post, $"{basePosApiUri}/sessions/{currentSessionId}/transaction?async=false")
        //        {
        //            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestContent), System.Text.Encoding.UTF8, "application/json")
        //        };
        //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //        HttpResponseMessage httpResponse = await client.SendAsync(request);
        //        httpResponse.EnsureSuccessStatusCode();
        //
        //        var apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<TransactionResponse>>(await httpResponse.Content.ReadAsStringAsync());
        //        var r = apiResponse.Response;
        //
        //        HideDialog();
        //
        //        ShowNotification(
        //            $"TRANSACTION {(r.Success ? "OK" : "FAILED")}",
        //            $"{r.ResponseCode} {r.ResponseText}",
        //            "",
        //            r.Success ? NotificationType.Success : NotificationType.Error,
        //            true
        //            );
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowNotification("TRANSACTION FAILURE", "", ex.Message, NotificationType.Error, true);
        //    }
        //}

        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtToken.Text = _settings.Token;
            txtPairingCode.Text = _settings.PairingCode;
            txtUsername.Text = _settings.Username;
            txtPassword.Text = _settings.Password;
            cboEnableCloud.IsChecked = _settings.EnableCloud;
            txtEFTClientAddress.Text = _settings.EFTClientAddress;
            // Set up PC-EFTPOS connection
            _eft = new EFTClientIPAsync();

            var connected = false;
            //If we already have the token, we can do a token auth
            if (!string.IsNullOrEmpty(_settings?.Token) && _settings.EnableCloud)
            {
                connected = await ConnectAsync();
            }
            // Try to connect if we have an address. Either navigate to the main 
            // page (if we are connected) or the settings page (if we aren't)
           else if (_settings?.EFTClientAddress.Length > 0 && connected == false)
           {
                connected = await ConnectAsync();
           }

            if (connected)
            {
                NavigateToMainPage();
            }
            else
            {
                NavigateToSettingsPage();
            }
        }

        //Debugging Function, Not Used In Production
        void BtnTempClearSettings(object sender, RoutedEventArgs e)
        {
            _settings.Username ="";
            _settings.Password = "";
            _settings.PairingCode = "";
            _settings.Token = "";
            _settings.EnableCloud = false;
            _settings.EFTClientAddress = "127.0.0.1:2011";
            _settings.Save();
        }

        void BtnTempSaveSettings(object sender, RoutedEventArgs e)
        {
            _settings.Username = txtUsername.Text;
            _settings.Password = txtPassword.Text;
            _settings.PairingCode = txtPairingCode.Text;
            _settings.Token = txtToken.Text;
            _settings.EnableCloud = cboEnableCloud.IsChecked.Value;
            _settings.EFTClientAddress = txtEFTClientAddress.Text;
            _settings.Save();
        }

        async void BtnPair_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _settings.Username = txtUsername.Text;
                _settings.Password = txtPassword.Text;
                _settings.PairingCode = txtPairingCode.Text;

                //var uri = $"{baseAuthApiUri}/pairing/cloudpos";
                //var request = new HttpRequestMessage(HttpMethod.Post, uri)
                //{
                //    Content = new StringContent(JsonConvert.SerializeObject(new { username = _settings.Username, password = _settings.Password, paircode = _settings.PairingCode, posName, posVersion, posId }), System.Text.Encoding.UTF8, "application/json")
                //};
                //
                //var httpResponse = await client.SendAsync(request);
                //httpResponse.EnsureSuccessStatusCode();
                //
                //var pairingResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<PairingResponse>(await httpResponse.Content.ReadAsStringAsync());
                var enableCloud = _settings.EnableCloud;
                int tmpPortVar = 0;
                var addr = _settings.EFTClientAddress.Split(new char[] { ':' });
                if (addr.Length < 2 || int.TryParse(addr[1], out tmpPortVar) == false)
                {
                    ShowNotification("INVALID ADDRESS", "", "", NotificationType.Error, true);
                }
                bool connected = false;
                try
                {
                    connected = await _eft.ConnectAsync(addr[0], tmpPortVar, enableCloud, enableCloud);
                }
                catch (Exception)
                {
                    connected = false;
                }

                var waiting = await _eft.WriteRequestAsync(new EFTCloudPairRequest { ClientID = _settings.Username, Password = _settings.Password, PairingCode = _settings.PairingCode });
                while (waiting)
                {
                    var eftResponse = await _eft.ReadResponseAsync(new CancellationTokenSource(45000).Token);
                    if (eftResponse is EFTCloudPairResponse)
                    {
                        var cloudLogonResponse = eftResponse as EFTCloudPairResponse;
                        if (!cloudLogonResponse.Success)
                        {
                            connected = false;
                        }
                        else
                        {
                            _settings.Token = cloudLogonResponse.Token;
                        }
                        waiting = false;
                    }
                }
                _settings.Save();

                // Update UI
                txtToken.Text = _settings.Token;
                //DO A simple logon when pairing???
                if (await ConnectAsync())
                {
                    NavigateToMainPage();
                }
                else
                {
                    NavigateToSettingsPage();
                }
            }
            catch(Exception ex)
            {
                ShowNotification("PAIRING FAILURE", "", ex.Message, NotificationType.Error, true);
            }
        }
        async Task<bool> ConnectAsyncToken()
        {
            ShowNotification("REFRESHING TOKEN, PLEASE WAIT...", "", "", NotificationType.Normal, false);

            try
            {
                await RefreshTokenAsync();
                HideNotification();
                UpdateSettingsPageUI();
            }
            catch (Exception e)
            {
                UpdateSettingsPageUI();
                ShowNotification("ERROR CONTACTING PC-EFTPOS CLOUD", "", e.Message, NotificationType.Error, true);
                return false;
            }
            var connected = await ConnectAsync();
            return connected;
        }

        async Task<string> RefreshTokenAsync()
        {
            //var uri = $"{baseAuthApiUri}/tokens/cloudpos";
            //var Secret = _settings.Token;
            //var request = new HttpRequestMessage(HttpMethod.Post, uri)
            //{
            //    Content = new StringContent(JsonConvert.SerializeObject(new { Secret, posName, posVersion, posId }), System.Text.Encoding.UTF8, "application/json")
            //};
            //
            //var httpResponse = await client.SendAsync(request);
            //httpResponse.EnsureSuccessStatusCode();
            //
            //var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(await httpResponse.Content.ReadAsStringAsync());
            //token = tokenResponse.Token;
            //tokenExpiry = DateTime.Now.AddSeconds(tokenResponse.ExpirySeconds);
            bool connected = true;
            var waiting = await _eft.WriteRequestAsync(new EFTCloudTokenLogonRequest() { Token = _settings.Token });
            while (waiting)
            {
                var eftResponse = await _eft.ReadResponseAsync(new CancellationTokenSource(45000).Token);
                if (eftResponse is EFTCloudTokenLogonResponse)
                {
                    var cloudLogonResponse = eftResponse as EFTCloudTokenLogonResponse;
                    if (!cloudLogonResponse.Success)
                    {
                        connected = false;
                    }
                    waiting = false;
                }
            }

            return token;
        }
       //async Task<bool> ConnectAsyncSimple()
       //{
       //    var enableCloud = _settings.EnableCloud;
       //    var addr = _settings.EFTClientAddress.Split(new char[] { ':' });
       //    if (addr.Length < 2 || int.TryParse(addr[1], out int tmpPort) == false)
       //    {
       //        ShowNotification("INVALID ADDRESS", "", "", NotificationType.Error, true);
       //        return false;
       //    }
       //
       //    //ShowNotification("CONTACTING EFT-CLIENT, PLEASE WAIT...", "", "", NotificationType.Normal, false);
       //    bool connected = false;
       //    try
       //    {
       //        connected = await _eft.ConnectAsync(addr[0], tmpPort, enableCloud, enableCloud);
       //    }
       //    catch (Exception)
       //    {
       //        connected = false;
       //    }
       //    return connected;
       //}
        async Task<bool> ConnectAsync()
        {
            try
            {
                var enableCloud = _settings.EnableCloud;
                var addr = _settings.EFTClientAddress.Split(new char[] { ':' });
                if (addr.Length < 2 || int.TryParse(addr[1], out int tmpPort) == false)
                {
                    ShowNotification("INVALID ADDRESS", "", "", NotificationType.Error, true);
                    return false;
                }

                ShowNotification("CONTACTING EFT-CLIENT, PLEASE WAIT...", "", "", NotificationType.Normal, false);
                bool connected = false;
                try
                {
                    connected = await _eft.ConnectAsync(addr[0], tmpPort, enableCloud, enableCloud);
                }
                catch (Exception)
                {
                    connected = false;
                }

                var notifyText = "";

                if (connected && enableCloud)
                {
                    // Cloud login if in cloud mode
                    if (enableCloud && _settings.Token == null)
                    {
                        var waiting = await _eft.WriteRequestAsync(new EFTCloudLogonRequest() { ClientID = _settings.Username, Password = _settings.Password, PairingCode = _settings.PairingCode });
                        while (waiting)
                        {
                            var eftResponse = await _eft.ReadResponseAsync(new CancellationTokenSource(45000).Token);
                            if (eftResponse is EFTCloudLogonResponse)
                            {
                                var cloudLogonResponse = eftResponse as EFTCloudLogonResponse;
                                if (!cloudLogonResponse.Success)
                                {
                                    connected = false;
                                    notifyText = $"{cloudLogonResponse.ResponseCode} {cloudLogonResponse.ResponseText}";
                                }
                                waiting = false;
                            }
                        }
                    }
                    else
                    {
                        var waiting = await _eft.WriteRequestAsync(new EFTCloudTokenLogonRequest() { Token = _settings.Token });
                        while (waiting)
                        {
                            var eftResponse = await _eft.ReadResponseAsync(new CancellationTokenSource(45000).Token);
                            if (eftResponse is EFTCloudTokenLogonResponse)
                            {
                                var cloudLogonResponse = eftResponse as EFTCloudTokenLogonResponse;
                                if (!cloudLogonResponse.Success)
                                {
                                    connected = false;
                                    notifyText = $"{cloudLogonResponse.ResponseCode} {cloudLogonResponse.ResponseText}";
                                }
                                waiting = false;
                            }
                        }
                    }
                }

                if (connected)
                {
                    _isServerVerified = true;
                    HideNotification();
                    UpdateSettingsPageUI();

                    // This is only required if we aren't using the PC-EFTPOS dialog
                    await _eft.WriteRequestAsync(new SetDialogRequest() { DialogType = DialogType.Hidden });
                    await _eft.ReadResponseAsync<SetDialogResponse>(new CancellationTokenSource(new TimeSpan(0, 5, 0)).Token); // 
                }
                else
                {
                    _isServerVerified = false;
                    UpdateSettingsPageUI();
                    ShowNotification("ERROR CONTACTING EFT-CLIENT", "", notifyText, NotificationType.Error, true);
                }
                return _isServerVerified;
            }
            catch(Exception ex)
            {
                ShowNotification("ERROR CONTACTING EFT-CLIENT", "", ex.Message, NotificationType.Error, true);
                return false;
            }
        }

      // async void PurchaseToken_Click() // NOT WORKING DELETE
      // {
      //     // Get an auth token 
      //     await RefreshTokenAsync();
      //
      //     ShowNotification("TRANSACTION IN PROGRESS", "", "CHECK PIN PAD FOR STATUS", NotificationType.Normal, false);
      //
      //     // TxnType is required
      //     string txnType = GetTxnType().ToString();
      //     // Set ReferenceNumber to something unique
      //     string txnRef = DateTime.Now.ToString("YYMMddHHmmssfff");
      //     // Set AmountCash for cash out, and AmountPurchase for purchase/refund
      //     int amtPurchase = (txnType == "C") ? 0 : (int)(decimal.Parse(txtAmount.Text) * 100);
      //     int amtCash = (txnType == "C") ? (int)(decimal.Parse(txtAmount.Text) * 100) : 0;
      //
      //     var requestContent = new { Request = new { txnType, amtPurchase, amtCash, txnRef } };
      //     var currentSessionId = Guid.NewGuid().ToString();
      //
      //
      //     var request = new HttpRequestMessage(HttpMethod.Post, $"{basePosApiUri}/sessions/{currentSessionId}/transaction?async=false")
      //     {
      //         Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestContent), System.Text.Encoding.UTF8, "application/json")
      //     };
      //     request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      //     HttpResponseMessage httpResponse = await client.SendAsync(request);
      //     httpResponse.EnsureSuccessStatusCode();
      //
      //     var apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<TransactionResponse>>(await httpResponse.Content.ReadAsStringAsync());
      //     var r = apiResponse.Response;
      //
      //     HideDialog();
      //
      //     ShowNotification(
      //         $"TRANSACTION {(r.Success ? "OK" : "FAILED")}",
      //         $"{r.ResponseCode} {r.ResponseText}",
      //         "",
      //         r.Success ? NotificationType.Success : NotificationType.Error,
      //         true
      //         );
      // }

        #region PC-EFTPOS EVENTS
        void OnTransaction(EFTTransactionResponse r)
        {
            HideDialog();

            ShowNotification(
                $"TRANSACTION {(r.Success ? "OK" : "FAILED")}",
                $"{r.ResponseCode} {r.ResponseText}",
                "",
                r.Success ? NotificationType.Success : NotificationType.Error,
                true
                );
        }

        void OnReceipt(EFTReceiptResponse r)
        {
            // Build receipt
            if (r.IsPrePrint)
            {
                txtReceipt.AppendText($"{Environment.NewLine}{Environment.NewLine}{r.Type.ToString()} receipt{Environment.NewLine}");
            }
            else
            {
                var receipt = new StringBuilder(26 * r.ReceiptText.Length);
                foreach (var l in r.ReceiptText)
                {
                    receipt.AppendLine(l);
                }
                txtReceipt.AppendText(receipt.ToString());
            }
        }

        void OnDisplay(EFTDisplayResponse r)
        {
            ShowDialog((r.DisplayText.Length >= 0) ? r.DisplayText[0] : "", (r.DisplayText.Length >= 1) ? r.DisplayText[1] : "", r.OKKeyFlag, r.CancelKeyFlag, r.AcceptYesKeyFlag, r.DeclineNoKeyFlag, r.AuthoriseKeyFlag);
        }

        void OnTerminated()
        {
            // OnTerminated can be called from a different thread. Always ensure we are in the UI thread.
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnTerminated());
                return;
            }

            _isServerVerified = false;
            NavigateToSettingsPage();
        }
        #endregion

        #region EFT DIALOG
        async void BtnEFTYes_Click(object sender, RoutedEventArgs e)
        {
            await _eft.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.YesAccept });
        }

        async void BtnEFTNo_Click(object sender, RoutedEventArgs e)
        {
            await _eft.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.NoDecline });
        }

        async void BtnEFTOk_Click(object sender, RoutedEventArgs e)
        {
            await _eft.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
        }

        async void BtnEFTCancel_Click(object sender, RoutedEventArgs e)
        {
            await _eft.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
        }

        async void BtnEFTAuth_Click(object sender, RoutedEventArgs e)
        {
            await _eft.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.Authorise, Data = txtEFTDisplayData.Text });
        }

        void ShowDialog(string line1, string line2, bool enableOkKey, bool enableCancelKey, bool enableYesKey, bool enableNoKey, bool enableAuthKey)
        {
            lblEFTDisplayLine1.Text = line1;
            lblEFTDisplayLine2.Text = line2;
            txtEFTDisplayData.Text = "";

            BtnEFTOk.Visibility = enableOkKey ? Visibility.Visible : Visibility.Collapsed;
            BtnEFTCancel.Visibility = enableCancelKey ? Visibility.Visible : Visibility.Collapsed;
            BtnEFTYes.Visibility = enableYesKey ? Visibility.Visible : Visibility.Collapsed;
            BtnEFTNo.Visibility = enableNoKey ? Visibility.Visible : Visibility.Collapsed;
            BtnEFTAuth.Visibility = enableAuthKey ? Visibility.Visible : Visibility.Collapsed;
            txtEFTDisplayData.Visibility = enableAuthKey ? Visibility.Visible : Visibility.Collapsed;

            EFTDialogGrid.Visibility = Visibility.Visible;
            HideNotification();
        }

        void HideDialog()
        {
            EFTDialogGrid.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region UI EVENT HANDLERS
        async void BtnTender_Click(object sender, RoutedEventArgs e)
        {
            {
                // Get an auth token 
                // Basket data
                var basketId = System.Guid.NewGuid().ToString("N");
                var basket = new EFTBasket()
                {
                    Id = basketId,
                    Amount = 100,
                    Items = new System.Collections.Generic.List<EFTBasketItem>()
                    {
                        new EFTBasketItem()
                        {
                            Id = System.Guid.NewGuid().ToString("N"),
                            Amount = 10,
                            Quantity = 5,
                            Name = "Cat food",
                            TagList = new List<string>() { "food", "pet" }
                         },
                        new EFTBasketItem()
                     {
                        Id = System.Guid.NewGuid().ToString("N"),
                        Amount = 25,
                        Name = "Dog food",
                        Quantity = 2
                    },
                     new EFTBasketItemCustom()
                    {
                        Id = System.Guid.NewGuid().ToString("N"),
                        Amount = 1,
                        Name = "Custom",
                        Description = "This is the description of a custom item",
                        Quantity = 1
                    }
                }
             };
                try
                {
                    if (!await _eft.WriteRequestAsync(new EFTBasketDataRequest() { Command = new EFTBasketDataCommandCreate() { Basket = basket } }))
                    {
                        ShowNotification("FAILED TO SEND TXN", "", "", NotificationType.Error, true);
                    }
                    else
                    {
                        try
                        {
                            await _eft.ReadResponseAsync<EFTBasketDataResponse>(new CancellationTokenSource(new TimeSpan(0, 0, 10)).Token);
                        }
                        catch (TaskCanceledException)
                        {
                            // EFT-Client timeout waiting for response
                            ShowNotification("EFT-CLIENT TIMEOUT", null, null, NotificationType.Error, true);
                        }
                        catch (Exception exc)
                        {
                            // TODO: Handle failed EFTBasketDataRequest. Should still continue and attempt the transaction.
                            ShowNotification("FAILED TO SEND TXN", "", exc.Message, NotificationType.Error, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Format the error message so it can properly appear in the notification
                    ex.Message.Replace('\r', ' ');
                    ex.Message.Replace('\n', ' ');
                    ShowNotification("FAILED TO SEND TXN", ex.Message, null, NotificationType.Error, true);
                    return;
                }

                // Transaction request
                var r = new EFTTransactionRequest();
                // TxnType is required
                r.TxnType = GetTxnType();
                // Set ReferenceNumber to something unique
                r.TxnRef = DateTime.Now.ToString("YYMMddHHmmssfff");
                // Set AmountCash for cash out, and AmountPurchase for purchase/refund
                try
                {
                    r.AmtPurchase = (r.TxnType == TransactionType.CashOut) ? 0 : decimal.Parse(txtAmount.Text); //fix crash when amt has a letter in it
                    r.AmtCash = (r.TxnType == TransactionType.CashOut) ? decimal.Parse(txtAmount.Text) : 0;
                    if (r.AmtPurchase < 0 || r.AmtCash < 0)
                    {
                        throw new Exception("Amount Cannot Be Less Than 0");
                    }
                }catch(Exception exc)
                {
                    ShowNotification("FAILED TO SEND TXN", "INVALID AMOUNT", exc.Message, NotificationType.Error, true);
                    return;
                }
                // Set POS or pinpad printer
                r.ReceiptPrintMode = ReceiptPrintModeType.POSPrinter;
                // Set application. Used for gift card & 3rd party payment
                r.Application = TerminalApplication.EFTPOS;
                // Set basket PAD tag
                r.PurchaseAnalysisData.SetTag("SKU", basketId);
                try
                {                 
                    if (!await _eft.WriteRequestAsync(r))
                    {
                        ShowNotification("FAILED TO SEND TXN", "", "", NotificationType.Error, true);
                    }
                    else
                    {
                        // Wait for response
                        var waitingForResponse = true;
                        do
                        {
                            EFTResponse eftResponse = null;
                            try
                            {
                                var timeoutToken = new CancellationTokenSource(new TimeSpan(0, 5, 0)).Token;
                                eftResponse = await _eft.ReadResponseAsync(timeoutToken);

                                // Handle response
                                if (eftResponse is null)
                                {
                                    // Error reading response
                                    waitingForResponse = false;
                                }
                                if (eftResponse is EFTReceiptResponse)
                                {
                                    OnReceipt(eftResponse as EFTReceiptResponse);
                                }
                                else if (eftResponse is EFTDisplayResponse)
                                {
                                    OnDisplay(eftResponse as EFTDisplayResponse);
                                }
                                else if (eftResponse is EFTTransactionResponse)
                                {
                                    waitingForResponse = false;
                                    OnTransaction(eftResponse as EFTTransactionResponse);
                                }

                                //// C#7
                                //switch (eftResponse)
                                //{
                                //    case EFTReceiptResponse resp:
                                //        OnReceipt(resp);
                                //        break;
                                //    case EFTDisplayResponse resp:
                                //        OnDisplay(resp);
                                //        break;
                                //    case EFTTransactionResponse resp:
                                //        waitingForResponse = false;
                                //        OnTransaction(resp);
                                //        break;
                                //    case null:
                                //        // Error reading response
                                //        waitingForResponse = false;
                                //        break;
                                //}
                            }
                            catch (TaskCanceledException)
                            {
                                // EFT-Client timeout waiting for response
                                ShowNotification("EFT-CLIENT TIMEOUT", null, null, NotificationType.Error, true);
                                OnTerminated();
                                waitingForResponse = false;
                            }
                            catch (ConnectionException)
                            {
                                // Socket closed
                                OnTerminated();
                                waitingForResponse = false;
                            }
                            catch (Exception)
                            {
                                // Unhandled exception
                                OnTerminated();
                                waitingForResponse = false;
                            }
                        }
                        while (waitingForResponse);
                    }
                } catch(Exception exc)
                {
                    ShowNotification("FAILED TO SEND TXN", exc.Message, null, NotificationType.Error, true);
                }
            }
        }

        void BtnSettingsBack_Click(object sender, RoutedEventArgs e)
        {
            _settings.Save();
            NavigateToMainPage();
        }

        TransactionType GetTxnType()
        {
            switch (cboType.SelectedIndex)
            {
                case 0: return TransactionType.PurchaseCash;
                case 1: return TransactionType.Refund;
                default: return TransactionType.CashOut;
            }
        }

        void txtServerAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_navigating)
                return;

            _isServerVerified = false;
            UpdateSettingsPageUI();
        }

        void BtnNotificationOk_Click(object sender, RoutedEventArgs e)
        {
            NotificationGrid.Visibility = Visibility.Collapsed;
        }

        async void BtnVerifyServerUri_Click(object sender, RoutedEventArgs e)
        {
            _settings.EFTClientAddress = txtEFTClientAddress.Text;
            _settings.EnableCloud = cboEnableCloud.IsChecked.HasValue && cboEnableCloud.IsChecked.Value;
            _settings.Username = txtUsername.Text;
            _settings.Password = txtPassword.Text;
            _settings.PairingCode = txtPairingCode.Text;
            _settings.Token = txtToken.Text;

            _eft.Disconnect();
            if (await ConnectAsync())
            {
                _settings.Save();
                NavigateToMainPage();
            }
            else
            {
                NavigateToSettingsPage();
            }
        }

        void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSettingsPage();
        }

        private void cboEnableCloud_Checked(object sender, RoutedEventArgs e)
        {
            if (cboEnableCloud.IsChecked.HasValue && cboEnableCloud.IsChecked.Value)
            {
                txtEFTClientAddress.Text = defaultCloudURL;
            }
            else
            {
                txtEFTClientAddress.Text = "127.0.0.1:2011";
            }
            _settings.EFTClientAddress = txtEFTClientAddress.Text;
            _settings.EnableCloud = cboEnableCloud.IsChecked.HasValue && cboEnableCloud.IsChecked.Value;
        }
        #endregion

        #region PAGE NAVIGATION
        void NavigateToMainPage()
        {
            SettingsGrid.Visibility = Visibility.Collapsed;
            MainGrid.Visibility = Visibility.Visible;
        }

        void NavigateToSettingsPage()
        {
            _navigating = true;
            txtEFTClientAddress.Text = _settings.EFTClientAddress;

            SettingsGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Collapsed;

            UpdateSettingsPageUI();
            _navigating = false;
        }
        #endregion

        #region SETTINGS
        void UpdateSettingsPageUI()
        {
            //SettingsGridMainContent.Visibility = _isServerVerified ? Visibility.Visible : Visibility.Collapsed;
            lblEFTClientConnectMessage.Visibility = _isServerVerified ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        # region NOTIFICATIONS
        void ShowNotification(string line1, string line2, string line3, NotificationType notificationType, bool enableOkButton)
        {
            lblNotificationLine1.Text = line1;
            lblNotificationLine2.Text = line2;
            lblNotificationLine3.Text = line3;

            var b = new SolidColorBrush(Colors.Black);
            if (notificationType == NotificationType.Error) b = new SolidColorBrush(Colors.Red);
            if (notificationType == NotificationType.Success) b = new SolidColorBrush(Colors.Green);

            lblNotificationLine1.Foreground = b;
            lblNotificationLine2.Foreground = b;
            lblNotificationLine3.Foreground = b;

            BtnNotificationOk.Visibility = enableOkButton ? Visibility.Visible : Visibility.Collapsed;

            NotificationGrid.Visibility = Visibility.Visible;
        }

        void HideNotification()
        {
            NotificationGrid.Visibility = Visibility.Collapsed;
        }

        #endregion

        private async void BtnTokenLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _settings.Token = txtToken.Text;
                bool connected = await ConnectAsync();
                if (!connected) { throw new Exception("Connection Failure"); }
                else { _settings.Save(); NavigateToMainPage(); }
            }
            catch (Exception ex)
            {
                ShowNotification("Token Login Failed",  ex.Message, null, NotificationType.Error, true);
            }
        }
    }
}
