using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFDC.metadataApi;

namespace SFDC.Models
{
    public class FieldInformation
    {
        public string name { get; set; }
        public string label { get; set; }
        public string type { get; set; }
		public int length { get; set; }
    }

    public class RelationshipInformation
    {
        public string ChildTable { get; set; }
        public string ChildField { get; set; }
    }
    public class TableInfo
    {

        public TableInfo()
        {
            fields = new List<FieldInformation>();
            relationships = new List<RelationshipInformation>();
        }
        public string name { get; set; }
        public string label { get; set; }
        public List<FieldInformation> fields;
        public List<RelationshipInformation> relationships;
    }

    public class UpdateSObject
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public List<ElementField> Fields { get; set; }
    }

    public class CreateSObject
    {   public string Type { get; set; }
        public List<ElementField> Fields { get; set; }
    }

    public class ElementField
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
