using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SFDC.Models;
using SFDC.soapApi;

namespace SFDC
{
    public class SalesForceApi
    {
        private readonly SforceService _binding;
        /// <summary>
        /// By using this constructor, you need to Login(string userName, string password)
        /// </summary>
        public SalesForceApi()
        {
            _binding = new SforceService();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        }
        public SalesForceApi(string endPointUrl, string sessionId)
        {
            _binding = new SforceService();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Init(endPointUrl, sessionId);
        }
        private void Init(string endPointUrl, string sessionId)
        {
            _binding.Url = endPointUrl;
            _binding.SessionHeaderValue = new SessionHeader { sessionId = sessionId };
        }

        public GetUserInfoResult GetUserInfo()
        {
            return _binding.getUserInfo();
        }

        public LoginResult Login(string userName, string password)
        {
            return _binding.login(userName, password);
        }

        ///<summary>
        ///Create any sObject such as Opportunity, Account, CustomObject__c
        ///</summary>
        ///<param name="model">
        ///model.Type = "Account"
        ///model.Fields = new List(ElementField)(){ ElementField ={Key ="Name", Value = "My new Account Name"} etc.}
        ///</param>
        ///<returns></returns>
        public SaveResult[] CreateElement(CreateSObject model)
        {
            if (model?.Fields == null)
            {
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

            for (int i = 0; i < model.Fields.Count; ++i)
            {
                fields[i] = doc.CreateElement(model.Fields[i].Key);
                fields[i].InnerText = model.Fields[i].Value;
            }

            element.type = model.Type;
            element.Any = fields;

            return _binding
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
        public SaveResult[] UpdateElement(UpdateSObject model)
        {
            if (model?.Fields == null)
            {
                return  new SaveResult[]
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

            for (int i = 0; i < model.Fields.Count; ++i)
            {
                fields[i] = doc.CreateElement(model.Fields[i].Key);
                fields[i].InnerText = model.Fields[i].Value;
            }

            element.type = model.Type;
            element.Id = model.Id;
            element.Any = fields;

            return _binding
                .update(new sObject[] { element });
        }
      
        public List<string> GatTableNameList()
        {
            List<string>  sobjectsNames = new List<string>();
            DescribeGlobalResult dgr = _binding.describeGlobal();

            foreach (var obj in dgr.sobjects)
            {
                sobjectsNames.Add(obj.name);
            }

            return sobjectsNames;
        }

        public List<TableInfo> DescribeTables(string[] tableNames)
        {
            DescribeSObjectResult[] dsrArray = _binding.describeSObjects(tableNames.Take(50).ToArray()); // get everything as this project grows we will need more from this
            var results = dsrArray.Select(c => new TableInfo
            {
                name = c.name,
                label = c.label,
                fields = c.fields?.Select(x => new FieldInformation
                {
                    label = x.label,
                    name = x.name,
                    type = x.type.ToString()
                }).ToList(),
                relationships = c.childRelationships?.Select(x => new RelationshipInformation
                {
                    ChildTable = x.childSObject,
                    ChildField = x.field
                }).ToList()
            }).ToList();

            return results;
        }
    }
}
