using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDC.metadataApi;

namespace SFDC.Models.MetaDataComponents
{
    public class PicklistElement : IElement
    {
        public CustomField CreateElement(ElementModel model)
        {

            List<CustomValue> options = new List<CustomValue>();
            foreach (var picklist in model.PickListValues)
            {
                CustomValue option = new CustomValue();
                option.label = picklist.Text;
                option.fullName = picklist.Value;
                option.@default = picklist.IsDefault;
                options.Add(option);
            }

            ValueSet value = new ValueSet();
            value.valueSetDefinition = new ValueSetValuesDefinition()
            {
                sorted = true,
                value = options.ToArray()

            };

            CustomField field = new CustomField();
            field.fullName = model.FullName;
            field.label = model.Label;
            field.type = FieldType.Picklist;
            field.valueSet = value;
            field.typeSpecified = true;
            field.securityClassification = SecurityClassification.DataIntendedToBePublic;
            field.securityClassificationSpecified = true;

            return field;
        }
    }
}
