using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQLInventoryBackend
{
    /// <summary>
    /// An entry in the user's folder skeleton. A partial folder containing
    /// the name of the folder, its asset type, and its parent ID
    /// </summary>
    public class InventorySkeletonEntry
    {
        /// <summary>
        /// The level at which the folder lies in the user inventory.
        /// This casn be used for preventing deletes of important folders 
        /// and stopping the creation of multiple root/top level folders 
        /// </summary>
        public enum Level
        {
            Root = 1,
            TopLevel = 2,
            Leaf = 3
        }

        public Guid UserId { get; set; }
        public Guid FolderId { get; set; }
        public string Name { get; set; }
        public Guid ParentId { get; set; }
        public int Type { get; set; }
        public Level FolderLevel { get; set; }
    }
}
