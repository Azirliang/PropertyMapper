using LowCode.PropertyMapper.Test.Fake;
using System.Globalization;

namespace LowCode.PropertyMapper.Test
{
    public class PropertyMapperTest
    {
        [Fact]
        public void MapperClass()
        {
            var classCollection = FakeClass();

            var firstClass = classCollection.First();

            PropertyMapper<Class>.MapperClass(firstClass);

            Assert.Equal("DEBUG:Remark", firstClass.Remark);

            var lastClass = classCollection.Last();

            PropertyMapper<Class>.MapperClass(lastClass);

        }

        [Fact]
        public void MapperList()
        {
            var classCollection = FakeClass();

            PropertyMapper<Class>.MapperList(classCollection.ToList());

        }

        private IEnumerable<Class> FakeClass()
        {
            Student student1 = new Student() { Name = "student1", Age = 10, Remark = "我是学生1" };
            Student student2 = new Student() { Name = "student2", Age = 10, Remark = "我是学生2" };
            Student student3 = new Student() { Name = "student3", Age = 12, Remark = "我是学生3" };
            Student student4 = new Student() { Name = "student4", Age = 10, Remark = "我是学生4" };
            Student student5 = new Student() { Name = "student5", Age = 11, Remark = "我是学生5" };
            Student student6 = new Student() { Name = "student6", Age = 12, Remark = "我是学生6" };

            Teacher teacher1 = new Teacher() { Name = "teacher1", Remark = "我是老师1" };
            Teacher teacher2 = new Teacher() { Name = "teacher2", Remark = "我是老师2" };
            Teacher teacher3 = new Teacher() { Name = "teacher3", Remark = "我是老师3" };

            Class class1 = new Class
            {
                Name = "class1",
                Remark = "班级1",
                HeadMaster = teacher1,
                Students = new List<Student> { student1, student2, student3 },
                Teachers = new Teacher[] { teacher1, teacher2 }
            };

            yield return class1;

            Class class2 = new Class
            {
                Name = "class2",
                Remark = "班级2",
                HeadMaster = teacher2,
                Students = new List<Student> { student4, student5, student6 },
                Teachers = new Teacher[] { teacher1, teacher3 }
            };

            yield return class2;
        }
    }
}