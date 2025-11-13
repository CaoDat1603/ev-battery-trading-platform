namespace Identity.Domain.Abtractions
{
    public interface ICacheService
    {
        void Set<T>(string key, T value, TimeSpan? expiry = null);
        T? Get<T>(string key);
        void Remove(string key);
    }
}
