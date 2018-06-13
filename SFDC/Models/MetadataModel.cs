using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDC.metadataApi;

namespace SFDC.Models
{
    public class ElementModel
    {
    
        public string Label { get; set; }
        /// <summary>
        /// Object.CustomField__c (e.g Account.MyTime__c)
        /// </summary>
        public string FullName { get; set; }
        public FieldType Type { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public string InlineHelpText { get; set; }
        public EncryptedFieldMaskChar MaskChar { get; set; }
        public EncryptedFieldMaskType MaskType { get; set; }
        public int Length { get; set; }
        public int Scale { get; set; }
        public string ReferenceTo { get; set; }
        public int Precision { get; set; }
        public List<PicklistValueModel> PickListValues { get; set; }
        public int VisibleLines { get; set; }
        public SummaryOperations SummaryOperation { get; set; }
        public string SummaryForeignKey { get; set; }
    }

    public class PicklistValueModel
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public bool IsDefault { get; set; }

    }
}
