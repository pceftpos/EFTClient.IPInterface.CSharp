
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		ClientViewModel _vm = new ClientViewModel();

		public MainWindow()
		{
			_vm.Initialize();
			_vm.OnLog += _vm_OnLog;
			_vm.PropertyChanged += _vm_PropertyChanged;

			// Set up some defaults
			_vm.Data.TransactionRequest.TxnType = TransactionType.PurchaseCash;
			_vm.Data.TransactionRequest.AmtPurchase = 1.00M;

			DataContext = _vm;
			InitializeComponent();

			if (_vm.Data.Settings.UseSSL)
			{
				tPassword.Password = _vm.Data.Settings.CloudInfo.Password;
				txtPassword.Password = _vm.Data.Settings.CloudInfo.Password;
			}
		}

		private void _vm_OnLog(string message)
		{
			try
			{
				tbLog.Dispatcher.Invoke(() =>
				{
					tbLog.AppendText(message);
					if (!_vm.Data.SendKeyEnabled && tcUtilities.SelectedIndex == 2)
					{
						tbLog.Focus();
						tbLog.SelectionStart = tbLog.Text.Length;
					}
				});

			}
			catch (Exception ex)
			{
				string m = ex.Message;
			}


		}

		private void _vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ConnectedState")
			{
				UpdateCloudLogonControls();
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			try
			{
				_vm.SaveSettings();
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message);
			}
			finally
			{
				Application.Current.Shutdown();
			}
		}

		private async void btnConnect_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (_vm.Data.ConnectedState == ConnectedStatus.Disconnected)
				{
					if (_vm.Data.Settings.UseSSL)
					{
						await _vm.DoCloudLogon(tPassword.Password);
					}
					else
					{
						await _vm.ConnectAsync();
					}
				}
				else
				{
					_vm.Disconnect();
				}

				UpdateCloudLogonControls();
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message);
			}
		}

		private void chkUseSsl_Checked(object sender, RoutedEventArgs e)
		{
			UpdateConnectionDetails();
			ShowCloudLogonControls();
		}

		private void chkUseSsl_Unchecked(object sender, RoutedEventArgs e)
		{
			UpdateConnectionDetails();
			ShowCloudLogonControls();
		}

		private void UpdateConnectionDetails()
		{
			try
			{
				if (chkUseSsl.IsChecked.Value)
				{
					_vm.Data.Settings.Host = "pos.cloud.pceftpos.com";
					_vm.Data.Settings.Port = 443;
				}
				else
				{
					_vm.Data.Settings.Host = "127.0.0.1";
					_vm.Data.Settings.Port = 2011;
				}

				txtAddress.Text = _vm.Data.Settings.Host;
				txtPort.Text = _vm.Data.Settings.Port.ToString();

				if (rbShowDialogAlways == null)
					//|| rbShowDialogOnEvents == null
					//|| rbHideDialog == null)
					return;

				if (!rbShowDialogAlways.IsChecked.Value)
				{
					if (chkUseSsl.IsChecked.Value)
					{
						rbShowDialogOnEvents.IsChecked = true;
						UpdateDialogSettings();
					}
					else if (!chkUseSsl.IsChecked.Value)
					{
						rbHideDialog.IsChecked = true;
						UpdateDialogSettings();
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message);
			}
		}

		private void UpdateCloudLogonControls()
		{
			try
			{
				if (_vm.Data.ConnectedState == ConnectedStatus.Connected)
				{
					wpCloudControls.Visibility = Visibility.Collapsed;
				}
				else
				{
					if (_vm.Data.Settings.UseSSL)
					{
						wpCloudControls.Visibility = Visibility.Visible;
					}
					else
					{
						wpCloudControls.Visibility = Visibility.Collapsed;
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message);
			}
		}

		private void ShowCloudLogonControls()
		{
			try
			{
				if (chkUseSsl.IsChecked.Value)
				{
					wpCloudControls.Visibility = Visibility.Visible;
				}
				else
				{
					wpCloudControls.Visibility = Visibility.Collapsed;
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message);
			}
		}

		private void tbMenu_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (tbMenu.SelectedIndex == -1)
				return;

			TabItem tab = (TabItem)tbMenu.SelectedItem;
			if (tab != null && tab.Header.Equals("Cloud Logon"))
			{
				if (_vm.Data.Settings.UseSSL)
				{
					txtPassword.Password = _vm.Data.Settings.CloudInfo.Password;
				}
			}

		}

		private async void btnCloudLogon_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				await _vm.DoCloudLogon(tPassword.Password);
			}
			catch
			{
			}
		}

		private void Expander_Collapsed(object sender, RoutedEventArgs e)
		{
			ResizeWindow();
		}

		private void Expander_Expanded(object sender, RoutedEventArgs e)
		{
			ResizeWindow();
		}

		private void ResizeWindow()
		{
			if (exUtilities == null || exSettings == null)
				return;

			if (exUtilities.IsExpanded)
			{
				MinHeight = 1000;
			}
			else if (exSettings.IsExpanded)
			{
				MinHeight = 700;
			}
			else
			{
				MinHeight = 500;
			}

			Height = MinHeight;
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
			ResizeWindow();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (_vm.Data.Settings.DemoDialogOption == DemoDialogMode.AlwaysShow)
			{
				_vm.ShowProxyDialog(true);
			}

			rbShowDialogAlways.IsChecked = (_vm.Data.Settings.DemoDialogOption == DemoDialogMode.AlwaysShow);
			rbShowDialogOnEvents.IsChecked = (_vm.Data.Settings.DemoDialogOption == DemoDialogMode.ShowOnEvents);
			rbHideDialog.IsChecked = (_vm.Data.Settings.DemoDialogOption == DemoDialogMode.Hide) | (!_vm.Data.Settings.UseSSL && _vm.Data.Settings.DemoDialogOption == DemoDialogMode.ShowOnEvents);
		}

		private void DemoDialog_Checked(object sender, RoutedEventArgs e)
		{
			UpdateDialogSettings();
		}

		private void DemoDialog_Unchecked(object sender, RoutedEventArgs e)
		{
			UpdateDialogSettings();
		}

		private void UpdateDialogSettings()
		{
			if (rbShowDialogAlways.IsChecked.Value)
			{
				_vm.Data.Settings.DemoDialogOption = DemoDialogMode.AlwaysShow;
				_vm.ShowProxyDialog(true);
			}
			else if (rbShowDialogOnEvents.IsChecked.Value)
			{
				_vm.Data.Settings.DemoDialogOption = DemoDialogMode.ShowOnEvents;
				_vm.ShowProxyDialog(false);
			}
			else
			{
				_vm.Data.Settings.DemoDialogOption = DemoDialogMode.Hide;
				_vm.ShowProxyDialog(false);
			}

			if (rbShowDialogAlways != null) rbShowDialogAlways.IsChecked = (_vm.Data.Settings.DemoDialogOption == DemoDialogMode.AlwaysShow);
			if (rbShowDialogOnEvents != null) rbShowDialogOnEvents.IsChecked = (_vm.Data.Settings.DemoDialogOption == DemoDialogMode.ShowOnEvents);
			if (rbHideDialog != null) rbHideDialog.IsChecked = (_vm.Data.Settings.DemoDialogOption == DemoDialogMode.Hide);
		}

		private void tcUtilities_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (tcUtilities == null || tbLog == null)
				return;

			if (_vm.Data.Settings.IsLogShown && tcUtilities.SelectedIndex == 2)
			{
				tbLog.Focus();
				tbLog.SelectionStart = tbLog.Text.Length;
			}
		}

	}
}
