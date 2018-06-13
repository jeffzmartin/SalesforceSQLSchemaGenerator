using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDC.metadataApi;

namespace SFDC.Models.MetaDataComponents
{
    public class SummaryElement : IElement
    {
        public CustomField CreateElement(ElementModel model)
        {
            CustomField fieldText = new CustomField();
            fieldText.fullName = model.FullName;
            fieldText.label = model.Label;
            fieldText.type = FieldType.Summary;
            fieldText.typeSpecified = true;
            fieldText.summaryOperation = model.SummaryOperation;
            fieldText.summaryOperationSpecified = true;
            fieldText.summaryForeignKey = model.SummaryForeignKey;
            return fieldText;
        }
    }
}
