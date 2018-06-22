using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceSQLSchemaGenerator {
	public class ForeignKeyEntry {
		public string FromTable { get; set; }
		public string FromField { get; set; }
		public string ToTable { get; set; }
		public string ToField { get; set; }

		public ForeignKeyEntry(string fromTable, string fromField, string toTable, string toField) {
			FromTable = fromTable;
			FromField = fromField;
			ToTable = toTable;
			ToField = toField;
		}
	}
}
