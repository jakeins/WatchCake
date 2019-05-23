using System.Collections.Generic;

namespace WatchCake.Services.Molder
{
    /// <summary>
    /// External Molder type, contains single molding (reshaping) instructions.
    /// </summary>
    public class Mold
    {
        /// <summary>
        /// Type of the modling mechanism.
        /// </summary>
        public MoldType Type { get; set; }

        /// <summary>
        /// List of attributes to be used with the Mold.
        /// </summary>
        public IList<string> Attributes { get; set; }


        /// <summary>
        /// Mold constructor. Pass as many attrbutes as needed for the specified Mold Type.
        /// </summary>
        public Mold(MoldType type, object attribute1 = null, object attribute2 = null, object attribute3 = null, object attribute4 = null)
        {
            Type = type;

            if (attribute1 == null)
                return;

            Attributes = new List<string>();

            foreach (var attr in new[] { attribute1, attribute2, attribute3, attribute4 })
            {
                if (attr == null)
                    return;
                else
                    Attributes.Add(attr.ToString());
            }                
        }
    }
}