using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQLInventoryBackend
{
    /// <summary>
    /// The level at which the folder lies in the user inventory.
    /// This casn be used for preventing deletes of important folders 
    /// and stopping the creation of multiple root/top level folders 
    /// </summary>
    public enum FolderLevel
    {
        Root = 1,
        TopLevel = 2,
        Leaf = 3
    }
}
