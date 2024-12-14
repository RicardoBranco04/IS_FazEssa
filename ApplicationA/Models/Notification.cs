using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationA.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Creation_date { get; set; } = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        public int Parent { get; set; }
        public int Event { get; set; } = 1;
        public string Endpoint { get; set; }
        public Boolean Enabled { get; set; } = true;
        public string res_type { get; set; } = "notification";

    }
}
