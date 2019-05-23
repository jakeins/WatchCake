namespace WatchCake.Parsers.Enums
{
    /// <summary>
    /// Price saving mode.
    /// </summary>
    public enum PriceMode
    {
        /// <summary>
        /// Price is standalone by itself.
        /// </summary>
        AsIs,

        /// <summary>
        /// Price of the option is the delta from default options price, it should be added to make complete price value.
        /// </summary>
        Add
    }
}