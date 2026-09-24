using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace AzureTools.Stores
{


    public class MemoryStoreSet<TEntity> : IStoreSet<TEntity>, IEnumerable<TEntity>, IListSource where TEntity : class
    {
        [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public MemoryStoreSet(List<TEntity> list)
        {
            this.list = list;
#pragma warning disable IL3050
            queryable = list.AsQueryable();
#pragma warning restore IL3050
        }
        private readonly List<TEntity> list;
        private readonly IQueryable<TEntity> queryable;

        bool IListSource.ContainsListCollection => true;

        Type IQueryable.ElementType => queryable.ElementType;

        Expression IQueryable.Expression => queryable.Expression;

        IQueryProvider IQueryable.Provider => queryable.Provider;

        public IEnumerator<TEntity> GetEnumerator()
        {
            return ((IEnumerable<TEntity>)list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }

        IList IListSource.GetList()
        {
            return list;
        }



        public void Add(TEntity item)
        {
            list.Add(item);
        }

        public void AddRange(IEnumerable<TEntity> items)
        {
            list.AddRange(items);
        }

        public void Update(TEntity item)
        {
            int index = list.FindIndex(x => object.Equals(x, item));
            if (index == -1)
                list.Add(item);
        }

        public void Remove(TEntity item)
        {
            list.Remove(item);
        }

        public void RemoveRange(IEnumerable<TEntity> items)
        {
            foreach (TEntity item in items)
            {
                list.Remove(item);
            }
        }
    }
}
