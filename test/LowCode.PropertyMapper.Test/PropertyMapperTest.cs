using LowCode.PropertyMapper.Test.Fake;
using Newtonsoft.Json;

namespace LowCode.PropertyMapper.Test
{
    public class PropertyMapperTest
    {
        [Fact]
        public void SystemText()
        {
            var classCollection = FakeClass();

            var class2 = classCollection.Last();

            Assert.Throws<System.Text.Json.JsonException>(() => { System.Text.Json.JsonSerializer.Serialize(class2); });
        }

        [Fact]
        public void NewtonSoft()
        {
            var classCollection = FakeClass();

            var class2 = classCollection.Last();

            Assert.Throws<JsonSerializationException>(() => { JsonConvert.SerializeObject(class2); });
        }


        [Fact]
        public void MapperClass()
        {
            var classCollection = FakeClass();

            var firstClass = classCollection.First();

            PropertyMapper<Class>.MapperClass(firstClass);

            Assert.Equal("DEBUG:Name", firstClass.Remark);

            foreach (var student in firstClass.Students)
            {
                Assert.Equal("DEBUG:Name", student.Remark);
            }

            foreach (var teacher in firstClass.Teachers)
            {
                Assert.Equal("DEBUG:Name", teacher.Remark);
            }

            Assert.Null(firstClass.Self);

            Assert.Null(firstClass.NextClass);

            var lastClass = classCollection.Last();

            PropertyMapper<Class>.MapperClass(lastClass);

            Assert.Equal("DEBUG:Name", lastClass.Remark);

            foreach (var student in firstClass.Students)
            {
                Assert.Equal("DEBUG:Name", student.Remark);
            }

            foreach (var teacher in firstClass.Teachers)
            {
                Assert.Equal("DEBUG:Name", teacher.Remark);
            }

            Assert.NotNull(lastClass.Self);

            Assert.NotNull(lastClass.NextClass);

            Assert.Equal(firstClass, lastClass.NextClass);

            Assert.Equal(lastClass, lastClass.Self);

        }

        [Fact]
        public void MapperList()
        {
            var classCollection = FakeClass().ToList();

            PropertyMapper<Class>.MapperList(classCollection);

            foreach (var item in classCollection)
            {
                Assert.Equal("DEBUG:Name", item.Remark);

                foreach (var student in item.Students)
                {
                    Assert.Equal("DEBUG:Name", student.Remark);
                }

                foreach (var teacher in item.Teachers)
                {
                    Assert.Equal("DEBUG:Name", teacher.Remark);
                }
            }
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
                Teachers = new Teacher[] { teacher1, teacher3 },
                NextClass = class1
            };

            class2.Self = class2;

            yield return class2;
        }
    }
}