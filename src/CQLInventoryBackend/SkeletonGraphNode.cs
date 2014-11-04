using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQLInventoryBackend
{
    /// <summary>
    /// Used to form a heirarchy from the inventory skeleton
    /// </summary>
    public class SkeletonGraphNode
    {
        public bool RootNode;
        public InventorySkeletonEntry Self;
        public SkeletonGraphNode Parent;
        public List<SkeletonGraphNode> DirectChildren;

        //only for the root node
        public Dictionary<Guid, SkeletonGraphNode> AllChildren;

        public SkeletonGraphNode()
        {
        }

        /// <summary>
        /// Builds a graph from the skeleton
        /// </summary>
        /// <param name="skel"></param>
        public SkeletonGraphNode(IEnumerable<InventorySkeletonEntry> skel)
        {
            Dictionary<Guid, List<InventorySkeletonEntry>> nodesByParent = new Dictionary<Guid, List<InventorySkeletonEntry>>();

            foreach (var node in skel)
            {
                if (node.Level == FolderLevel.Root)
                {
                    this.RootNode = true;
                    this.Self = node;
                    this.Parent = null;
                }

                List<InventorySkeletonEntry> childList;
                if (! nodesByParent.TryGetValue(node.ParentId, out childList))
                {
                    childList = new List<InventorySkeletonEntry>();
                    nodesByParent[node.ParentId] = childList;
                }

                childList.Add(node);
            }

            this.AllChildren = new Dictionary<Guid, SkeletonGraphNode>();

            //start at the root and work down
            this.RecursiveBuild(nodesByParent, this.AllChildren); 
        }

        private void RecursiveBuild(Dictionary<Guid, List<InventorySkeletonEntry>> nodesByParent, Dictionary<Guid, SkeletonGraphNode> allChildren)
        {
            allChildren.Add(this.Self.FolderId, this);

            //find my direct children
            this.DirectChildren = new List<SkeletonGraphNode>();

            List<InventorySkeletonEntry> children;
            if (nodesByParent.TryGetValue(this.Self.FolderId, out children))
            {
                foreach (var child in children)
                {
                    var newChild = new SkeletonGraphNode { Self = child, Parent = this, RootNode = false };
                    this.DirectChildren.Add(newChild);
                    newChild.RecursiveBuild(nodesByParent, allChildren);
                }
            }
        }
    }
}
