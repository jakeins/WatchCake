namespace WatchCake.Models.Interfaces
{
    /// <summary>
    /// Entity that has nullable integer ID property.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        int? ID { get; set; }
    }
}