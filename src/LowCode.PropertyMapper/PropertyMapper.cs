using LowCode.PropertyMapper.Abstractions;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace LowCode.PropertyMapper
{
    public class PropertyMapper<TSource>
    //: IPropertyMapper<TSource> where TSource : class
    {

        public readonly static Action<TSource, Dictionary<Type, HashSet<object>>> MapperClassCache = MapperCalss();

        public static void MapperClass(TSource source) => MapperClass(source, new Dictionary<Type, HashSet<object>>());

        public static void MapperList(IEnumerable<TSource> sources)
        {
            MapperList(sources, new Dictionary<Type, HashSet<object>>());
        }

        internal static void MapperClass(TSource source, Dictionary<Type, HashSet<object>> kvCache)
        {
            if (source == null)
            {
                return;
            }

            //判断对象引用是否存在过
            if (kvCache.ContainsKey(typeof(TSource)))
            {
                if (!kvCache[typeof(TSource)].Add(source))
                {
                    return;
                }
            }
            else
            {
                kvCache[typeof(TSource)] = new HashSet<object>() { source };
            }

            MapperClassCache(source, kvCache);
        }

        internal static void MapperList(IEnumerable<TSource> sources, Dictionary<Type, HashSet<object>> kvCache)
        {
            foreach (var source in sources)
            {
                MapperClass(source, kvCache);
            }
        }

        private static Action<TSource, Dictionary<Type, HashSet<object>>> MapperCalss()
        {
            var sourceType = typeof(TSource);

            var sourceTypePropertyInfos = sourceType.GetProperties().Where(x => x.PropertyType.IsPublic && x.CanRead && x.CanWrite);

            var sourceTypeParameterExpression = Expression.Parameter(sourceType, "p");

            var kvCacheType = typeof(Dictionary<Type, HashSet<object>>);

            var kvCacheParameterExpression = Expression.Parameter(kvCacheType, "kv");

            List<Expression> expressions = new List<Expression>();

            foreach (var sourceTypePropertyInfo in sourceTypePropertyInfos)
            {
                if (sourceTypePropertyInfo.GetCustomAttribute<PropertyMapperAttribute>() == null)
                {
                    // 如果值类型或者字符串没有加PropertyMapperAttribute特性则不处理
                    if (sourceTypePropertyInfo.PropertyType.IsValueType == true || sourceTypePropertyInfo.PropertyType == typeof(string))
                    {
                        continue;
                    }
                }

                var sourceTypeProperty = Expression.Property(sourceTypeParameterExpression, sourceTypePropertyInfo);

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
                    Expression classMapperExpression = ClassMapperExpression(sourceTypeProperty, sourceTypePropertyInfo.PropertyType, kvCacheParameterExpression);

                    expressions.Add(classMapperExpression);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(sourceTypePropertyInfo.PropertyType))
                {
                    Expression listMapperExpression = ListMapperExpression(sourceTypeProperty, sourceTypePropertyInfo.PropertyType, kvCacheParameterExpression);

                    expressions.Add(listMapperExpression);
                }
            }

            if (expressions.Any())
            {
                var block = Expression.Block(expressions);

                var lambda = Expression.Lambda<Action<TSource, Dictionary<Type, HashSet<object>>>>(block, sourceTypeParameterExpression, kvCacheParameterExpression);

                return lambda.Compile();
            }
            else
            {
                return new Action<TSource, Dictionary<Type, HashSet<object>>>((p, kv) => { });
            }
        }

        private static Expression ClassMapperExpression(Expression classProperty, Type classType, Expression kvCacheExpression)
        {
            var condition = Expression.NotEqual(classProperty, Expression.Constant(null, classType));

            var propertyMapperType = typeof(PropertyMapper<>).MakeGenericType(classType);

            var ifTrue = Expression.Call(
                propertyMapperType.GetMethod(nameof(MapperClass), BindingFlags.NonPublic | BindingFlags.Static,
                new[] {
                    classType,
                    typeof(Dictionary<Type, HashSet<object>>)
                })!, classProperty, kvCacheExpression);

            var ifFalse = Expression.Assign(classProperty, Expression.Constant(null, classType));

            var conditionExpression = Expression.Condition(condition, ifTrue, ifFalse, typeof(void));

            return conditionExpression;
        }

        private static Expression ListMapperExpression(Expression listProperty, Type listType, Expression kvCacheExpression)
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
                ifTrue = Expression.Call(propertyMapperType.GetMethod(nameof(MapperList), BindingFlags.NonPublic | BindingFlags.Static,
                    new[] { listType,
                        typeof(Dictionary<Type, HashSet<object>>)
                    })!, listProperty, kvCacheExpression);
            }

            var ifFalse = Expression.Assign(listProperty, Expression.Constant(null, listType));

            var conditionItem = Expression.Condition(condition, ifTrue, ifFalse, typeof(void));

            return conditionItem;
        }
    }
}
