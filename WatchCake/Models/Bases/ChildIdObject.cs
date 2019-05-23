using WatchCake.Helpers;
using WatchCake.Models.Interfaces;

namespace WatchCake.Models.Bases
{
    /// <summary>
    /// Objects that has integer ID.
    /// </summary>
    public abstract class ChildIdObject<TParent> : IdObject, IIdentifiable 
        where TParent : IIdentifiable
    {
        private TParent _parent;

        /// <summary>
        /// An ID of a parent this entity belongs to.
        /// </summary>
        public int? ParentID { get; private set; }

        /// <summary>
        /// A parent this entity belongs to. Setter also sets ParentID.
        /// </summary>
        public TParent Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                ParentID = value?.ID;
            }
        }
    }
}