using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDC.metadataApi;
using SFDC.Models;
using SFDC.Models.MetaDataComponents;

namespace SFDC
{
    public class MetadataApi
    {
        private readonly MetadataService _mtDataService;

        public MetadataApi(string sessionId, string metadataServerUrl)
        {
            _mtDataService = new MetadataService
            {
                SessionHeaderValue = new SessionHeader {sessionId = sessionId},
                Url = metadataServerUrl
            };
        }

        public UpsertResult[] UpsertCustomField(CustomField field)
        {
            return _mtDataService.upsertMetadata(new Metadata[] {
                field
            });
        }

        public UpsertResult[] UpsertCustomObject(CustomObject customObject)
        {
            return _mtDataService.upsertMetadata(new Metadata[] {
                customObject
            });
        }

        public UpsertResult[] CreateCustomField(ElementModel model)
        {
            ElementFactory factory = new ElementFactory();
            IElement element = factory.CreateInstance(model.Type.ToString());
            CustomField field = element.CreateElement(model);
            return UpsertCustomField(field);
        }
    }
}
