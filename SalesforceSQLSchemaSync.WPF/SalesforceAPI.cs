using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesforceMagic;
using SalesforceMagic.Configuration;

using SFDC;
using SFDC.metadataApi;
using SFDC.soapApi;

namespace SalesforceSQLSchemaSync.WPF {
	public class SalesforceAPI {
		private SalesforceClient salesforceClient = null;
		private SalesForceApi salesForceApi = null;
		private LoginResult loginResult = null;

		private string url = null;

		public SalesforceAPI(string url, string username, string password, string token) {
			this.url = url;
			salesForceApi = new SalesForceApi();
			loginResult = salesForceApi.Login(username, password + token);

			//salesforceClient = new SalesforceClient(new SalesforceConfig() {
			//	InstanceUrl = url,
			//	Username = username,
			//	Password = password,
			//	SecurityToken = token,
			//	LogoutOnDisposal = true,
			//}, true);
		}

		public List<string> GetObjectNames() {
			return (new SalesForceApi(string.Format("{0}/services/Soap/u/18.0",url), loginResult.sessionId)).GatTableNameList();
		}
	}
}
