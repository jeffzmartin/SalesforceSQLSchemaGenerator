using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SFDC.Models;
using SFDC.soapApi;
using SFDC;

namespace SalesforceSQLSchemaSync.WPF {
	public class SalesforceApi : IDisposable {
		private readonly SforceService salesforceSoapService;
		private readonly LoginResult loginResult;
		private readonly string url;

		public SalesforceApi(string url, string username, string password, string token) {
			this.url = url;

			salesforceSoapService = new SforceService();
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			loginResult = Login(username, password + token);

			Init(url, loginResult.sessionId);
		}

		public List<string> GetObjectNames() {
			return GetTableNameList();
		}

		#region SFDC.SalesforceApi from https://github.com/marcincymbalista/SFDC/
		private void Init(string endPointUrl, string sessionId) {
			salesforceSoapService.Url = string.Format("{0}/services/Soap/u/18.0", endPointUrl);
			salesforceSoapService.SessionHeaderValue = new SessionHeader { sessionId = sessionId };
		}

		public GetUserInfoResult GetUserInfo() {
			return salesforceSoapService.getUserInfo();
		}

		private LoginResult Login(string userName, string password) {
			return salesforceSoapService.login(userName, password);
		}

		///<summary>
		///Create any sObject such as Opportunity, Account, CustomObject__c
		///</summary>
		///<param name="model">
		///model.Type = "Account"
		///model.Fields = new List(ElementField)(){ ElementField ={Key ="Name", Value = "My new Account Name"} etc.}
		///</param>
		///<returns></returns>
		public SaveResult[] CreateElement(CreateSObject model) {
			if (model?.Fields == null) {
				return new SaveResult[]
				{
					new SaveResult()
					{
						success = false,
						errors =  new Error[]
						{
							new Error()
							{
							   message = "UpdateSObject or UpdateSObject.Fields cannot be null"
							}
						}
					}
				};
			}

			sObject element = new sObject();
			XmlDocument doc = new XmlDocument();
			XmlElement[] fields = new XmlElement[model.Fields.Count];

			for (int i = 0; i < model.Fields.Count; ++i) {
				fields[i] = doc.CreateElement(model.Fields[i].Key);
				fields[i].InnerText = model.Fields[i].Value;
			}

			element.type = model.Type;
			element.Any = fields;

			return salesforceSoapService
				.create(new sObject[] { element });
		}
		///<summary>
		///Update any sObject such as Opportunity, Account, CustomObject__c
		///</summary>
		///<param name="model">
		///model.Id = "SalesForceId"
		///model.Type = "Account"
		///model.Fields = new List(ElementField)(){ ElementField ={Key ="Name", Value = "My new Account Name"} etc.  }
		///</param>
		///<returns></returns>
		public SaveResult[] UpdateElement(UpdateSObject model) {
			if (model?.Fields == null) {
				return new SaveResult[]
				{
					new SaveResult()
					{
						success = false,
						errors =  new Error[]
						{
							new Error()
							{
								message = "UpdateSObject or UpdateSObject.Fields cannot be null "
							}
						}
					}
				};
			}

			sObject element = new sObject();
			XmlDocument doc = new XmlDocument();
			XmlElement[] fields = new XmlElement[model.Fields.Count];

			for (int i = 0; i < model.Fields.Count; ++i) {
				fields[i] = doc.CreateElement(model.Fields[i].Key);
				fields[i].InnerText = model.Fields[i].Value;
			}

			element.type = model.Type;
			element.Id = model.Id;
			element.Any = fields;

			return salesforceSoapService
				.update(new sObject[] { element });
		}

		public List<string> GetTableNameList() {
			List<string> sobjectsNames = new List<string>();
			DescribeGlobalResult dgr = salesforceSoapService.describeGlobal();

			foreach (var obj in dgr.sobjects) {
				sobjectsNames.Add(obj.name);
			}

			return sobjectsNames;
		}

		public List<DescribeSObjectResult> GetSObjectDetails(List<string> tableNames) {
			List<DescribeSObjectResult> results = new List<DescribeSObjectResult>();
			foreach(List<string> tableSet in splitList(tableNames, 100)) {
				results.AddRange(salesforceSoapService.describeSObjects(tableSet.ToArray()));
			}
			return results;
		}
		#endregion

		/*
		 * From: https://stackoverflow.com/questions/11463734/split-a-list-into-smaller-lists-of-n-size
		 */
		public static IEnumerable<List<T>> splitList<T>(List<T> locations, int nSize = 30) {
			for (int i = 0; i < locations.Count; i += nSize) {
				yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					// TODO: dispose managed state (managed objects).
					salesforceSoapService.logout();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~SalesforceApi() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose() {
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
