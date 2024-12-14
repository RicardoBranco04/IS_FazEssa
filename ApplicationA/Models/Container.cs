using System;
using System.Collections.Generic;

namespace ApplicationA.Models
{
    public class Container
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Creation_date { get; set; } = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        public int Parent { get; set; }
        public string res_type { get; set; } = "container";
    }
}
