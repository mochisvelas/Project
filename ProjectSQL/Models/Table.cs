using System.Collections.Generic;

namespace ProjectSQL.Models {

    public class Table {

        public string name { get; set; }
        public List<KeyValuePair<string, string>> columns { get; set; }

    }

}
