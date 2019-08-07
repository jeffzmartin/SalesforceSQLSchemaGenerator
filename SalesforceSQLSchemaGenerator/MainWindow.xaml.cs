using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
		private bool loaded = false;

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
		private string statusTextLeft = null;
		public string StatusTextLeft {
			get {
				return statusTextLeft;
			}
			set {
				statusTextLeft = value;
				OnPropertyChanged("StatusTextLeft");
			}
		}
		private string filterText = null;
		public string FilterText {
			get {
				return filterText;
			}
			set {
				filterText = value;
				OnPropertyChanged("FilterText");
			}
		}

		public string SalesforceUrl { get; set; }
		public string SalesforceUsername { get; set; }
		private string SalesforcePassword = null;
		public string SalesforceToken { get; set; }
		public bool SalesforceRememberSelectedObjects { get; set; }
		private StringCollection SalesforceSelectedObjects { get; set; }
		public bool SalesforceRememberConnection { get; set; }

		public string SqlSchemaName { get; set; }
		public bool SqlTextUnicode { get; set; }
		public int? SqlVarcharMaxMinimumThreshold { get; set; }
		private string SqlSaveDirectory { get; set; }
		private bool sqlGenerateForeignKeys = true;
		public bool SqlGenerateForeignKeys {
			get {
				return sqlGenerateForeignKeys;
			}
			set {
				sqlGenerateForeignKeys = value;
				OnPropertyChanged("SqlGenerateForeignKeys");
			}
		}
		public bool SqlGenerateForeignKeysSelectedObjectsOnly { get; set; }

		public ObservableCheckedListItemCollection SalesforceObjects { get; set; }

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

		private string[] AllowedNullableFieldNames = new string[] {
			"Id",
			"CreatedById",
			"CreatedDate",
			"LastModifiedById",
			"LastModifiedDate",
			"SystemModstamp",
			"IsDeleted",
			"Name",
			"OwnerId"
		};

		public Thickness DefaultMargin { get; set; }
		private Dictionary<string, string> GeneratedSqlScript = new Dictionary<string, string>();

		private SalesforceApi salesforceAPI = null;

		#region Constructor
		public MainWindow() {
			//set defaults
			StatusText = "Initializing...";
			SaveScriptVisibility = Visibility.Hidden;
			GenerateScriptVisibility = Visibility.Hidden;
			DefaultMargin = new Thickness(2, 2, 2, 2);
			SalesforceSelectedObjects = new StringCollection();

			//retrieve values from settings
			SalesforceRememberConnection = SettingsManager.GetValue<bool>("SalesforceRememberConnection");
			if(SalesforceRememberConnection) {
				SalesforceUrl = SettingsManager.GetString("SalesforceUrl");
				SalesforceUsername = SettingsManager.GetSecureString("SalesforceUsername");
				SalesforcePassword = SettingsManager.GetSecureString("SalesforcePassword");
				SalesforceToken = SettingsManager.GetSecureString("SalesforceToken");
			}
			SalesforceRememberSelectedObjects = SettingsManager.GetValue<bool>("SalesforceRememberSelectedObjects");
			SqlGenerateForeignKeysSelectedObjectsOnly = SettingsManager.GetValue<bool>("SqlGenerateForeignKeysSelectedObjectsOnly");
			if (SalesforceRememberSelectedObjects && SettingsManager.GetValue<StringCollection>("SalesforceSelectedObjects") != null) {
				SalesforceSelectedObjects = SettingsManager.GetValue<StringCollection>("SalesforceSelectedObjects");
			}
			SqlSchemaName = SettingsManager.GetString("SqlSchemaName");
			SqlTextUnicode = SettingsManager.GetValue<bool>("SqlTextUnicode");
			SqlVarcharMaxMinimumThreshold = SettingsManager.GetNullableValue<int>("SqlVarcharMaxMinimumThreshold"); /* SqlVarcharMaxMinimumThreshold = 4000; //4000 is the default min for N/VARCHAR(MAX) */
			SqlSaveDirectory = SettingsManager.GetString("SqlSaveDirectory");
			SqlGenerateForeignKeys = SettingsManager.GetValue<bool>("SqlGenerateForeignKeys");

			DataContext = this;

			SalesforceObjects = new ObservableCheckedListItemCollection();
			SalesforceObjects.ItemCheckedChanged += SalesforceObjects_ItemCheckedChanged;
			SalesforceObjects.CollectionChanged += SalesforceObjects_CollectionChanged;
			SqlOutputDocument = new TextDocument();

			LoadSqlSyntaxHighlightRules();

			InitializeComponent();

			if(!string.IsNullOrWhiteSpace(SalesforcePassword)) {
				SalesforcePasswordBox.Password = SalesforcePassword;
			}
			StatusText = "Ready";

			loaded = true;
		}

		#endregion

		#region INotifyPropertyChanged - PropertyChanged; OnPropertyChanged(); 
		// Basically, the UI thread subscribes to this event and update the binding if the received Property Name correspond to the Binding Path element
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		#region LoadSqlSyntaxHighlightRules();
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
		#endregion

		#region SalesforceObjects_CollectionChanged(); SalesforceObjects_ItemCheckedChanged(); SalesforcePassword_PasswordChanged();
		private void SalesforceObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			StatusTextLeft = string.Format("{0} of {1} objects selected", SalesforceSelectedObjects.Count, SalesforceObjects.Count);
		}

		private void SalesforceObjects_ItemCheckedChanged(ObservableCheckedListCheckedChanged e) {
			if(e.IsChecked && !SalesforceSelectedObjects.Contains(e.Value)) {
				SalesforceSelectedObjects.Add(e.Value);
			}
			else if(!e.IsChecked) {
				SalesforceSelectedObjects.Remove(e.Value);
			}
			SaveCheckedSalesforceObjectsInfo();
			SalesforceObjects_CollectionChanged(null, null);
		}

		private void SalesforcePassword_PasswordChanged(object sender, RoutedEventArgs e) {
			SalesforcePassword = ((PasswordBox)sender).Password;
			SaveSalesforceConnectionInfo();
		}
		#endregion

		#region SalesforceConnect_Click(); SelectAllObjects_Click(); UnselectAllObjects_Click();
		private void SalesforceConnect_Click(object sender, RoutedEventArgs e) {
			SaveSalesforceConnectionInfo();

			try {
				StatusText = string.Format("Connecting to {0}...", SalesforceUrl);
				salesforceAPI = new SalesforceApi(SalesforceUrl, SalesforceUsername, SalesforcePassword, SalesforceToken);
				StatusText = string.Format("Retrieving object listing...", SalesforceUrl);
				List<string> sfObjects = salesforceAPI.GetObjectNames();
				sfObjects.Sort(StringComparer.InvariantCultureIgnoreCase);
				SalesforceObjects.Clear();
				foreach (string sObjectName in sfObjects) {
					SalesforceObjects.Add(new CheckedListItem(sObjectName) {
						IsChecked = SalesforceSelectedObjects.Contains(sObjectName)
					});
				}
				FilterSalesforceObjects(null);
				if (SalesforceObjects.Count > 0) {
					GenerateScriptVisibility = Visibility.Visible;
				}
				SalesforceObjects_CollectionChanged(null, null);
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
			SaveCheckedSalesforceObjectsInfo();
		}
		public void UnselectAllObjects_Click(object sender, RoutedEventArgs e) {
			foreach (CheckedListItem i in SalesforceObjects) {
				i.IsChecked = false;
			}
			SaveCheckedSalesforceObjectsInfo();
		}
		#endregion

		#region SaveSalesforceConnectionInfo(); SaveCheckedSalesforceObjectsInfo(); SaveSqlInfo();
		private void SaveSalesforceConnectionInfo(object sender, RoutedEventArgs e) {
			SaveSalesforceConnectionInfo();
		}
		private void SaveSalesforceConnectionInfo() {
			if (loaded) {
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

		private void SaveCheckedSalesforceObjectsInfo() {
			SettingsManager.SetValue("SalesforceRememberSelectedObjects", SalesforceRememberSelectedObjects);
			if (SalesforceRememberSelectedObjects) {
				SettingsManager.SetValue("SalesforceSelectedObjects", SalesforceSelectedObjects);
			}
			else {
				SettingsManager.SetValue<StringCollection>("SalesforceSelectedObjects", null);
			}
		}

		private void SaveSqlInfo(object sender, RoutedEventArgs e) {
			SaveSqlInfo();
		}
		private void SaveSqlInfo() {
			if (loaded) {
				SettingsManager.SetString("SqlSchemaName", SqlSchemaName);
				SettingsManager.SetValue("SqlTextUnicode", SqlTextUnicode);
				SettingsManager.SetNullableValue("SqlVarcharMaxMinimumThreshold", SqlVarcharMaxMinimumThreshold);
				SettingsManager.SetValue("SqlGenerateForeignKeys", SqlGenerateForeignKeys);
				SettingsManager.SetValue("SqlGenerateForeignKeysSelectedObjectsOnly", SqlGenerateForeignKeysSelectedObjectsOnly);
			}
		}
		#endregion

		#region SaveAsSingleFile(); SaveAsMultipleFiles();
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
		#endregion

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
			IEnumerable<DescribeSObjectResult> sObjects = salesforceAPI.GetSObjectDetails(tableNames);
			StatusText = string.Format("Generating SQL script for {0} objects...", sObjects.Count());
			string stringDataType = (SqlTextUnicode ? "NVARCHAR" : "VARCHAR"); 
			string schema = null;
			if (!string.IsNullOrWhiteSpace(SqlSchemaName)) {
				schema = string.Format("[{0}].", SqlSchemaName);
			}
			foreach (DescribeSObjectResult t in sObjects) {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine(string.Format("CREATE TABLE {0}[{1}] (", schema, t.name));
				List<string> primaryKeys = new List<string>();
				List<ForeignKeyEntry> foreignKeys = new List<ForeignKeyEntry>();
				bool firstRow = true;
				foreach (Field f in t.fields) {
					sb.Append("	"); //indent
					if (!firstRow) {
						sb.Append(",");
					}
					firstRow = false;
					/*  
					 *  Useful starting point: https://learn.capstorm.com/copystorm/frequently-asked-questions/how-does-copystorm-work/how-do-salesforce-types-map-to-database-column-types/
					 */
					switch (f.type.ToString()) {
						case "currency":
						case "decimal":
						case "double":
						case "percent":
							sb.Append(string.Format("[{0}] DECIMAL({1},{2})", f.name, f.precision, f.scale));
							break;
						case "id":
							sb.Append(string.Format("[{0}] {1}({2})", f.name, stringDataType, f.length));
							if (string.Equals(f.name, "id", StringComparison.InvariantCultureIgnoreCase)) {
								primaryKeys.Add(f.name);
							}
							break;
						case "address":
							if (SqlVarcharMaxMinimumThreshold != null && f.length >= SqlVarcharMaxMinimumThreshold.Value) {
								sb.Append(string.Format("[{0}] {1}(MAX)", f.name, stringDataType));
							}
							else if (f.length == 0) {
								if (SqlVarcharMaxMinimumThreshold.Value > 2000) {
									sb.Append(string.Format("[{0}] {1}({2})", f.name, stringDataType, "2000")); //assume 2000 length based on some testing (data may come through as XML)
								}
								else {
									sb.Append(string.Format("[{0}] {1}(MAX)", f.name, stringDataType));
								}
							}
							else {
								sb.Append(string.Format("[{0}] {1}({2})", f.name, stringDataType, f.length));
							}
							break;
						case "combobox":
						case "email":
						case "location":
						case "multipicklist":
						case "phone":
						case "picklist":
						case "string":
						case "url":
						case "textarea":
							if (SqlVarcharMaxMinimumThreshold != null && f.length >= SqlVarcharMaxMinimumThreshold.Value) {
								sb.Append(string.Format("[{0}] {1}(MAX)", f.name, stringDataType));
							}
							else {
								sb.Append(string.Format("[{0}] {1}({2})", f.name, stringDataType, f.length));
							}
							break;
						case "reference":
							sb.Append(string.Format("[{0}] {1}({2})", f.name, stringDataType, f.length));
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
					if ((f.referenceTo != null && f.referenceTo.Length == 1) || string.Equals(f.name, "id", StringComparison.InvariantCultureIgnoreCase)) {
						sb.Append(" COLLATE SQL_Latin1_General_CP1_CS_AS");
						if (f.referenceTo != null && f.referenceTo.Length == 1) {
							foreignKeys.Add(new ForeignKeyEntry(t.name, f.name, f.referenceTo[0], "Id")); //hardcode foreign key to id of ref table
						}
					}
					if (!f.nillable && AllowedNullableFieldNames.Contains(f.name, StringComparer.InvariantCultureIgnoreCase)) {
						sb.Append(" NOT NULL");
					}
					sb.AppendLine();
				}
				if (primaryKeys.Count > 0) {
					sb.AppendLine(string.Format("	,CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ([{1}] ASC)", t.name, string.Join("], [", primaryKeys)));
					//sb.AppendLine(string.Format("	,PRIMARY KEY ([{0}])", string.Join("], [", primaryKeys)));
				}
				if (SqlGenerateForeignKeys) {
					foreach (ForeignKeyEntry fk in foreignKeys) {
						if (!SqlGenerateForeignKeysSelectedObjectsOnly || tableNames.Contains(fk.ToTable, StringComparer.InvariantCultureIgnoreCase)) {
							sb.AppendLine(string.Format("	,CONSTRAINT [FK_{0}_{1}_{2}] FOREIGN KEY ([{1}]) REFERENCES {4}[{2}]([{3}])", fk.FromTable, fk.FromField, fk.ToTable, fk.ToField, schema));
						}
					}
				}
				sb.Append(");");

				output.Add(t.name, sb.ToString());
			}
			StatusText = string.Format("Generated SQL script for {0} objects", sObjects.Count());

			return output;
		}

		private void GenerateSqlScript_Click(object sender, RoutedEventArgs e) {
			StringBuilder sb = new StringBuilder();
			GeneratedSqlScript.Clear();
			GeneratedSqlScript = GenerateSqlScript();
			bool isFirst = true;
			foreach (string sql in GeneratedSqlScript.Values.Reverse()) {
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

		#region FilterSalesforceObjects();
		private void FilterSalesforceObjects(string filter) {
			foreach (CheckedListItem i in SalesforceObjects) {
				i.IsVisible = string.IsNullOrWhiteSpace(filter) || i.Label.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1;
			}
			OnPropertyChanged("SalesforceObjects");
		}
		#endregion

		private void FilterTextbox_TextChanged(object sender, TextChangedEventArgs e) {
			FilterSalesforceObjects(((TextBox)e.Source).Text);
		}
	}
}