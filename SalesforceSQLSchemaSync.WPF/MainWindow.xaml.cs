using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using SFDC.soapApi;

namespace SalesforceSQLSchemaSync.WPF {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		//private bool IsInitialized = false;

		public string SalesforceUrl { get; set; }
		public string SalesforceUsername { get; set; }
		private string SalesforcePassword = null;
		public string SalesforceToken { get; set; }
		public bool SalesforceRememberConnection { get; set; }

		public string SqlSchemaName { get; set; }
		public bool SqlTextUnicode { get; set; }
		public int? SqlVarcharMaxMinimumThreshold { get; set; }
		private string SqlSaveDirectory { get; set; }

		public ObservableCollection<CheckedListItem> SalesforceObjects { get; set; }

		public TextDocument SqlOutputDocument { get; set; }
		public bool HasSqlOutput { get; set; }
		private Dictionary<string, string> GeneratedSqlScript = new Dictionary<string, string>();

		private SalesforceApi salesforceAPI = null;

		public MainWindow() {
			//set defaults
			HasSqlOutput = false;

			//retrieve values from settings
			SalesforceRememberConnection = SettingsManager.GetValue<bool>("SalesforceRememberConnection");
			if(SalesforceRememberConnection) {
				SalesforceUrl = SettingsManager.GetString("SalesforceUrl");
				SalesforceUsername = SettingsManager.GetSecureString("SalesforceUsername");
				SalesforcePassword = SettingsManager.GetSecureString("SalesforcePassword");
				SalesforceToken = SettingsManager.GetSecureString("SalesforceToken");
			}
			SqlSchemaName = SettingsManager.GetString("SqlSchemaName");
			SqlTextUnicode = SettingsManager.GetValue<bool>("SqlTextUnicode");
			SqlVarcharMaxMinimumThreshold = SettingsManager.GetNullableValue<int>("SqlVarcharMaxMinimumThreshold"); /* SqlVarcharMaxMinimumThreshold = 4000; //4000 is the default min for N/VARCHAR(MAX) */
			SqlSaveDirectory = SettingsManager.GetString("SqlSaveDirectory");

			DataContext = this;

			SalesforceObjects = new ObservableCollection<CheckedListItem>();
			SqlOutputDocument = new TextDocument();

			LoadSqlSyntaxHighlightRules();

			InitializeComponent();

			if(!string.IsNullOrWhiteSpace(SalesforcePassword)) {
				SalesforcePasswordBox.Password = SalesforcePassword;
			}

			//IsInitialized = true;
		}

		private void LoadSqlSyntaxHighlightRules() {
			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SalesforceSQLSchemaSync.WPF.AvalonEdit.SQLSyntax.xhsd");
			XmlTextReader reader = new XmlTextReader(stream);
			IHighlightingDefinition highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
			HighlightingManager.Instance.RegisterHighlighting(highlightingDefinition.Name, new string[] { "*.sql", "*.txt" }, highlightingDefinition);
			reader.Close();
			reader.Dispose();
			stream.Close();
			stream.Dispose();
		}

		private void SaveSalesforceConnctionInfo(object sender, RoutedEventArgs e) {
			SaveSalesforceConnctionInfo();
		}
		private void SaveSalesforceConnctionInfo() {
			if(IsInitialized) {
				SettingsManager.SetValue("SalesforceRememberConnection", SalesforceRememberConnection);
				if (SalesforceRememberConnection) {
					SettingsManager.SetValue("SalesforceUrl", SalesforceUrl);
					SettingsManager.SetSecureString("SalesforceUsername", SalesforceUsername);
					SettingsManager.SetSecureString("SalesforcePassword", SalesforcePassword);
					SettingsManager.SetSecureString("SalesforceToken", SalesforceToken);
				}
				else {
					SettingsManager.SetValue("SalesforceUrl", String.Empty);
					SettingsManager.SetSecureString("SalesforceUsername", String.Empty);
					SettingsManager.SetSecureString("SalesforcePassword", String.Empty);
					SettingsManager.SetSecureString("SalesforceToken", String.Empty);
				}
			}
		}

		private void SalesforcePassword_PasswordChanged(object sender, RoutedEventArgs e) {
			SalesforcePassword = ((PasswordBox)sender).Password;
			SaveSalesforceConnctionInfo();
		}

		private void SalesforceConnect_Click(object sender, RoutedEventArgs e) {
			SaveSalesforceConnctionInfo();

			try {
				salesforceAPI = new SalesforceApi(SalesforceUrl, SalesforceUsername, SalesforcePassword, SalesforceToken);
				List<string> sfObjects = salesforceAPI.GetObjectNames();
				SalesforceObjects.Clear();
				foreach (string sObjectName in sfObjects) {
					SalesforceObjects.Add(new CheckedListItem(sObjectName));
				}
			}
			catch {
				throw;
			}
		}

		private void SaveSqlInfo(object sender, RoutedEventArgs e) {
			SaveSqlInfo();
		}
		private void SaveSqlInfo() {
			if (IsInitialized) {
				SettingsManager.SetString("SqlSchemaName", SqlSchemaName);
				SettingsManager.SetValue("SqlTextUnicode", SqlTextUnicode);
				SettingsManager.SetNullableValue("SqlVarcharMaxMinimumThreshold", SqlVarcharMaxMinimumThreshold);
			}
		}

