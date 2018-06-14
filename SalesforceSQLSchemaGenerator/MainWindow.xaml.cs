using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
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

namespace SalesforceSQLSchemaGenerator {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged {

		private string statusText = null;
		public string StatusText { 
			get {
				return statusText;
			}
			set {
				statusText = value;
				OnPropertyChanged("StatusText");
			}
		}

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
		private Visibility generateScriptVisibility = Visibility.Hidden;
		public Visibility GenerateScriptVisibility {
			get {
				return generateScriptVisibility;
			}
			set {
				generateScriptVisibility = value;
				OnPropertyChanged("GenerateScriptVisibility");
			}
		}
		private Visibility saveScriptVisibility = Visibility.Hidden;
		public Visibility SaveScriptVisibility {
			get {
				return saveScriptVisibility;
			}
			set {
				saveScriptVisibility = value;
				OnPropertyChanged("SaveScriptVisibility");
			}
		}

		public Thickness DefaultMargin { get; set; }
		private Dictionary<string, string> GeneratedSqlScript = new Dictionary<string, string>();

		private SalesforceApi salesforceAPI = null;

		public MainWindow() {
			//set defaults
			StatusText = "Initializing...";
			SaveScriptVisibility = Visibility.Hidden;
			GenerateScriptVisibility = Visibility.Hidden;
			DefaultMargin = new Thickness(2, 2, 2, 2);

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
			StatusText = "Ready";
		}

		#region INotifyPropertyChanged implementation
		// Basically, the UI thread subscribes to this event and update the binding if the received Property Name correspond to the Binding Path element
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		private void LoadSqlSyntaxHighlightRules() {
			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SalesforceSQLSchemaGenerator.AvalonEdit.SQLSyntax.xhsd");
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
				StatusText = string.Format("Connecting to {0}...", SalesforceUrl);
				salesforceAPI = new SalesforceApi(SalesforceUrl, SalesforceUsername, SalesforcePassword, SalesforceToken);
				StatusText = string.Format("Retrieving object listing...", SalesforceUrl);
				List<string> sfObjects = salesforceAPI.GetObjectNames();
				SalesforceObjects.Clear();
				foreach (string sObjectName in sfObjects) {
					SalesforceObjects.Add(new CheckedListItem(sObjectName));
				}
				if (SalesforceObjects.Count > 0) {
					GenerateScriptVisibility = Visibility.Visible;
				}
				StatusText = string.Format("Discovered {0} objects", SalesforceObjects.Count);
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		public void SelectAllObjects_Click(object sender, RoutedEventArgs e) {
			foreach(CheckedListItem i in SalesforceObjects) {
				i.IsChecked = true;
			}
		}
		public void UnselectAllObjects_Click(object sender, RoutedEventArgs e) {
			foreach (CheckedListItem i in SalesforceObjects) {
				i.IsChecked = false;
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
			System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog {
				OverwritePrompt = true
			};
			if (!string.IsNullOrWhiteSpace(SqlSaveDirectory)) {
				dialog.InitialDirectory = SqlSaveDirectory;
			}
			else {
				dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
			dialog.DefaultExt = ".sql";
			dialog.Filter = "SQL Script (*.sql)|*.sql|All Files (*.*)|*.*";
			if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				StatusText = string.Format("Saving {0}...", dialog.FileName);
				StreamWriter sw = new StreamWriter(dialog.OpenFile());
				foreach(string sql in GeneratedSqlScript.Values) {
					sw.Write(sql);
				}
				sw.Close();
				sw.Dispose();
				FileInfo fileInfo = new FileInfo(dialog.FileName);
				SqlSaveDirectory = fileInfo.DirectoryName;
				SettingsManager.SetString("SqlSaveDirectory", SqlSaveDirectory);
				StatusText = string.Format("Saved {0}", dialog.FileName);
			}
		}
		private void SaveAsMultipleFiles(object sender, RoutedEventArgs e) {
			System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
			if (!string.IsNullOrWhiteSpace(SqlSaveDirectory)) {
				dialog.SelectedPath = SqlSaveDirectory;
			}
			else {
				dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
			dialog.ShowNewFolderButton = true;
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {

				SqlSaveDirectory = dialog.SelectedPath;
				SettingsManager.SetString("SqlSaveDirectory", SqlSaveDirectory);

				Parallel.ForEach(GeneratedSqlScript, (i) => {
					string filePath = string.Format("{0}\\{1}.sql", SqlSaveDirectory, i.Key);
					try {
						lock(statusText) {
							StatusText = string.Format("Saving {0}...", filePath);
						}
						File.WriteAllText(filePath, i.Value);
					}
					catch(Exception ex) {
						MessageBox.Show(string.Format("Unable to save file {0}.\n{1}", filePath, ex.Message));
					}
				});
				StatusText = string.Format("Saved {0} files to {1}", GeneratedSqlScript.Count, SqlSaveDirectory);
			}
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

			StatusText = string.Format("Getting details for {0} objects from Salesforce...", tableNames.Count);
			List<DescribeSObjectResult> sObjects = salesforceAPI.GetSObjectDetails(tableNames);
			StatusText = string.Format("Generating SQL script for {0} objects...", sObjects.Count);
			string schema = null;
			if (!string.IsNullOrWhiteSpace(SqlSchemaName)) {
				schema = string.Format("[{0}].", SqlSchemaName);
			}
			foreach (DescribeSObjectResult t in sObjects) {
				StringBuilder sb = new StringBuilder();
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
						case "double":
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
					if(string.Equals(f.name, "id", StringComparison.InvariantCultureIgnoreCase)) {
						primaryKeys.Add(f.name);
					}
				}
				if(primaryKeys.Count > 0) {
					sb.AppendLine(string.Format("	,PRIMARY KEY ([{0}])", string.Join("], [", primaryKeys)));
				}
				sb.Append(");");

				output.Add(t.name, sb.ToString());
			}
			StatusText = string.Format("Generated SQL script for {0} objects", sObjects.Count);

			return output;
		}

		private void GenerateSqlScript_Click(object sender, RoutedEventArgs e) {
			StringBuilder sb = new StringBuilder();
			GeneratedSqlScript.Clear();
			GeneratedSqlScript = GenerateSqlScript();
			bool isFirst = true;
			foreach (string sql in GeneratedSqlScript.Values) {
				if (!isFirst) {
					sb.AppendLine().AppendLine();
				}
				else {
					isFirst = false;
				}
				sb.AppendLine(sql);
			}
			SqlOutputDocument.Text = sb.ToString();
			SaveScriptVisibility = Visibility.Visible;
		}
		#endregion
	}
}