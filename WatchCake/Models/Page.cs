using System.ComponentModel.DataAnnotations.Schema;
using WatchCake.Helpers;
using WatchCake.Models.Bases;

namespace WatchCake.Models
{
    /// <summary>
    /// Clean representation of the product page data.
    /// </summary>
    public class Page : IdObject
    {
        private string _title;
        private string _relativeUri;

        [NotMapped]
        private Shop _parent;

        /// <summary>
        /// An ID of a parent this entity belongs to.
        /// </summary>
        public int? ParentShopID { get; private set; }

        /// <summary>
        /// A parent this entity belongs to. Setter also sets ParentID.
        /// </summary>
        [NotMapped]
        public Shop ParentShop
        {
            get => _parent;
            set
            {
                _parent = value;
                ParentShopID = value.ID;
            }
        }

        /// <summary>
        /// Relative URI of the current page.
        /// </summary>
        public string RelativeUri
        {
            get => _relativeUri;
            set => _relativeUri = value.StripLeadingSlashes();
        }

        /// <summary>
        /// Page uri with the Shop prefix.
        /// </summary>
        public string FullUri => ParentShop.Domain.ToString().SlashSafeUriConcat(RelativeUri);

        /// <summary>
        /// Exttracted title of the page.
        /// </summary>
        public string Title { get => _title; set { _title = value; RaisePropertyChanged(); } }

        /// <summary>
        /// Product thumbnail path.
        /// </summary>
        public string ThumbPath { get; set; }

        /// <summary>
        /// Local property indication tracking status of a page within specific context.
        /// </summary>
        public bool? IsTracked { get; set; }

        /// <summary>
        /// Local property for mere indication count of included options.
        /// </summary>
        [NotMapped]
        public int? OptionCount { get; set; }
    }
}