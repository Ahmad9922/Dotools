using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotools
{
    public static class clsDataTypes
    {
        public class clsFilterData
        {
            public enum enFilterStyle { Contains = 1, Equals = 2 }

            public string Value { get; set; }
            public string FieldName { get; set; }
            public enFilterStyle FilterStyle { get; set; }

            public clsFilterData()
            {

            }

            public clsFilterData(string Value, string FieldName)
            {
                this.Value = Value;
                this.FieldName = FieldName;
                FilterStyle = enFilterStyle.Contains;
            }
        }
    }
}
