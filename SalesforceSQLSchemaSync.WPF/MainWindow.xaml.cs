using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SalesforceSQLSchemaSync.WPF {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			SalesforceRememberConnection = false;

			if (!string.IsNullOrEmpty(SettingsManager.GetValue("SalesforceUrl"))) {
				SalesforceUrl = SettingsManager.GetValue("SalesforceUrl");
				SalesforceRememberConnection = true;
			}
			if (!string.IsNullOrEmpty(SettingsManager.GetValue("SalesforceUsername"))) {
				SalesforceUsername = SettingsManager.GetSecureValue("SalesforceUsername");
				SalesforceRememberConnection = true;
			}
			if (!string.IsNullOrEmpty(SettingsManager.GetValue("SalesforcePassword"))) {
				SalesforcePassword = SettingsManager.GetSecureValue("SalesforcePassword");
				SalesforceRememberConnection = true;
			}
			if (!string.IsNullOrEmpty(SettingsManager.GetValue("SalesforceToken"))) {
				SalesforceToken = SettingsManager.GetSecureValue("SalesforceToken");
				SalesforceRememberConnection = true;
			}

			DataContext = this;
			SalesforceObjects = new ObservableCollection<CheckedListItem>();

			InitializeComponent();

			if(!string.IsNullOrWhiteSpace(SalesforcePassword)) {
				SalesforcePasswordBox.Password = SalesforcePassword;
			}
		}

		public string SalesforceUrl { get; set; }
		public string SalesforceUsername { get; set; }
		private string SalesforcePassword = null;
		public string SalesforceToken { get; set; }
		public bool SalesforceRememberConnection { get; set; }

		public ObservableCollection<CheckedListItem> SalesforceObjects { get; set; }

		private SalesforceAPI salesforceAPI = null;

		private void SaveSalesforceConnctionInfo(object sender, RoutedEventArgs e) {
			SaveSalesforceConnctionInfo();
		}
		private void SaveSalesforceConnctionInfo() {
			if (SalesforceRememberConnection) {
				SettingsManager.SetValue("SalesforceUrl", SalesforceUrl);
				SettingsManager.SetSecureValue("SalesforceUsername", SalesforceUsername);
				SettingsManager.SetSecureValue("SalesforcePassword", SalesforcePassword);
				SettingsManager.SetSecureValue("SalesforceToken", SalesforceToken);
			}
			else {
				SettingsManager.SetValue("SalesforceUrl", String.Empty);
				SettingsManager.SetSecureValue("SalesforceUsername", String.Empty);
				SettingsManager.SetSecureValue("SalesforcePassword", String.Empty);
				SettingsManager.SetSecureValue("SalesforceToken", String.Empty);
			}
		}

		private void SalesforcePassword_PasswordChanged(object sender, RoutedEventArgs e) {
			SalesforcePassword = ((PasswordBox)sender).Password;
			SaveSalesforceConnctionInfo();
		}

		private void SalesforceConnect_Click(object sender, RoutedEventArgs e) {
			if(SalesforceRememberConnection) {
				SettingsManager.SetValue("SalesforceUrl", SalesforceUrl);
				SettingsManager.SetSecureValue("SalesforceUsername", SalesforceUsername);
				SettingsManager.SetSecureValue("SalesforcePassword", SalesforcePassword);
				SettingsManager.SetSecureValue("SalesforceToken", SalesforceToken);
			}
			
			try {
				salesforceAPI = new SalesforceAPI(SalesforceUrl, SalesforceUsername, SalesforcePassword, SalesforceToken);
				List<string> sfObjects = salesforceAPI.GetObjectNames();
				SalesforceObjects.Clear();
				foreach (string sObjectName in sfObjects) {
					SalesforceObjects.Add(new CheckedListItem(sObjectName));
				}
			}
			catch(SalesforceMagic.Exceptions.SalesforceRequestException ex) {
				MessageBox.Show(ex.Message);
			}
			catch {
				throw;
			}
		}
	}

	public class CheckedListItem {
		public CheckedListItem(string value, string label) {
			Value = value;
			Label = label;
		}
		public CheckedListItem(string value) {
			Value = value;
			Label = value.Replace("_","__");
		}

		public string Value { get; set; }
		public string Label { get; set; }
		public bool IsChecked { get; set; }
	}
}
