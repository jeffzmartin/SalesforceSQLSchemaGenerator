using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

/*
 * Uses code from https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file
 * and https://stackoverflow.com/questions/2333149/how-to-fast-get-hardware-id-in-c
 */
namespace SalesforceSQLSchemaGenerator {
	class Encryption {
		public static string GetMachineGuid() {
			string location = @"SOFTWARE\Microsoft\Cryptography";
			string name = "MachineGuid";

			using (RegistryKey localMachineX64View =
				RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
				using (RegistryKey rk = localMachineX64View.OpenSubKey(location)) {
					if (rk == null)
						throw new KeyNotFoundException(
							string.Format("Key Not Found: {0}", location));

					object machineGuid = rk.GetValue(name);
					if (machineGuid == null)
						throw new IndexOutOfRangeException(
							string.Format("Index Not Found: {0}", name));

					return machineGuid.ToString();
				}
			}
		}

		public static string EncryptString(System.Security.SecureString input) {
			byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
				System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
				Encoding.Unicode.GetBytes(GetMachineGuid()),
				System.Security.Cryptography.DataProtectionScope.CurrentUser);
			return Convert.ToBase64String(encryptedData);
		}

		public static SecureString DecryptString(string encryptedData) {
			try {
				byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
					Convert.FromBase64String(encryptedData),
					Encoding.Unicode.GetBytes(GetMachineGuid()),
					System.Security.Cryptography.DataProtectionScope.CurrentUser);
				return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
			}
			catch {
				return new SecureString();
			}
		}

		public static SecureString ToSecureString(string input) {
			SecureString secure = new SecureString();
			foreach (char c in input) {
				secure.AppendChar(c);
			}
			secure.MakeReadOnly();
			return secure;
		}

		public static string ToInsecureString(SecureString input) {
			string returnValue = string.Empty;
			IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
			try {
				returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
			}
			finally {
				System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
			}
			return returnValue;
		}
	}
}
