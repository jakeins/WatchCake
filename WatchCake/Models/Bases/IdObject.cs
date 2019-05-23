using System.ComponentModel.DataAnnotations;
using WatchCake.Helpers;
using WatchCake.Models.Interfaces;

namespace WatchCake.Models.Bases
{
    /// <summary>
    /// Objects that has integer ID.
    /// </summary>
    public class IdObject : NotifyingObject, IIdentifiable 
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        [Key]
        public int? ID { get; set; }
    }
}