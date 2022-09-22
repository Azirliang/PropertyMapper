using LowCode.PropertyMapper.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowCode.PropertyMapper.Test.Fake
{
    public class Student
    {
        public string Name { get; set; }

        public int Age { get; set; }

        [PropertyMapper]
        public string Remark { get; set; }
    }
}
