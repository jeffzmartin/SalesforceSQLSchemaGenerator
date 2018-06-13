using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceSQLSchemaSync.WPF {
	public class CheckedListItem {
		public CheckedListItem(string value, string label) {
			Value = value;
			Label = label;
		}
		public CheckedListItem(string value) {
			Value = value;
			Label = value.Replace("_", "__");
		}

		public string Value { get; set; }
		public string Label { get; set; }
		public bool IsChecked { get; set; }
	}
}
