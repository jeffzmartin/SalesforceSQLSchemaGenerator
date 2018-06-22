using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceSQLSchemaGenerator {
	public class CheckedListItem : INotifyPropertyChanged {
		public CheckedListItem(string value, string label) {
			Value = value;
			Label = label;
		}
		public CheckedListItem(string value) {
			Value = value;
			Label = value.Replace("_", "__");
		}
		private string value;
		public string Value { get {
				return value;
			}
			set {
				this.value = value;
				OnPropertyChanged("Value");
			}
		}
		private string label;
		public string Label {
			get {
				return label;
			}
			set {
				this.label = value;
				OnPropertyChanged("Label");
			}
		}
		private bool isChecked = false;
		public bool IsChecked {
			get {
				return isChecked;
			}
			set {
				this.isChecked = value;
				OnPropertyChanged("IsChecked");
			}
		}

		public delegate void CheckedChangedEventHandler(string value, bool isChecked);
		public event CheckedChangedEventHandler CheckedChanged;

		#region INotifyPropertyChanged implementation
		// Basically, the UI thread subscribes to this event and update the binding if the received Property Name correspond to the Binding Path element
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			CheckedChanged?.Invoke(value, isChecked);
		}
		#endregion
	}

	public class ObservableCheckedListCheckedChanged {
		public string Value { get; set; }
		public bool IsChecked { get; set; }
	}

	public class ObservableCheckedListItemCollection : ObservableCollection<CheckedListItem> {
		public delegate void ItemCheckedChangedHandler(ObservableCheckedListCheckedChanged e);

		public new void Add(CheckedListItem item) {
			item.CheckedChanged += Item_CheckedChanged;
			base.Add(item);
		}

		public event ItemCheckedChangedHandler ItemCheckedChanged;
		private void Item_CheckedChanged(string value, bool isChecked) {
			ItemCheckedChanged?.Invoke(new ObservableCheckedListCheckedChanged() {
				Value = value,
				IsChecked = isChecked
			});
		}
	}
}
