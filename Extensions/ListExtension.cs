namespace DistributedCache.Extensions
{
    public static class ListExtension
    {
        public static IList<T> Map<T>(this IList<T> source, Func<T, bool> body)
        { 
            return source;
        }
    }
}
