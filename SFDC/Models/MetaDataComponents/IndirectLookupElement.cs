using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDC.metadataApi;

namespace SFDC.Models.MetaDataComponents
{
   public class IndirectLookupElement : IElement
    {
        public CustomField CreateElement(ElementModel model)
        {
            CustomField fieldText = new CustomField();
            fieldText.fullName = model.FullName;
            fieldText.label = model.Label;
            fieldText.type = FieldType.IndirectLookup;
            fieldText.typeSpecified = true;
            fieldText.length = model.Length;
            fieldText.lengthSpecified = true;
            fieldText.referenceTo = model.ReferenceTo;
            return fieldText;
        }
    }
}
