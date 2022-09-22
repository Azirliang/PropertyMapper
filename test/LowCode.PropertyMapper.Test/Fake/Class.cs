using LowCode.PropertyMapper.Abstractions;

namespace LowCode.PropertyMapper.Test.Fake
{
    public class Class
    {
        public string Name { get; set; }

        public Teacher HeadMaster { get; set; }

        [PropertyMapper]
        public string Remark { get; set; }

        public IEnumerable<Student> Students { get; set; }

        public Teacher[] Teachers { get; set; }
    }
}
