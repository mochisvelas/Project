using DataStructures;
using System.Collections.Generic;

namespace ProjectSQL.Models {

    public class Table {
        
        public List<KeyValuePair<string, string>> columns { get; set; }
        public BPlusTree<List<KeyValuePair<string, string>>, int> data { get; set; }

    }

}
