using System.Collections.Generic;
using System.Linq;
using WatchCake.Helpers;
using WatchCake.Models.Bases;
using WatchCake.Parsers.Models;

namespace WatchCake.Models
{
    /// <summary>
    /// The product option/variant that holds basic data and have associated Snapshots.
    /// </summary>
    public class Option : ChildIdObject<Page>
    {
        /// <summary>
        /// Internal retailer site identifier of the specified product.
        /// </summary>
        public string Code { get; set; }

        string _givenName;
        /// <summary>
        /// Display name.
        /// </summary>
        public string Name
        {
            get => _givenName ?? PropertiesAB;
            set => _givenName = value;
        }

        /// <summary>
        /// One of two basic product options, usually the color.
        /// </summary>
        public string PropertyA { get; set; }

        /// <summary>
        /// One of two basic product options, usually the size.
        /// </summary>
        public string PropertyB { get; set; }

        /// <summary>
        /// Additional Product Option.
        /// </summary>
        public string PropertyC { get; set; }

        /// <summary>
        /// Combined option phrase.
        /// </summary>
        public string PropertiesAB
        {
            get
            {
                var props = new List<string>();

                foreach (string prop in new[] { PropertyA, PropertyB, PropertyC })
                {
                    if (!string.IsNullOrEmpty(prop))
                        props.Add(prop);
                }

                return props.Count() > 0 ? string.Join("/", props) : "-";
            }
        }

        #region Tracker Window Props

        /// <summary>
        /// Snapshots, associated with this option.
        /// </summary>
        public IEnumerable<Snapshot> Snapshots { get; set; }

        /// <summary>
        /// Mean price of all snapshots associated with this option.
        /// </summary>
        public Money PriceMean => this.CalculateWeightedMeanPrice();

        /// <summary>
        /// Indicator of latest price deviation from the average option price.
        /// </summary>
        public decimal? PriceDynamics => PriceMaths.CalculatePriceShift(PriceMean, LatestSnapshot.Price);

        /// <summary>
        /// The lowest priced snaphot of this option. 
        /// </summary>
        public Snapshot LowestSnapshot => Snapshots?.OrderBy(snapshot => snapshot.Price).FirstOrDefault();

        /// <summary>
        /// The latest (= current) snaphot of this option. 
        /// </summary>
        public Snapshot LatestSnapshot => Snapshots?.OrderByDescending(snapshot => snapshot.Timestamp).FirstOrDefault();              

        /// <summary>
        /// Count of all spanshots for this option. 
        /// </summary>
        public int? SnapshotCount => Snapshots?.Count();

        /// <summary>
        /// Local property indication tracking status of an option within specific context.
        /// </summary>
        public bool? IsTracked { get; set; }

        #endregion Tracker Window Props


        #region Constructors
        /// <summary>
        /// Default constructor for automatic reconstitution.
        /// </summary>
        public Option()
        { }

        /// <summary>
        /// Initializes Option with the data from OptionParse object.
        /// </summary>
        public Option(OptionParseResult optionParse)
        {
            Code = optionParse.Code;
            PropertyA = optionParse.PropertyA;
            PropertyB = optionParse.PropertyB;
        }
        #endregion Constructors
    }
}