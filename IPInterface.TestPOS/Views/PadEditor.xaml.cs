
using System.Windows;
using System.IO;

using Newtonsoft.Json;
using System;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    /// <summary>
    /// Interaction logic for PadEditor.xaml
    /// </summary>
    public partial class PadEditor : Window
    {
        public PadViewModel ViewModel = null;

        public PadEditor(string filename, string title = "PAD")
        {
            ViewModel = new PadViewModel(filename);
            DataContext = ViewModel;
            Title = $"Select {title} content";

            InitializeComponent();
            txtMName.Focus();
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
            DialogResult = true;
            Close();
        }

        private void lstPadContent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (lstPadContent == null)
                    return;

                if (lstPadContent.SelectedIndex < 0)
                {
                    btnMAdd.Content = "Add";
                    txtMName.Focus();
                    return;
                }

                var items = lstPadContent.SelectedItem.ToString().Split('|');
                if (items.Length > 0)
                {
                    ViewModel.PadName = items[0].TrimEnd();
                }

                if (items.Length > 1)
                {
                    ViewModel.PadValue = items[1].TrimStart();
                }

                btnMAdd.Content = "New";
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void lstPadEditor_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (lstPadEditor == null )
                    return;

                if (lstPadEditor.SelectedIndex < 0)
                {
                    btnAdd.Content = "Add";
                    txtName.Focus();
                    return;
                }

                var items = lstPadEditor.SelectedItem.ToString().Split('|');
                if (items.Length > 0)
                {
                    ViewModel.PadTagName = items[0].TrimEnd();
                }

                if (items.Length > 1)
                {
                    ViewModel.PadTagValue = items[1].TrimStart();
                }

                btnAdd.Content = "New";
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditMode = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnAdd.Content.Equals("New"))
                {
                    ViewModel.PadTagName = string.Empty;
                    ViewModel.PadTagValue = string.Empty;

                    btnAdd.Content = "Add";
                    lstPadEditor.SelectedIndex = -1;
                }
                else
                {
                    ViewModel.AddPadTagFunc();
                    lstPadEditor.SelectedIndex = -1;
                }

                txtName.Focus();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnMAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnMAdd.Content.Equals("New"))
                {
                    ViewModel.PadName = string.Empty;
                    ViewModel.PadValue = string.Empty;
                    btnMAdd.Content = "Add";
                    lstPadContent.SelectedIndex = -1;
                }
                else
                {
                    ViewModel.AddPadContentFunc();
                    lstPadContent.SelectedIndex = -1;
                }

                txtMName.Focus();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnMUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.UpdatePadContentFunc(lstPadContent.SelectedIndex);
                lstPadContent.Items.Refresh();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.UpdatePadTagFunc(lstPadEditor.SelectedIndex);
                lstPadEditor.Items.Refresh();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.SavePadFieldFunc();
                lstPadContent.Items.Refresh();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }

}
