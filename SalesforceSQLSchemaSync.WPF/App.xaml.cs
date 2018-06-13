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
}