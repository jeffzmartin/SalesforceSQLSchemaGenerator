using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDC.metadataApi;

namespace SFDC.Models.MetaDataComponents
{
    public class EncryptedTextElement : IElement
    {
        public CustomField CreateElement(ElementModel model)
        {
            CustomField fieldText = new CustomField();
            fieldText.fullName = model.FullName;
            fieldText.label = model.Label;
            fieldText.type = FieldType.EncryptedText;
            fieldText.typeSpecified = true;
            fieldText.length = model.Length;
            fieldText.lengthSpecified = true;
            fieldText.maskChar = model.MaskChar;
            fieldText.maskCharSpecified = true;
            fieldText.maskType = EncryptedFieldMaskType.lastFour;
            fieldText.maskTypeSpecified = true;

            return fieldText;
        }
    }
   
    
}
