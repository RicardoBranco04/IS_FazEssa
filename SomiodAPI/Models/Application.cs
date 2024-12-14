using System;

namespace SomiodAPI.Models
{
    public class Application
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Creation_date { get; set; } = DateTime.Now.ToString("yyyy/MM/dd H:mm:ss");
        public string res_type = "application";
    }
}