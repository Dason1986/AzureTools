using System.Collections.Generic;
using System.Linq;

namespace AzureTools.Stores
{
    public interface IStoreContext
    {
        bool Encrypted { get; }

        void EnableEncryption();



        IStoreSet<TEntity> GetSet<TEntity>() where TEntity : class;



        void Add<TEntity>(TEntity item) where TEntity : class;

        void AddRange<TEntity>(IEnumerable<TEntity> items) where TEntity : class;

        void Update<TEntity>(TEntity item) where TEntity : class;

        void Remove<TEntity>(TEntity item) where TEntity : class;

        void RemoveRange<TEntity>(IEnumerable<TEntity> items) where TEntity : class;

        void SaveChanges();

        void DiscardChanges();
    }

    public interface IStoreSet<TEntity> : IQueryable<TEntity> where TEntity : class
    {


        void Add(TEntity item);

        void AddRange(IEnumerable<TEntity> items);

        void Update(TEntity item);

        void Remove(TEntity item);

        void RemoveRange(IEnumerable<TEntity> items);

    }
}
