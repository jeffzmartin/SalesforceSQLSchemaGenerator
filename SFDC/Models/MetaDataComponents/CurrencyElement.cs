using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDC.metadataApi;

namespace SFDC.Models.MetaDataComponents
{
    public class CurrencyElement : IElement
    {
        public CustomField CreateElement(ElementModel model)
        {
            CustomField fieldText = new CustomField();
            fieldText.fullName = model.FullName;
            fieldText.label = model.Label;
            fieldText.type = FieldType.Currency;
            fieldText.precision = model.Precision;
            fieldText.precisionSpecified = true;
            fieldText.scale = model.Scale;
            fieldText.scaleSpecified = true;
            fieldText.typeSpecified = true;
            return fieldText;
        }
    }
}
