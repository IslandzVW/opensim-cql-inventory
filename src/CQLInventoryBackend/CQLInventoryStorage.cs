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
using System.Threading.Tasks;
using Cassandra;
using System.IO;
using System.Text.RegularExpressions;

namespace CQLInventoryBackend
{
    class CQLInventoryStorage : IInventoryStorage, IDisposable
    {
        private Cluster _cluster;
        private ISession _session;

        /// <summary>
        /// The name of the keyspace where we will store all inventory data
        /// </summary>
        private const string KEYSPACE_NAME = "OpensimInventory";

        /// <summary>
        /// This magic item ID is used to store the folder data
        /// </summary>
        private readonly Guid FOLDER_MAGIC_ENTRY = Guid.Parse("00000000-0000-0000-0000-000000000001");

        private readonly PreparedStatement SKEL_SELECT_STMT;
        private readonly PreparedStatement SKEL_SINGLE_SELECT_STMT;
        private readonly PreparedStatement SKEL_INSERT_STMT;
        private readonly PreparedStatement FOLDER_SELECT_STMT;
        private readonly PreparedStatement FOLDER_ATTRIB_SELECT_STMT;
        private readonly PreparedStatement FOLDER_ATTRIB_INSERT_STMT;
        private readonly PreparedStatement FOLDER_ITEM_INSERT_STMT;
        private readonly PreparedStatement FOLDER_ITEM_UPDATE_STMT;
        private readonly PreparedStatement FOLDER_VERSION_INC_STMT;
        private readonly PreparedStatement FOLDER_VERSION_SELECT_STMT;
        private readonly PreparedStatement FOLDER_VERSION_SINGLE_SELECT_STMT;
        private readonly PreparedStatement FOLDER_UPDATE_STMT;
        private readonly PreparedStatement SKEL_UPDATE_STMT;
        private readonly PreparedStatement SKEL_MOVE_STMT;


