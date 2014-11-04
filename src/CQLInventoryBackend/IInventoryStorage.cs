/*
    Copyright (c) 2014, InWorldz, LLC
    All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this
      list of conditions and the following disclaimer.

    * Redistributions in binary form must reproduce the above copyright notice,
      this list of conditions and the following disclaimer in the documentation
      and/or other materials provided with the distribution.

    * Neither the name of opensim-cql-inventory nor the names of its
      contributors may be used to endorse or promote products derived from
      this software without specific prior written permission.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
    AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
    CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
    OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
    OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQLInventoryBackend
{
    /// <summary>
    /// Inventory storage interface
    /// </summary>
    public interface IInventoryStorage
    {
        /// <summary>
        /// Returns a copy of all user inventory folders with subfolders and items excluded
        /// </summary>
        /// <returns>A list of all folders that belong to this user</returns>
        List<InventorySkeletonEntry> GetInventorySkeleton(Guid userId);

        /// <summary>
        /// Returns a single entry from the inventory skeleton folder index
        /// </summary>
        /// <returns>The matching skeleton entry</returns>
        InventorySkeletonEntry GetInventorySkeletonEntry(Guid userId, Guid folderId);

        /// <summary>
        /// Returns a full copy of the requested folder including items and sub folder ids
        /// </summary>
        /// <param name="folderId">The ID of the folder to retrieve</param>
        /// <returns>The folder that was found</returns>
        InventoryFolder GetFolder(Guid folderId);

        /// <summary>
        /// Returns a copy of the requested folder's properties. Excludes items and subfolder ids.
        /// </summary>
        /// <param name="folderId">The ID of the folder to retrieve</param>
        /// <returns>The folder that was found</returns>
        InventoryFolder GetFolderAttributes(Guid folderId);

        /// <summary>
        /// Creates a new folder and sets its parent correctly as well as other properties
        /// </summary>
        /// <param name="folder"></param>
        void CreateFolder(InventoryFolder folder);

        /// <summary>
        /// Stores changes made to the base properties of the folder. Can not be used to reassign a new
        /// parent
        /// </summary>
        /// <param name="folder">The folder to save</param>
        void SaveFolder(InventoryFolder folder);

        /// <summary>
        /// Moves the specified folder to the new parent
        /// </summary>
        /// <param name="folder">The folder to move</param>
        /// <param name="oldParent">The ID of the new parent folder</param>
        void MoveFolder(InventorySkeletonEntry folder, Guid newParent);

        /// <summary>
        /// Finds the best root folder to hold the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The best folder to put an object</returns>
        InventorySkeletonEntry FindFolderForType(Guid owner, byte assetType);

        /// <summary>
        /// Purges all subfolders and items from the specified folder
        /// </summary>
        /// <param name="folder">The folder to purge</param>
        void PurgeFolderContents(InventoryFolder folder);

        /// <summary>
        /// Purges all subfolders and items from the specified folder and then removes the folder
        /// </summary>
        /// <param name="folder">The folder to purge</param>
        void PurgeFolder(InventoryFolder folder);

        /// <summary>
        /// Returns an item fetched by the given id
        /// </summary>
        /// <param name="itemId">The item id</param>
        /// <param name="parentFolderHint">An optional hint to the parent folder that contains the item. If unknown pass Guid.Zero</param>
        /// <returns>The item that matches the id, or null if none found</returns>
        InventoryItem GetItem(Guid itemId, Guid parentFolderHint);

        /// <summary>
        /// Creates a new item in the given folder
        /// </summary>
        /// <param name="item">The item to create</param>
        void CreateItem(InventoryItem item);

        /// <summary>
        /// Saves changes that have been made to an item
        /// </summary>
        /// <param name="item">The item to store</param>
        void SaveItem(InventoryItem item);

        /// <summary>
        /// Moves the given item to the given folder
        /// </summary>
        /// <param name="item">The item to move</param>
        /// <param name="parentFolder">The parent folder to move the item into</param>
        void MoveItem(InventoryItem item, InventoryFolder parentFolder);

        /// <summary>
        /// Purges a single item from the inventory
        /// </summary>
        /// <param name="item">The item to purge</param>
        void PurgeItem(InventoryItem item);
    }
}
