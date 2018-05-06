using IPInterface.TestPOS.Views;
using PCEFTPOS.EFTClient.IPInterface;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace IPInterface.TestPOS.ViewModels
{
	public class DialogUIHandler : IDialogUIHandlerAsync, INotifyPropertyChanged
	{
		public IEFTClientIPAsync EFTClientIPAsync { get; set; }
		private EFTDisplayResponse _displayResponse = new EFTDisplayResponse();
		public event PropertyChangedEventHandler PropertyChanged;
		public TestDialogUI eftDialog = null;
		public bool ProxyWindowClosing = false;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public EFTDisplayResponse DisplayResponse
		{
			get
			{
				return _displayResponse;
			}
			set
			{
				_displayResponse = value;
				OnPropertyChanged(nameof(DisplayResponse));
			}
		}

		public Task HandleCloseDisplayAsync()
		{
			if(eftDialog != null)
			{
			eftDialog.Close();
			eftDialog.BtnOK.Click -= BtnOK_Click;
			eftDialog.BtnCancel.Click -= BtnCancel_Click;
			eftDialog = null;
			}
			return Task.FromResult(0);
		}

		public Task HandleDisplayResponseAsync(EFTDisplayResponse eftDisplayResponse)
		{
			DisplayResponse = eftDisplayResponse;
			if (eftDialog == null)
			{
				eftDialog = new TestDialogUI();
				eftDialog.DataContext = this;
				eftDialog.BtnOK.Click += BtnOK_Click;
				eftDialog.BtnCancel.Click += BtnCancel_Click;
				eftDialog.txtResponseLine1.Text = eftDisplayResponse.DisplayText[0];
				eftDialog.txtResponseLine2.Text = eftDisplayResponse.DisplayText[1];
				eftDialog.Show();
			}
			loadButtons(eftDisplayResponse);
			if (eftDisplayResponse.InputType != InputType.None)
			{
				eftDialog.txtInput.Visibility = System.Windows.Visibility.Visible;
			}
			return Task.FromResult(0);
		}

		private void BtnOK_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			switch (eftDialog.BtnOK.Content)
			{
				case ("OK"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;
				case ("Authorise"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.Authorise, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;
				case ("Yes"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.YesAccept, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;
				default:
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;

			}
		}

		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			switch (eftDialog.BtnCancel.Content)
			{
				case ("Cancel"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
					break;
				case ("Decline"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.NoDecline });
					break;
				default:
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
					break;

			}
		}

		private void loadButtons(EFTDisplayResponse eftDisplayResponse)
		{
			#region OKButtons
			if (eftDisplayResponse.AcceptYesKeyFlag == true)
			{
				eftDialog.BtnOK.Content = "Yes";
				eftDialog.BtnOK.Visibility = System.Windows.Visibility.Visible;
			}
			else if (eftDisplayResponse.AuthoriseKeyFlag == true)
			{
				eftDialog.BtnOK.Content = "Authorise";
				eftDialog.BtnOK.Visibility = System.Windows.Visibility.Visible;
			}
			else if (eftDisplayResponse.OKKeyFlag == true)
			{
				eftDialog.BtnOK.Content = "OK";
				eftDialog.BtnOK.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				eftDialog.BtnOK.Visibility = System.Windows.Visibility.Collapsed;
			}
			#endregion

			#region CancelButtons
			if (eftDisplayResponse.CancelKeyFlag == true)
			{
				eftDialog.BtnCancel.Content = "Cancel";
				eftDialog.BtnCancel.Visibility = System.Windows.Visibility.Visible;
			}
			else if (eftDisplayResponse.DeclineNoKeyFlag == true)
			{
				eftDialog.BtnCancel.Content = "No";
				eftDialog.BtnCancel.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				eftDialog.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
			}
			#endregion
		}
	}
}
