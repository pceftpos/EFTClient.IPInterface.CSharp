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

namespace PCEFTPOS.EFTClient.IPInterface.SimpleDemoAsync
{
    [DataContract]
    public class EFTBasketItemCustom: EFTBasketItem
    {
        [DataMember(Name = "customValue")]
        public string CustomItemTest { get; set; } = "This is a test";
    }

    public partial class MainWindow : Window
    {
        IEFTClientIPAsync _eft = null;
        ISettings _settings = null;
        bool _isServerVerified = false;
        bool _navigating = false;

        enum NotificationType { Normal, Error, Success }

        public MainWindow()
        {
            _settings = new Settings();
            _settings.Load();

            InitializeComponent();
        }

        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set up PC-EFTPOS connection
            _eft = new EFTClientIPAsync();

            // Try to connect if we have an address. Either navigate to the main 
            // page (if we are connected) or the settings page (if we aren't)
            var connected = false;
            if (_settings?.EFTClientAddress.Length > 0)
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

        async Task<bool> ConnectAsync()
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

            if(connected && enableCloud)
            {
                // Cloud login if in cloud mode
                if (enableCloud)
                {
                    var waiting = await _eft.WriteRequestAsync(new EFTCloudLogonRequest() { ClientID = _settings.Username, Password = _settings.Password, PairingCode = _settings.PairingCode });
                    while (waiting)
                    {
                        var eftResponse = await _eft.ReadResponseAsync(new CancellationTokenSource(45000).Token);
                        if (eftResponse is EFTCloudLogonResponse)
                        {
                            var cloudLogonResponse = eftResponse as EFTCloudLogonResponse;
                            if(!cloudLogonResponse.Success)
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
                await _eft.WriteRequestAsync(new SetDialogRequest() {DialogType = DialogType.Hidden });
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

            if(!await _eft.WriteRequestAsync(new EFTBasketDataRequest() { Command = new EFTBasketDataCommandCreate() { Basket = basket } }))
            {
                ShowNotification("FAILED TO SEND TXN", "", "", NotificationType.Error, true);
            }
            else
            {
                try
                {
                    //await _eft.ReadResponseAsync<EFTBasketDataResponse>(new CancellationTokenSource(new TimeSpan(0, 0, 10)).Token);
                }
                catch (TaskCanceledException)
                {
                    // EFT-Client timeout waiting for response
                    ShowNotification("EFT-CLIENT TIMEOUT", null, null, NotificationType.Error, true);
                }
                catch (Exception)
                {
                    // TODO: Handle failed EFTBasketDataRequest. Should still continue and attempt the transaction.
                }
            }


            // Transaction request
            var r = new EFTTransactionRequest();
            // TxnType is required
            r.TxnType = GetTxnType();
            // Set ReferenceNumber to something unique
            r.TxnRef = DateTime.Now.ToString("YYMMddHHmmssfff");
            // Set AmountCash for cash out, and AmountPurchase for purchase/refund
            r.AmtPurchase = (r.TxnType == TransactionType.CashOut) ? 0 : decimal.Parse(txtAmount.Text);
            r.AmtCash = (r.TxnType == TransactionType.CashOut) ? decimal.Parse(txtAmount.Text) : 0;
            // Set POS or pinpad printer
            r.ReceiptPrintMode = ReceiptPrintModeType.POSPrinter;
            // Set application. Used for gift card & 3rd party payment
            r.Application = TerminalApplication.EFTPOS;
            // Set basket PAD tag
            r.PurchaseAnalysisData.SetTag("SKU", basketId);

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
                        var timeoutToken = new CancellationTokenSource(new TimeSpan(0,5,0)).Token;
                        eftResponse = await _eft.ReadResponseAsync(timeoutToken);

                        // Handle response
                        if(eftResponse is null)
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
        }

        void BtnSettingsBack_Click(object sender, RoutedEventArgs e)
        {
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
                txtEFTClientAddress.Text = "pos.cloud.pceftpos.com:443";
            }
            else
            {
                txtEFTClientAddress.Text = "127.0.0.1:2011";
            }
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
    }
}
