using WatchCake.Models.Bases;

namespace WatchCake.Models
{
    /// <summary>
    /// The entry representing many-to-many relationship between Page and Tracker. Is derivtive for relation DB, but defined explicitly for dictionary implementation.
    /// </summary>
    public class TrackedPage : IdObject
    {
        /// <summary>
        /// An ID of a parent this entity belongs to.
        /// </summary>
        public int? TrackerID { get; set; }

        /// <summary>
        /// Parent this entity belongs to.
        /// </summary>
        public Tracker Tracker { get; set; }

        /// <summary>
        /// The ID a page than being tracker by a Tracker.
        /// </summary>
        public int? PageID { get; set; }

        /// <summary>
        /// Page that being tracker by a Tracker.
        /// </summary>
        public Page Page { get; set; }

        /// <summary>
        /// Parameter inidicating that current tracking relationship is currently needs to be tracked.
        /// Useful for archived, inactual and broken pages in a false state.
        /// </summary>
        public bool IsTracked { get; set; } = true;
    }
}
