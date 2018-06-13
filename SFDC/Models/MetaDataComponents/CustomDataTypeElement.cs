using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDC.metadataApi;

namespace SFDC.Models.MetaDataComponents
{
    public class CustomDataTypeElement : IElement
    {
        public CustomField CreateElement(ElementModel model)
        {
            CustomField fieldText = new CustomField();
            fieldText.fullName = model.FullName;
            fieldText.label = model.Label;
            fieldText.type = FieldType.CustomDataType;
            fieldText.typeSpecified = true;

            return fieldText;
        }
    }
}
