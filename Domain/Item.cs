using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdjacenyListDemo.Web.Domain
{
    public class Item
    {
        public string HASH { get; set; }
        public string RANGE { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime CreatedDateTime { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
