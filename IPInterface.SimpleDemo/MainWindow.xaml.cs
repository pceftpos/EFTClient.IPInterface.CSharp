using PCEFTPOS.EFTClient.IPInterface;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PCEFTPOS.EFTClient.IPInterface.SimpleDemo
{
    public partial class MainWindow : Window
    {
        IEFTClientIP _eft = null;
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

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set up PC-EFTPOS connection and hook up events
            _eft = new EFTClientIP();
            _eft.OnDisplay += _eft_OnDisplay;
            _eft.OnReceipt += _eft_OnReceipt;
            _eft.OnTransaction += _eft_OnTransaction;
            _eft.OnTerminated += _eft_OnTerminated;

            // Try to connect if we have an address. Either navigate to the main 
            // page (if we are connected) or the settings page (if we aren't)
            var connected = false;
            if (_settings?.EFTClientAddress.Length > 0)
            {
                connected = Connect();
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

        bool Connect()
        {
            var tmpPort = 0;
            var addr = _settings.EFTClientAddress.Split(new char[] { ':' });
            if(addr.Length < 2 || int.TryParse(addr[1], out tmpPort) == false)
            {
                ShowNotification("INVALID ADDRESS", "", "", NotificationType.Error, true);
                return false;
            }
            _eft.HostName = addr[0];
            _eft.HostPort = tmpPort;

            ShowNotification("CONTACTING EFT-CLIENT, PLEASE WAIT...", "", "", NotificationType.Normal, false);
            var connected = _eft.Connect();

            if (connected)
            {
                _isServerVerified = true;
                HideNotification();
                UpdateSettingsPageUI();

                // This is only required if we aren't using the PC-EFTPOS dialog
                _eft.DoHideDialogs();
            }
            else
            {
                _isServerVerified = false;
                UpdateSettingsPageUI();
                ShowNotification("ERROR CONTACTING EFT-CLIENT", "", "", NotificationType.Error, true);
            }
            return _isServerVerified;
        }

        #region PC-EFTPOS EVENTS
        void _eft_OnTransaction(object sender, EFTEventArgs<EFTTransactionResponse> e)
        {
            HideDialog();

            var r = e.Response;
            ShowNotification(
                $"TRANSACTION {(r.Success ? "OK" : "FAILED")}",
                $"{r.ResponseCode} {r.ResponseText}",
                "",
                r.Success ? NotificationType.Success : NotificationType.Error,
                true
                );
        }

        void _eft_OnReceipt(object sender, EFTEventArgs<EFTReceiptResponse> e)
        {
            // Build receipt
            if(e.Response.IsPrePrint)
            {
                txtReceipt.AppendText($"{Environment.NewLine}{Environment.NewLine}{e.Response.Type.ToString()} receipt{Environment.NewLine}");
            }
            else
            {
                var receipt = new StringBuilder(26 * e.Response.ReceiptText.Length);
                foreach(var l in e.Response.ReceiptText)
                {
                    receipt.AppendLine(l);
                }
                txtReceipt.AppendText(receipt.ToString());
            }
        }

        void _eft_OnDisplay(object sender, EFTEventArgs<EFTDisplayResponse> e)
        {
            var r = e.Response;
            ShowDialog((r.DisplayText.Length >= 0) ? r.DisplayText[0] : "", (r.DisplayText.Length >= 1) ? r.DisplayText[1] : "", r.OKKeyFlag, r.CancelKeyFlag, r.AcceptYesKeyFlag, r.DeclineNoKeyFlag, r.AuthoriseKeyFlag);
        }

        void _eft_OnTerminated(object sender, SocketEventArgs e)
        {
            // OnTerminated can be called from a different thread. Always ensure we are in the UI thread.
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => _eft_OnTerminated(sender, e));
                return;
            }

            _isServerVerified = false;
            NavigateToSettingsPage();
        }
        #endregion

        #region EFT DIALOG
        void BtnEFTYes_Click(object sender, RoutedEventArgs e)
        {
            _eft.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.YesAccept });
        }

        void BtnEFTNo_Click(object sender, RoutedEventArgs e)
        {
            _eft.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.NoDecline });
        }

        void BtnEFTOk_Click(object sender, RoutedEventArgs e)
        {
            _eft.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
        }

        void BtnEFTAuth_Click(object sender, RoutedEventArgs e)
        {
            _eft.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.Authorise, Data = txtEFTDisplayData.Text });
        }

        void BtnEFTCancel_Click(object sender, RoutedEventArgs e)
        {
            _eft.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
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
        void BtnTender_Click(object sender, RoutedEventArgs e)
        {
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

            if (!_eft.DoTransaction(r))
            {
                ShowNotification("FAILED TO SEND TXN", "", "", NotificationType.Error, true);
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

        void BtnVerifyServerUri_Click(object sender, RoutedEventArgs e)
        {
            _settings.EFTClientAddress = txtEFTClientAddress.Text;
            _eft.Disconnect();
            if(Connect())
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

        private void BtnEFTAuth_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
