using System;
using System.Collections.Generic;
using System.Linq;
using WatchCake.Models.Interfaces;

namespace WatchCake.DAL
{
    /// <summary>
    /// Base implementation of the IRepo interface using Dictionary collection, with manually/automatically set ID.
    /// </summary>
    public class DictionaryRepoBase<TEntity> : IRepo<TEntity> where TEntity : class, IIdentifiable
    {
        readonly static Dictionary<int, TEntity> memoryMap = new Dictionary<int, TEntity>();

        public bool Exists(int id) => memoryMap.ContainsKey(id);
        public bool Exists(Func<TEntity, bool> predicate) => memoryMap.Values.Count(predicate) > 0;

        public TEntity Single(int id) => memoryMap[id];
        public TEntity this[int id] => Single(id);
        public TEntity Single(Func<TEntity, bool> predicate) => memoryMap.Values.Single(predicate);

        public TEntity SingleOrDefault(int id) => Exists(id) ? memoryMap[id] : null;
        public TEntity SingleOrDefault(Func<TEntity, bool> predicate) => memoryMap.Values.SingleOrDefault(predicate);

        public TEntity First(Func<TEntity, bool> predicate) => memoryMap.Values.First(predicate);
        public TEntity FirstOrDefault(Func<TEntity, bool> predicate) => memoryMap.Values.FirstOrDefault(predicate);

        public int Count(Func<TEntity, bool> predicate = null)
        {
            if (predicate == null)
                return memoryMap.Count;
            else
                return memoryMap.Values.Count(predicate);
        }

        public IEnumerable<TEntity> List(Func<TEntity, bool> predicate = null)
        {
            if (predicate == null)
                return memoryMap.Values;
            else
                return memoryMap.Values.Where(predicate);
        }        

        public virtual int Add(TEntity model)
        {
            lock (memoryMap)
            {
                int curID = model.ID ?? (memoryMap.Count() > 0 ? (memoryMap.Keys.Max() + 1) : 1);
                model.ID = curID;
                memoryMap.Add(curID, model);
                return curID;
            }
        }

        public List<int> Add(IEnumerable<TEntity> models)
        {
            List<int> results = new List<int>();

            lock (memoryMap)
            {
                foreach (TEntity model in models)
                    results.Add(Add(model));
            }

            return results;
        }

        public void Update(TEntity model)
        {
            if (model.ID == null)
                throw new NullReferenceException($"The ID of the provided model [{model.ID}] is illegal for update operation.");

            memoryMap[(int)model.ID] = model;
        }

        public void Remove(int id) => memoryMap.Remove(id);
    }
}
