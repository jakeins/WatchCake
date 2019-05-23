using System;
using System.Collections.Generic;
using WatchCake.Models.Interfaces;

namespace WatchCake.DAL
{
    /// <summary>
    /// Advanced concrete repository interface.
    /// </summary>
    public interface IRepo<TEntity> where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Store an entity.
        /// </summary>
        /// <returns>Assigned ID of the stored entity.</returns>
        int Add(TEntity model);

        /// <summary>
        /// Store a set of entities.
        /// </summary>
        /// <returns>Assigned IDs of stored entities.</returns>
        List<int> Add(IEnumerable<TEntity> models);

        /// <summary>
        /// Get a number of entites.
        /// </summary>
        /// <param name="predicate">Optional filtering predicate.</param>
        int Count(Func<TEntity, bool> predicate = null);

        /// <summary>
        /// Get a set of entities.
        /// </summary>
        /// <param name="predicate">Optional filtering predicate.</param>
        IEnumerable<TEntity> List(Func<TEntity, bool> predicate = null);

        /// <summary>
        /// Check if entity by the specified ID exists.
        /// </summary>
        bool Exists(int id);

        /// <summary>
        /// Check if entity exists.
        /// </summary>
        /// <param name="predicate">Selection predicate.</param>
        bool Exists(Func<TEntity, bool> predicate);        

        /// <summary>
        /// Get entity by specified ID.
        /// </summary>
        TEntity this[int id] { get; }

        /// <summary>
        /// Get entity by the specified ID. Throws an exception in case of fail.
        /// </summary>
        TEntity Single(int id);

        /// <summary>
        /// Get single entity.
        /// </summary>
        /// <param name="predicate">Selection predicate.</param>
        TEntity Single(Func<TEntity, bool> predicate);

        /// <summary>
        /// Get entity by the specified ID. Returns default value in case of fail.
        /// </summary>
        TEntity SingleOrDefault(int id);             

        /// <summary>
        /// Get single entity. Returns default value in case of fail.
        /// </summary>
        /// <param name="predicate">Selection predicate.</param>
        TEntity SingleOrDefault(Func<TEntity, bool> predicate);
               
        /// <summary>
        /// Get first entity. Throws an exception in case of fail.
        /// </summary>
        /// <param name="predicate">Selection predicate.</param>
        TEntity First(Func<TEntity, bool> predicate);

        /// <summary>
        /// Get first entity. Returns default value in case of fail.
        /// </summary>
        /// <param name="predicate">Selection predicate.</param>
        TEntity FirstOrDefault(Func<TEntity, bool> predicate);

        /// <summary>
        /// Update provided entity, ID is specified in the entity itself.
        /// </summary>
        void Update(TEntity model);

        /// <summary>
        /// Remove entity by the specified ID.
        /// </summary>
        void Remove(int id);
    }
}