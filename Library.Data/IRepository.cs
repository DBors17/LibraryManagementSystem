namespace Library.Data
{
    public interface IRepository<T>
    {
        void Add(T entity);
        IEnumerable<T> GetAll();
    }

    public class InMemoryRepository<T> : IRepository<T>
    {
        private readonly List<T> _storage = new();

        public void Add(T entity) => _storage.Add(entity);

        public IEnumerable<T> GetAll() => _storage;
    }
}

