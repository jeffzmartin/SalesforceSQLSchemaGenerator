using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDC.metadataApi;

namespace SFDC.Models.MetaDataComponents
{
    public class PercentElement : IElement
    {
        public CustomField CreateElement(ElementModel model)
        {
            CustomField fieldText = new CustomField();
            fieldText.fullName = model.FullName;
            fieldText.label = model.Label;
            fieldText.type = FieldType.Percent;
            fieldText.typeSpecified = true;
            fieldText.precisionSpecified = true;
            fieldText.precision = model.Precision;
            fieldText.scale = model.Scale;
            fieldText.scaleSpecified = true;
            return fieldText;
        }
    }
}
