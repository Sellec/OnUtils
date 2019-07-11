using System.Collections.Generic;

namespace OnUtils.Application.Modules.Extensions.CustomFields.Model
{
    public class Fields
    {
        public IDictionary<uint, string> Schemes { get; set; }

        public IDictionary<Application.DB.ItemType, IDictionary<Scheme.SchemeItem, string>> SchemeItems { get; set; }

        public IDictionary<int, Field.IField> FieldsList { get; set; }

        public bool AllowSchemesManage { get; set; }
    }
}
