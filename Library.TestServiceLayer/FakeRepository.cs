using Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;

public class FakeRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _data = new();

    public void Add(T entity)
    {
        _data.Add(entity);
    }

    public IEnumerable<T> GetAll()
    {
        return _data;
    }

    public void Remove(T entity)
    {
        _data.Remove(entity);
    }
}