		private void SaveAsSingleFile(object sender, RoutedEventArgs e) {
			Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
			dialog.OverwritePrompt = true;
			if (SqlSaveDirectory != null) {
				dialog.InitialDirectory = SqlSaveDirectory;
			}
			else {
				dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
			dialog.DefaultExt = ".sql";
			dialog.Filter = "SQL Script (*.sql)|*.sql|All Files (*.*)|*.*";
			if(dialog.ShowDialog() == true) {
				StreamWriter sw = new StreamWriter(dialog.OpenFile());
				foreach(string sql in GeneratedSqlScript.Values) {
					sw.Write(sql);
				}
				sw.Close();
				sw.Dispose();
				FileInfo fileInfo = new FileInfo(dialog.FileName);
				SqlSaveDirectory = fileInfo.DirectoryName;
			}
			SettingsManager.SetString("SqlSaveDirectory", SqlSaveDirectory);
		}
		private void SaveAsMultipleFiles(object sender, RoutedEventArgs e) {
		}


		#region GenerateSqlScript(); GenerateSqlScript_Click();
		public Dictionary<string,string> GenerateSqlScript() {
			Dictionary<string, string> output = new Dictionary<string, string>();

			List<string> tableNames = new List<string>();
			foreach (CheckedListItem i in SalesforceObjects) {
				if (i.IsChecked) {
					tableNames.Add(i.Value);
				}
			}

			bool isFirst = true;
			DescribeSObjectResult[] sObjects = salesforceAPI.GetSObjectDetails(tableNames.ToArray());
			string schema = null;
			if (!string.IsNullOrWhiteSpace(SqlSchemaName)) {
				schema = string.Format("[{0}].", SqlSchemaName);
			}
			foreach (DescribeSObjectResult t in sObjects) {
				StringBuilder sb = new StringBuilder();

				if (!isFirst) {
					sb.AppendLine().AppendLine();
				}
				else {
					isFirst = false;
				}
				sb.AppendLine(string.Format("CREATE TABLE {0}[{1}] (", schema, t.name));
				List<string> primaryKeys = new List<string>();
				bool firstRow = true;
				foreach (Field f in t.fields) {
					sb.Append("	"); //indent
					if (!firstRow) {
						sb.Append(",");
					}
					firstRow = false;
					/*
					 *  https://learn.capstorm.com/copystorm/frequently-asked-questions/how-does-copystorm-work/how-do-salesforce-types-map-to-database-column-types/
					 */
					switch (f.type.ToString()) {
						case "currency":
						case "decimal":
						case "percent":
							sb.Append(string.Format("[{0}] DECIMAL({1},{2})", f.name, f.precision, f.scale));
							break;
						case "address":
						case "email":
						case "id":
						case "location":
						case "multipicklist":
						case "phone":
						case "picklist":
						case "string":
						case "url":
						case "textarea":
							if (SqlVarcharMaxMinimumThreshold != null && f.length >= SqlVarcharMaxMinimumThreshold.Value) {
								sb.Append(string.Format("[{0}] {1}(MAX)", f.name, (SqlTextUnicode ? "NVARCHAR" : "VARCHAR")));
							}
							else {
								sb.Append(string.Format("[{0}] {1}({2})", f.name, (SqlTextUnicode ? "NVARCHAR" : "VARCHAR"), f.length));
							}
							break;
						case "reference":
							sb.Append(string.Format("[{0}] {1}({2})", f.name, (SqlTextUnicode ? "NCHAR" : "CHAR"), f.length));
							break;
						case "anyType":
							sb.Append(string.Format("[{0}] TEXT", f.name));
							break;
						case "boolean":
							sb.Append(string.Format("[{0}] BIT", f.name));
							break;
						case "blob":
							sb.Append(string.Format("[{0}] BINARY", f.name));
							break;
						case "long":
							sb.Append(string.Format("[{0}] BIGINT", f.name));
							break;
						case "double":
						case "date":
						case "datetime":
						case "int":
						case "time":
							sb.Append(string.Format("[{0}] {1}", f.name, f.type.ToString().ToUpper()));
							break;
						default:
							sb.Append(string.Format("[{0}] {1}", f.name, f.type));
							break;
					}
					if(!f.nillable) {
						sb.Append(" NOT NULL");
					}
					sb.AppendLine();
					if(f.idLookup) {
						primaryKeys.Add(f.name);
					}
				}
				if(primaryKeys.Count > 0) {
					sb.AppendLine(string.Format("	,PRIMARY KEY ({0})", string.Join(",", primaryKeys)));
				}
				sb.Append(");");

				output.Add(t.name, sb.ToString());
			}

			return output;
		}

		private void GenerateSqlScript_Click(object sender, RoutedEventArgs e) {
			StringBuilder sb = new StringBuilder();
			GeneratedSqlScript.Clear();
			GeneratedSqlScript = GenerateSqlScript();
			foreach (string sql in GeneratedSqlScript.Values) {
				sb.AppendLine(sql);
			}
			SqlOutputDocument.Text = sb.ToString();
		}
		#endregion
	}
}