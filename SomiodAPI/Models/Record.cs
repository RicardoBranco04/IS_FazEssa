using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SomiodAPI.Models
{
    public class Record
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; } = "";
        public string Creation_date { get; set; } = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        public int Parent { get; set; }
        public string res_type = "record";
    }
}
