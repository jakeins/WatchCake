namespace WatchCake.Parsers.Enums
{
    /// <summary>
    /// The mode of the default option of the product.
    /// </summary>
    public enum DefaultOptionMode
    {
        /// <summary>
        /// There is no default option for this site.
        /// </summary>
        Ignore,

        /// <summary>
        /// Default option is the one of all options.
        /// </summary>
        Alongside,

        /// <summary>
        /// Default option is the base information, actual options work as modifiers.
        /// </summary>
        Basis,

        /// <summary>
        /// Use default option as single option, if there are no actual options.
        /// </summary>
        Alternative,

        /// <summary>
        /// Use default option as the only option
        /// </summary>
        Single
    }
}