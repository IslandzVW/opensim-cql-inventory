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
        public Guid UserId { get; set; }
        public Guid FolderId { get; set; }
        public string Name { get; set; }
        public Guid ParentId { get; set; }
        public int Type { get; set; }
        public FolderLevel Level { get; set; }
    }
}
