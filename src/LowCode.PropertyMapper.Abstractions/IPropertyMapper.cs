namespace LowCode.PropertyMapper.Abstractions
{
    public interface IPropertyMapper<in TSource>
    {
        public void MapperClass(TSource source);

        public void MapperList(IEnumerable<TSource> sources);
    }
}
