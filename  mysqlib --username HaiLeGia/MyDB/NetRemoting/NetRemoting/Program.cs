using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetRemoting
{
    [Serializable]
    public class Param1
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string PassWord { get; set; }

        public string  FullName{ get; set; }

        public int sex { get; set; }

        public DateTime Birthday { get; set; }

        public string Address { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