        public CQLInventoryStorage(string[] contactPoints)
        {
            _cluster = Cluster.Builder().AddContactPoints(contactPoints).Build();
            _session = _cluster.Connect(KEYSPACE_NAME);

            
            SKEL_SELECT_STMT = _session.Prepare("SELECT * FROM skeletons WHERE user_id = ?;");
            SKEL_SELECT_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            SKEL_SINGLE_SELECT_STMT = _session.Prepare("SELECT * FROM skeletons WHERE user_id = ? AND folder_id = ?;");
            SKEL_SINGLE_SELECT_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);

            
            SKEL_INSERT_STMT 
                = _session.Prepare( "INSERT INTO skeletons (user_id, folder_id, folder_name, parent_id, type, level) " +
                                    "VALUES(?, ?, ?, ?, ?, ?);");
            SKEL_INSERT_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);

            
            FOLDER_SELECT_STMT = _session.Prepare("SELECT * FROM folder_contents WHERE folder_id = ?;");
            FOLDER_SELECT_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            FOLDER_ATTRIB_SELECT_STMT = _session.Prepare(   "SELECT * FROM folder_contents WHERE folder_id = ? AND item_id = " 
                                                            + FOLDER_MAGIC_ENTRY.ToString() + ";");
            FOLDER_ATTRIB_SELECT_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            FOLDER_ATTRIB_INSERT_STMT
                = _session.Prepare( "INSERT INTO folder_contents (folder_id, item_id, name, inv_type, creation_date, owner_id) " +
                                    "VALUES (?, ?, ?, ?, ?, ?);");
            FOLDER_ATTRIB_INSERT_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            FOLDER_ITEM_INSERT_STMT
                = _session.Prepare( "INSERT INTO folder_contents (folder_id, item_id, name, asset_id, asset_type, " +
                                        "base_permissions, creation_date, creator_id, current_permissions, description, " +
                                        "everyone_permissions, flags, group_id, group_owned, group_permissions, " +
                                        "inv_type, next_permissions, owner_id, sale_type) " +
                                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);");
            FOLDER_ITEM_INSERT_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            FOLDER_ITEM_UPDATE_STMT
                = _session.Prepare("UPDATE folder_contents SET name = ?, asset_id = ?, asset_type = ?, " +
                                        "base_permissions = ?, creation_date = ?, creator_id = ?, current_permissions= ?, description = ?, " +
                                        "everyone_permissions = ?, flags = ?, group_id = ?, group_owned = ?, group_permissions = ?, " +
                                        "inv_type = ?, next_permissions = ?, sale_type = ? " +
                                    "WHERE folder_id = ? AND item_id = ?;");
            FOLDER_ITEM_UPDATE_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            FOLDER_VERSION_INC_STMT
                = _session.Prepare("UPDATE folder_versions SET version = version + 1 WHERE user_id = ? AND folder_id = ?;");
            FOLDER_VERSION_INC_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            FOLDER_VERSION_SELECT_STMT = _session.Prepare("SELECT * FROM folder_versions WHERE user_id = ?;");
            FOLDER_VERSION_SELECT_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            FOLDER_VERSION_SINGLE_SELECT_STMT = _session.Prepare("SELECT * FROM folder_versions WHERE user_id = ? AND folder_id = ?;");
            FOLDER_VERSION_SINGLE_SELECT_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            FOLDER_UPDATE_STMT = _session.Prepare(  "UPDATE folder_contents SET name = ?, inv_type = ? " +
                                                    "WHERE folder_id = ? AND item_id = " + FOLDER_MAGIC_ENTRY.ToString());
            FOLDER_UPDATE_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            SKEL_UPDATE_STMT = _session.Prepare("UPDATE skeletons SET folder_name = ?, type = ? WHERE user_id = ? AND folder_id = ?");
            SKEL_UPDATE_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);


            SKEL_MOVE_STMT = _session.Prepare("UPDATE skeletons SET parent_id = ? WHERE user_id = ? AND folder_id = ?");
            SKEL_MOVE_STMT.SetConsistencyLevel(ConsistencyLevel.Quorum);
        }

        /// <summary>
        /// Creates the keyspace and tables
        /// </summary>
        public static void CreateKeySpaceAndTables(string[] contactPoints)
        {
            var cluster = Cluster.Builder().AddContactPoints(contactPoints).Build();
            var session = cluster.Connect();

            session.CreateKeyspaceIfNotExists(KEYSPACE_NAME, new Dictionary<string, string> { { "class", "SimpleStrategy" }, { "replication_factor", "3" } });
            session.ChangeKeyspace(KEYSPACE_NAME);

            string[] statements = ProcessSchemaFile();

            foreach (var statement in statements)
            {
                var stmt = statement.Trim().TrimEnd('\r', '\n');

                if (stmt != "")
                {
                    Console.Out.WriteLine("running: " + stmt);
                    session.Execute(new SimpleStatement(stmt));
                }
            }
        }

        /// <summary>
        /// Loads the schema file from disk and removes comments then splits into statements
        /// by looking for semi colons
        /// </summary>
        /// <returns></returns>
        private static string[] ProcessSchemaFile()
        {
            string contents = File.ReadAllText("schema.cql");
            StringBuilder withoutComments = new StringBuilder();

            //filter comments
            bool skipping = false;
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i] == '/' && contents[i + 1] == '*')
                {
                    skipping = true;
                }
                else if (contents[i] == '*' && contents[i+1] == '/')
                {
                    i += 2;
                    skipping = false;
                    continue;
                }

                if (! skipping)
                {
                    withoutComments.Append(contents[i]);
                }
            }

            return withoutComments.ToString().Split(';');
        }

        public static int UnixTimeNow()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public List<InventorySkeletonEntry> GetInventorySkeleton(Guid userId)
        {
            var statement = SKEL_SELECT_STMT.Bind(userId);
            var rowset = _session.Execute(statement);

            var retList = new Dictionary<Guid, InventorySkeletonEntry>();
            foreach (var row in rowset)
            {
                retList.Add(row.GetValue<Guid>("folder_id"), MapRowToSkeletonItem(row));
            }

            //look up the versions
            statement = FOLDER_VERSION_SELECT_STMT.Bind(userId);
            rowset = _session.Execute(statement);

            foreach (var row in rowset)
            {
                var folderId = row.GetValue<Guid>("folder_id");

                if (retList.ContainsKey(folderId))
                {
                    retList[folderId].Version = row.GetValue<long>("version");
                }
            }

            return new List<InventorySkeletonEntry>(retList.Values);
        }

        private static InventorySkeletonEntry MapRowToSkeletonItem(Row row)
        {
            return new InventorySkeletonEntry
            {
                UserId = row.GetValue<Guid>("user_id"),
                FolderId = row.GetValue<Guid>("folder_id"),
                Name = row.GetValue<string>("folder_name"),
                ParentId = row.GetValue<Guid>("parent_id"),
                Type = (byte)row.GetValue<int>("type"),
                Level = (FolderLevel)row.GetValue<int>("level")
            };
        }

        public InventorySkeletonEntry GetInventorySkeletonEntry(Guid userId, Guid folderId)
        {
            var statement = SKEL_SINGLE_SELECT_STMT.Bind(userId, folderId);
            var rowset = _session.Execute(statement);

            InventorySkeletonEntry entry = null;
            foreach (var row in rowset)
            {
                //should only be one
                entry = MapRowToSkeletonItem(row);
                break;
            }

            if (entry != null)
            {
                //look up the version
                statement = FOLDER_VERSION_SINGLE_SELECT_STMT.Bind(userId, folderId);
                rowset = _session.Execute(statement);

                foreach (var row in rowset)
                {
                    //should only be one 
                    entry.Version = row.GetValue<long>("version");
                }
            }

            return entry;
        }

        public InventoryFolder GetFolder(Guid folderId)
        {
            var statement = FOLDER_SELECT_STMT.Bind(folderId);
            var rowset = _session.Execute(statement);
            
            var itemList = new List<InventoryItem>();
            InventoryFolder retFolder = null;
            foreach (var row in rowset)
            {
                if (row.GetValue<Guid>("item_id") == FOLDER_MAGIC_ENTRY)
                {
                    retFolder = new InventoryFolder { FolderId = folderId };
                    //this is the data row that holds the information for the folder itself
                    MapRowToFolder(retFolder, row);
                }
                else
                {
                    itemList.Add(MapRowToItem(row));
                }
            }

            retFolder.Items = itemList;
            return retFolder;
        }

        private static InventoryItem MapRowToItem(Row row)
        {
            return new InventoryItem
            {
                AssetId = row.GetValue<Guid>("asset_id"),
                AssetType = row.GetValue<int>("asset_type"),
                BasePermissions = row.GetValue<int>("base_permissions"),
                CreationDate = row.GetValue<int>("creation_date"),
                CreatorId = row.GetValue<Guid>("creator_id"),
                CurrentPermissions = row.GetValue<int>("current_permissions"),
                Description = row.GetValue<string>("description"),
                EveryonePermissions = row.GetValue<int>("everyone_permissions"),
                Flags = row.GetValue<int>("flags"),
                FolderId = row.GetValue<Guid>("folder_id"),
                GroupId = row.GetValue<Guid>("group_id"),
                GroupOwned = row.GetValue<bool>("group_owned"),
                GroupPermissions = row.GetValue<int>("group_permissions"),
                InventoryType = row.GetValue<int>("inv_type"),
                ItemId = row.GetValue<Guid>("item_id"),
                Name = row.GetValue<string>("name"),
                NextPermissions = row.GetValue<int>("next_permissions"),
                OwnerId = row.GetValue<Guid>("owner_id"),
                SaleType = row.GetValue<int>("sale_type")
            };
        }

        private static void MapRowToFolder(InventoryFolder retFolder, Row row)
        {
            retFolder.CreationDate = row.GetValue<int>("creation_date");
            retFolder.Name = row.GetValue<string>("name");
            retFolder.OwnerId = row.GetValue<Guid>("owner_id");
            retFolder.Type = row.GetValue<int>("inv_type");
        }

        public InventoryFolder GetFolderAttributes(Guid folderId)
        {
            var statement = FOLDER_SELECT_STMT.Bind(folderId);
            var rowset = _session.Execute(statement);

            InventoryFolder retFolder = null;

            foreach (var row in rowset)
            {
                retFolder = new InventoryFolder { FolderId = folderId };
                //should only be a single row
                MapRowToFolder(retFolder, row);
                break;
            }

            return retFolder;
        }

        public void CreateFolder(InventoryFolder folder)
        {
            var skelInsert = SKEL_INSERT_STMT.Bind(folder.OwnerId, folder.FolderId, folder.Name, folder.ParentId, folder.Type, (int)folder.Level);
            var contentInsert = FOLDER_ATTRIB_INSERT_STMT.Bind(folder.FolderId, FOLDER_MAGIC_ENTRY, folder.Name, folder.Type, UnixTimeNow(), folder.OwnerId);

            var batch = new BatchStatement()
                .Add(skelInsert)
                .Add(contentInsert);

            _session.Execute(batch);

            VersionInc(folder.OwnerId, folder.FolderId);
        }

        private void VersionInc(Guid ownerId, Guid folderId)
        {
            var versionInc = FOLDER_VERSION_INC_STMT.Bind(ownerId, folderId);
            _session.Execute(versionInc);
        }

        public void SaveFolder(InventoryFolder folder)
        {
            var skelUpdate = SKEL_UPDATE_STMT.Bind(folder.Name, folder.Type, folder.OwnerId, folder.FolderId);
            var contentUpdate = FOLDER_UPDATE_STMT.Bind(folder.Name, folder.Type, folder.FolderId);

            var batch = new BatchStatement()
                .Add(skelUpdate)
                .Add(contentUpdate);

            _session.Execute(batch);

            VersionInc(folder.OwnerId, folder.FolderId);
        }

        public void MoveFolder(InventorySkeletonEntry folder, Guid newParent)
        {
            var skelMove = SKEL_MOVE_STMT.Bind(newParent, folder.UserId, folder.FolderId);
            _session.Execute(skelMove);

            VersionInc(folder.UserId, folder.FolderId);
            VersionInc(folder.UserId, folder.ParentId);
            VersionInc(folder.UserId, newParent);
        }

        public InventorySkeletonEntry FindFolderForType(Guid owner, byte assetType)
        {
            //get the skel, search for the type
            var skel = this.GetInventorySkeleton(owner);
            foreach (var entry in skel)
            {
                if (entry.Level == FolderLevel.TopLevel || entry.Level == FolderLevel.Root)
                {
                    if (entry.Type == assetType)
                    {
                        return entry;
                    }
                }
            }

            return null;
        }

        public void PurgeFolderContents(InventoryFolder folder)
        {

        }

        public void PurgeFolder(InventoryFolder folder)
        {
            throw new NotImplementedException();
        }

        public void PurgeFolders(IEnumerable<InventoryFolder> folders)
        {
            throw new NotImplementedException();
        }

        public InventoryItem GetItem(Guid itemId, Guid parentFolderHint)
        {
            throw new NotImplementedException();
        }

        public void CreateItem(InventoryItem item)
        {
            var statement = FOLDER_ITEM_INSERT_STMT.Bind(item.FolderId, item.ItemId, item.Name, item.AssetId, item.AssetType,
                item.BasePermissions, item.CreationDate, item.CreatorId, item.CurrentPermissions, item.Description, item.EveryonePermissions,
                item.Flags, item.GroupId, item.GroupOwned, item.GroupPermissions, item.InventoryType, item.NextPermissions, 
                item.OwnerId, item.SaleType);

            _session.Execute(statement);

            VersionInc(item.OwnerId, item.FolderId);
        }

        public void SaveItem(InventoryItem item)
        {
            var statement = FOLDER_ITEM_UPDATE_STMT.Bind(item.Name, item.AssetId, item.AssetType, item.BasePermissions, item.CreationDate, item.CreatorId,
                item.CurrentPermissions, item.Description, item.EveryonePermissions, item.Flags, item.GroupId, item.GroupOwned, item.GroupPermissions,
                item.InventoryType, item.NextPermissions, item.SaleType, item.FolderId, item.ItemId);

            _session.Execute(statement);
            VersionInc(item.OwnerId, item.FolderId);
        }

        public void MoveItem(InventoryItem item, InventoryFolder parentFolder)
        {
            throw new NotImplementedException();
        }

        public void PurgeItem(InventoryItem item)
        {
            throw new NotImplementedException();
        }

        public void PurgeItems(IEnumerable<InventoryItem> items)
        {
            throw new NotImplementedException();
        }
    
        public void Dispose()
        {
            _session.Dispose();
            _cluster.Dispose();
        }
    }
}
