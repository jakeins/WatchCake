using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WatchCake.Models.Interfaces;

namespace WatchCake.DAL
{
    /// <summary>
    /// Base implementation of the IRepo interface using EF.
    /// </summary>
    public class EFRepoBase<TEntity> : IRepo<TEntity> where TEntity : class, IIdentifiable
    {
        readonly WcDbContext Context;

        DbSet<TEntity> Entities => Context.Set<TEntity>();

        public EFRepoBase(WcDbContext context)
        {
            Context = context;
        }

        public bool Exists(int id) => Entities.Any(entity => entity.ID == id);
        public bool Exists(Func<TEntity, bool> predicate) => Entities.Count(predicate) > 0;

        public TEntity Single(int id) => Entities.Single(entity => entity.ID == id);
        public TEntity this[int id] => Single(id);
        public TEntity Single(Func<TEntity, bool> predicate) => Entities.Single(predicate);

        public TEntity SingleOrDefault(int id) => Entities.SingleOrDefault(entity => entity.ID == id);
        public TEntity SingleOrDefault(Func<TEntity, bool> predicate) => Entities.SingleOrDefault(predicate);

        public TEntity First(Func<TEntity, bool> predicate) => Entities.First(predicate);
        public TEntity FirstOrDefault(Func<TEntity, bool> predicate) => Entities.FirstOrDefault(predicate);

        public int Count(Func<TEntity, bool> predicate = null)
        {
            if (predicate == null)
                return Entities.Count();
            else
                return Entities.Count(predicate);
        }

        public IEnumerable<TEntity> List(Func<TEntity, bool> predicate = null)
        {
            if (predicate == null)
                return Entities.ToList();
            else
                return Entities.Where(predicate).ToList();
        }

        public virtual int Add(TEntity model)
        {
            Entities.Add(model);
            Context.SaveChanges();
            return (int)model.ID;
        }

        public List<int> Add(IEnumerable<TEntity> models)
        {
            Entities.AddRange(models);
            Context.SaveChanges();
            return models.Select(m=>(int)m.ID).ToList();
        }

        public void Update(TEntity newModel)
        {
            var id = newModel.ID ?? throw new NullReferenceException($"The ID of the provided model [{newModel.ID}] is illegal for update operation.");

            var oldModel = this[id];

            Context.Entry(oldModel).CurrentValues.SetValues(newModel);
            Context.SaveChanges();
        }

        public void Remove(int id)
        {
            var model = this[id];
            Entities.Remove(model);
            Context.SaveChanges();
        }
    }
}
