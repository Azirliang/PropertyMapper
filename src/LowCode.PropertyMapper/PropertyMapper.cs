using LowCode.PropertyMapper.Abstractions;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace LowCode.PropertyMapper
{
    public class PropertyMapper<TSource>
    //: IPropertyMapper<TSource> where TSource : class
    {
        public readonly static Action<TSource> MapperClassCache = MapperCalss();

        public static void MapperClass(TSource source) => MapperClassCache(source);

        public static void MapperList(IEnumerable<TSource> sources)
        {
            foreach (var source in sources)
            {
                MapperClass(source);
            }
        }

        private static Action<TSource> MapperCalss()
        {
            var sourceType = typeof(TSource);

            var sourceTypePropertyInfos = sourceType.GetProperties().Where(x => x.PropertyType.IsPublic && x.CanRead && x.CanWrite);

            var sourceTypeParmeterExpression = Expression.Parameter(sourceType, "p");

            List<Expression> expressions = new List<Expression>();

            foreach (var sourceTypePropertyInfo in sourceTypePropertyInfos)
            {
                if (sourceTypePropertyInfo.PropertyType == typeof(TSource))
                {
                    continue;
                }

                if (sourceTypePropertyInfo.GetCustomAttribute<PropertyMapperAttribute>() == null)
                {
                    // 如果值类型或者字符串没有加PropertyMapperAttribute特性则不处理
                    if (sourceTypePropertyInfo.PropertyType.IsValueType == true || sourceTypePropertyInfo.PropertyType == typeof(string))
                    {
                        continue;
                    }
                }

                var sourceTypeProperty = Expression.Property(sourceTypeParmeterExpression, sourceTypePropertyInfo);

                if (sourceTypePropertyInfo.PropertyType.IsValueType == true || sourceTypePropertyInfo.PropertyType == typeof(string))
                {
                    Expression destinationValue;

#if DEBUG
                    destinationValue = Expression.Constant($"DEBUG:{nameof(sourceTypePropertyInfo.Name)}");
#else
                    //TODO: 处理Value的值转换
                    destinationValue = Expression.Constant(sourceTypeProperty);
#endif

                    var convertValue = Expression.Convert(destinationValue, sourceTypePropertyInfo.PropertyType);

                    Expression assignExpression = Expression.Assign(sourceTypeProperty, convertValue);

                    expressions.Add(assignExpression);

                    continue;
                }

                if (sourceTypePropertyInfo.PropertyType.IsClass && !sourceTypePropertyInfo.PropertyType.IsArray && sourceTypePropertyInfo.PropertyType.IsGenericType == false)
                {
                    Expression classMapperExpression = ClassMapperExpression(sourceTypeProperty, sourceTypePropertyInfo.PropertyType);

                    expressions.Add(classMapperExpression);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(sourceTypePropertyInfo.PropertyType))
                {
                    Expression listMapperExpression = ListMapperExpression(sourceTypeProperty, sourceTypePropertyInfo.PropertyType);

                    expressions.Add(listMapperExpression);
                }
            }

            if (expressions.Any())
            {
                var block = Expression.Block(expressions);

                var lambda = Expression.Lambda<Action<TSource>>(block, sourceTypeParmeterExpression);

                return lambda.Compile();
            }
            else
            {
                return new Action<TSource>(p => { });
            }
        }

        private static Expression ClassMapperExpression(Expression classProperty, Type classType)
        {
            var condition = Expression.NotEqual(classProperty, Expression.Constant(null, classType));

            var propertyMapperType = typeof(PropertyMapper<>).MakeGenericType(classType);

            var ifTrue = Expression.Call(propertyMapperType.GetMethod(nameof(MapperClass), new[] { classType })!, classProperty);

            var ifFalse = Expression.Assign(classProperty, Expression.Constant(null, classType));

            var conditionExpression = Expression.Condition(condition, ifTrue, ifFalse, typeof(void));

            return conditionExpression;
        }

        private static Expression ListMapperExpression(Expression listProperty, Type listType)
        {
            var condition = Expression.NotEqual(listProperty, Expression.Constant(null, listType));

            var listItemType = listType.IsArray ? listType.GetElementType() : listType.GetGenericArguments()[0];

            var propertyMapperType = typeof(PropertyMapper<>).MakeGenericType(listItemType!);

            Expression ifTrue;

            if (typeof(IDictionary).IsAssignableFrom(listType))
            {
                // TODO: 处理字典类型
                ifTrue = Expression.Equal(Expression.Constant(1), Expression.Constant(-1));
            }
            else
            {
                ifTrue = Expression.Call(propertyMapperType.GetMethod(nameof(MapperList), new[] { listType })!, listProperty); ;
            }

            var ifFalse = Expression.Assign(listProperty, Expression.Constant(null, listType));

            var conditionItem = Expression.Condition(condition, ifTrue, ifFalse, typeof(void));

            return conditionItem;
        }
    }
}
