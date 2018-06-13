using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceSQLSchemaSync.WPF {
	internal class SettingsManager {
		public static string GetString(string propertyName) {
			if (Properties.Settings.Default[propertyName] != null && !string.IsNullOrWhiteSpace((string)Properties.Settings.Default[propertyName])) {
				return (string)Properties.Settings.Default[propertyName];
			}
			else {
				return null;
			}
		}
		public static T GetValue<T>(string propertyName) {
			return (T)Properties.Settings.Default[propertyName];
		}
		public static T? GetNullableValue<T>(string propertyName) where T : struct {
			if (Properties.Settings.Default[propertyName] != null) {
				TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));
				if (typeConverter != null && typeConverter.CanConvertTo(typeof(T)) && Properties.Settings.Default[propertyName].ToString().Length > 0) {
					return (T)typeConverter.ConvertTo(Properties.Settings.Default[propertyName], typeof(T));
				}
			}
			return null;
		}
		public static void SetString(string propertyName, string value) {
			if (!string.IsNullOrWhiteSpace(value)) {
				Properties.Settings.Default[propertyName] = value;
			}
			else {
				Properties.Settings.Default[propertyName] = null;
			}
			Properties.Settings.Default.Save();
		}
		public static void SetValue<T>(string propertyName, T value) {
			Properties.Settings.Default[propertyName] = value;
			Properties.Settings.Default.Save();
		}
		public static void SetNullableValue<T>(string propertyName, Nullable<T> value) where T : struct {
			if (value != null) {
				Properties.Settings.Default[propertyName] = value.Value.ToString();
			}
			else {
				Properties.Settings.Default[propertyName] = null;
			}
			Properties.Settings.Default.Save();
		}
		public static string GetSecureString(string propertyName) {
			if (Properties.Settings.Default[propertyName] != null) {
				string value = (string)Properties.Settings.Default[propertyName];
				if (!string.IsNullOrEmpty(value)) {
					return Encryption.ToInsecureString(Encryption.DecryptString(value));
				}
			}
			return null;
		}
		public static void SetSecureString(string propertyName, string value) {
			if (!string.IsNullOrWhiteSpace(value)) {
				Properties.Settings.Default[propertyName] = Encryption.EncryptString(Encryption.ToSecureString(value));
			}
			else {
				Properties.Settings.Default[propertyName] = null;
			}
			Properties.Settings.Default.Save();
		}
	}
}
