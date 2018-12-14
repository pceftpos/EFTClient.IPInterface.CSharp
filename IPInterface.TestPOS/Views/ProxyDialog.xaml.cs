using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using PCEFTPOS.EFTClient.IPInterface;


namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    /// <summary>
    /// Interaction logic for ProxyDialog.xaml
    /// </summary>
    public partial class ProxyDialog : Window
    {
        public ProxyDialog()
        {
            InitializeComponent();
        }

		private  void Window_Closed(object sender, System.EventArgs e)
        {
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var d = (ViewModels.ProxyViewModel)DataContext;
            if (d != null)
            {
                if (!d.ProxyWindowClosing)
                {
                    d.SendKeyFunc(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
                }
            }
        }
    }
}
