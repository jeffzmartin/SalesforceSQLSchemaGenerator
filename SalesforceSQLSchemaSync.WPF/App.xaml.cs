using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;

namespace SalesforceSQLSchemaSync.WPF {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		[STAThread]
		public static void Main(string[] args) {
			App app = new App();
			app.Run();
		}

		public App() {
			MainWindow window = new MainWindow();
			window.Show();
		}
	}

	internal class SettingsManager {
		public static string GetValue(string propertyName) {
			return (string)Properties.Settings.Default[propertyName];
		}
		public static void SetValue(string propertyName, string value) {
			Properties.Settings.Default[propertyName] = value;
			Properties.Settings.Default.Save();
		}
		public static string GetSecureValue(string propertyName) {
			string value = (string)Properties.Settings.Default[propertyName];
			if(!string.IsNullOrEmpty(value)) {
				return Encryption.DecryptString(Encryption.ToSecureString(value));
			}
			else {
				return null;
			}
		}
		public static void SetSecureValue(string propertyName, string value) {
			Properties.Settings.Default[propertyName] = Encryption.EncryptString(Encryption.ToSecureString(value));
			Properties.Settings.Default.Save();
		}
	}
}
