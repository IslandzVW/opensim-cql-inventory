using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CQLInventoryBackend.Tests
{
    [TestFixture]
    public class InventoryStorageTests
    {
        private CQLInventoryStorage _storage;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var contactPoints = new string[] {"127.0.0.1"};

            CQLInventoryStorage.CreateKeySpaceAndTables(contactPoints);
            _storage = new CQLInventoryStorage(contactPoints);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _storage.Dispose();
        }

        [Test]
        public void TestCreateAndReadFolder()
        {
            InventoryFolder folder = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test",
                OwnerId = Guid.NewGuid(),
                ParentId = Guid.Empty,
                Type = 2
            };

            _storage.CreateFolder(folder);

            var copy = _storage.GetFolderAttributes(folder.FolderId);

            Assert.AreEqual(folder.FolderId, copy.FolderId);
            //note that level is a property of the skel and GetFolderAttributes doesnt load it
            Assert.AreEqual(folder.Name, copy.Name);
            Assert.AreEqual(folder.OwnerId, copy.OwnerId);
            Assert.AreEqual(folder.ParentId, copy.ParentId);
            Assert.AreEqual(folder.Type, copy.Type);

            //get the skel for this user to check the level and version
            var skel = _storage.GetInventorySkeleton(folder.OwnerId);

            Assert.AreEqual(1, skel.Count);
            Assert.AreEqual(1, skel[0].Version);
            Assert.AreEqual(folder.Level, skel[0].Level);
            Assert.AreEqual(folder.Name, skel[0].Name);
            Assert.AreEqual(folder.ParentId, skel[0].ParentId);
            Assert.AreEqual(folder.Type, skel[0].Type);
        }

        [Test]
        public void TestUpdateFolder()
        {
            InventoryFolder folder = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test",
                OwnerId = Guid.NewGuid(),
                ParentId = Guid.NewGuid(),
                Type = 2
            };

            _storage.CreateFolder(folder);

            folder.Name = "Updated";

            _storage.SaveFolder(folder);

            var copy = _storage.GetFolderAttributes(folder.FolderId);

            Assert.AreEqual(folder.FolderId, copy.FolderId);
            //note that level is a property of the skel and GetFolderAttributes doesnt load it
            Assert.AreEqual(folder.Name, copy.Name);
            Assert.AreEqual(folder.OwnerId, copy.OwnerId);
            //note that parent id is a property of the skel and GetFolderAttributes doesnt load it
            Assert.AreEqual(folder.Type, copy.Type);

            //get the skel for this user to check the level, version, and parentage
            var skel = _storage.GetInventorySkeleton(folder.OwnerId);

            Assert.AreEqual(1, skel.Count);
            Assert.AreEqual(2, skel[0].Version);
            Assert.AreEqual(folder.Level, skel[0].Level);
            Assert.AreEqual(folder.Name, skel[0].Name);
            Assert.AreEqual(folder.ParentId, skel[0].ParentId);
            Assert.AreEqual(folder.Type, skel[0].Type);
        }

        [Test]
        public void TestMoveFolder()
        {
            var ownerId = Guid.NewGuid();
            InventoryFolder firstParent = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test1",
                OwnerId = ownerId,
                ParentId = Guid.Empty,
                Type = 2
            };

            InventoryFolder secondParent = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test2",
                OwnerId = ownerId,
                ParentId = Guid.Empty,
                Type = 2
            };

            InventoryFolder folder = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test",
                OwnerId = ownerId,
                ParentId = firstParent.FolderId,
                Type = 2
            };

            _storage.CreateFolder(firstParent);
            _storage.CreateFolder(secondParent);
            _storage.CreateFolder(folder);

            //find the folder in the skel
            var folderSkelEntry = _storage.GetInventorySkeletonEntry(ownerId, folder.FolderId);
            _storage.MoveFolder(folderSkelEntry, secondParent.FolderId);

            //get the skel for this user to check the level, version, and parent
            var skel = _storage.GetInventorySkeleton(folder.OwnerId);
            Dictionary<Guid, InventorySkeletonEntry> entries = new Dictionary<Guid, InventorySkeletonEntry>();
            foreach (var skelentry in skel)
            {
                entries.Add(skelentry.FolderId, skelentry);
            }



            Assert.AreEqual(3, skel.Count);

            Assert.AreEqual(folder.Level, entries[folder.FolderId].Level);
            Assert.AreEqual(secondParent.FolderId, entries[folder.FolderId].ParentId);
            Assert.AreEqual(2, entries[folder.FolderId].Version);
            Assert.AreEqual(folder.Name, entries[folder.FolderId].Name);
            Assert.AreEqual(folder.Type, entries[folder.FolderId].Type);
        }

        [Test]
        public void TestFindFolderForType()
        {
            var owner = Guid.NewGuid();
            InventoryFolder folder1 = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.TopLevel,
                Name = "Test1",
                OwnerId = owner,
                ParentId = Guid.Empty,
                Type = 2
            };
            InventoryFolder folder2 = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.TopLevel,
                Name = "Test2",
                OwnerId = owner,
                ParentId = Guid.Empty,
                Type = 3
            };

            _storage.CreateFolder(folder1);
            _storage.CreateFolder(folder2);

            Assert.AreEqual(folder1.FolderId, _storage.FindFolderForType(owner, 2).FolderId);
        }

        [Test]
        public void TestWriteItemToFolder()
        {
            InventoryFolder folder = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test",
                OwnerId = Guid.NewGuid(),
                ParentId = Guid.Empty,
                Type = 2
            };

            _storage.CreateFolder(folder);

            InventoryItem item = new InventoryItem
            {
                AssetId = Guid.NewGuid(),
                AssetType = 11,
                BasePermissions = 3,
                CreationDate = 4,
                CreatorId = Guid.NewGuid(),
                CurrentPermissions = 5,
                Description = "Description",
                EveryonePermissions = 6,
                Flags = 7,
                FolderId = folder.FolderId,
                GroupId = Guid.NewGuid(),
                GroupOwned = true,
                GroupPermissions = 8,
                InventoryType = 9,
                ItemId = Guid.NewGuid(),
                Name = "Name",
                NextPermissions = int.MaxValue,
                OwnerId = folder.OwnerId,
                SaleType = 10
            };

            _storage.CreateItem(item);

            folder = _storage.GetFolder(folder.FolderId);
            var skelEntry = _storage.GetInventorySkeletonEntry(folder.OwnerId, folder.FolderId);

            Assert.AreEqual(2, skelEntry.Version);
            Assert.AreEqual(1, folder.Items.Count);

            AssertItemEqual(item, folder.Items[0]);

            var foundItem = _storage.GetItem(item.ItemId, Guid.Empty);

            AssertItemEqual(item, foundItem);
        }

        private void AssertItemEqual(InventoryItem a, InventoryItem b)
        {
            Assert.AreEqual(a.AssetId, b.AssetId);
            Assert.AreEqual(a.AssetType, b.AssetType);
            Assert.AreEqual(a.BasePermissions, b.BasePermissions);
            Assert.AreEqual(a.CreationDate, b.CreationDate);
            Assert.AreEqual(a.CreatorId, b.CreatorId);
            Assert.AreEqual(a.CurrentPermissions, b.CurrentPermissions);
            Assert.AreEqual(a.Description, b.Description);
            Assert.AreEqual(a.EveryonePermissions, b.EveryonePermissions);
            Assert.AreEqual(a.Flags, b.Flags);
            Assert.AreEqual(a.FolderId, b.FolderId);
            Assert.AreEqual(a.GroupId, b.GroupId);
            Assert.AreEqual(a.GroupOwned, b.GroupOwned);
            Assert.AreEqual(a.GroupPermissions, b.GroupPermissions);
            Assert.AreEqual(a.InventoryType, b.InventoryType);
            Assert.AreEqual(a.ItemId, b.ItemId);
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.NextPermissions, b.NextPermissions);
            Assert.AreEqual(a.OwnerId, b.OwnerId);
            Assert.AreEqual(a.SaleType, b.SaleType);
        }

        [Test]
        public void TestSaveItem()
        {
            InventoryFolder folder = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test",
                OwnerId = Guid.NewGuid(),
                ParentId = Guid.Empty,
                Type = 2
            };

            _storage.CreateFolder(folder);

            InventoryItem item = new InventoryItem
            {
                AssetId = Guid.NewGuid(),
                AssetType = 11,
                BasePermissions = 3,
                CreationDate = 4,
                CreatorId = Guid.NewGuid(),
                CurrentPermissions = 5,
                Description = "Description",
                EveryonePermissions = 6,
                Flags = 7,
                FolderId = folder.FolderId,
                GroupId = Guid.NewGuid(),
                GroupOwned = true,
                GroupPermissions = 8,
                InventoryType = 9,
                ItemId = Guid.NewGuid(),
                Name = "Name",
                NextPermissions = int.MaxValue,
                OwnerId = folder.OwnerId,
                SaleType = 10
            };

            _storage.CreateItem(item);

            item = new InventoryItem
            {
                AssetId = Guid.NewGuid(),
                AssetType = 12,
                BasePermissions = 4,
                CreationDate = 5,
                CreatorId = Guid.NewGuid(),
                CurrentPermissions = 6,
                Description = "Description1",
                EveryonePermissions = 7,
                Flags = 8,
                FolderId = folder.FolderId,
                GroupId = Guid.NewGuid(),
                GroupOwned = false,
                GroupPermissions = 9,
                InventoryType = 10,
                ItemId = item.ItemId,
                Name = "Name1",
                NextPermissions = int.MinValue,
                OwnerId = folder.OwnerId,
                SaleType = 11
            };

            _storage.SaveItem(item);

            folder = _storage.GetFolder(folder.FolderId);
            var skelEntry = _storage.GetInventorySkeletonEntry(folder.OwnerId, folder.FolderId);

            Assert.AreEqual(3, skelEntry.Version);
            Assert.AreEqual(1, folder.Items.Count);

            AssertItemEqual(item, folder.Items[0]);
        }

        [Test]
        public void TestMoveItem()
        {
            InventoryFolder folder = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test",
                OwnerId = Guid.NewGuid(),
                ParentId = Guid.Empty,
                Type = 2
            };

            InventoryFolder folder1 = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test1",
                OwnerId = folder.OwnerId,
                ParentId = Guid.Empty,
                Type = 2
            };

            _storage.CreateFolder(folder);
            _storage.CreateFolder(folder1);

            InventoryItem item = new InventoryItem
            {
                AssetId = Guid.NewGuid(),
                AssetType = 11,
                BasePermissions = 3,
                CreationDate = 4,
                CreatorId = Guid.NewGuid(),
                CurrentPermissions = 5,
                Description = "Description",
                EveryonePermissions = 6,
                Flags = 7,
                FolderId = folder.FolderId,
                GroupId = Guid.NewGuid(),
                GroupOwned = true,
                GroupPermissions = 8,
                InventoryType = 9,
                ItemId = Guid.NewGuid(),
                Name = "Name",
                NextPermissions = int.MaxValue,
                OwnerId = folder.OwnerId,
                SaleType = 10
            };

            _storage.CreateItem(item);
            _storage.MoveItem(item, folder1);
            item.FolderId = folder1.FolderId;

            folder1 = _storage.GetFolder(folder1.FolderId);
            var skelEntry = _storage.GetInventorySkeletonEntry(folder1.OwnerId, folder1.FolderId);

            Assert.AreEqual(2, skelEntry.Version);
            Assert.AreEqual(1, folder1.Items.Count);

            AssertItemEqual(item, folder1.Items[0]);

            folder = _storage.GetFolder(folder.FolderId);
            skelEntry = _storage.GetInventorySkeletonEntry(folder.OwnerId, folder.FolderId);

            Assert.AreEqual(3, skelEntry.Version);
            Assert.AreEqual(0, folder.Items.Count);

            var foundItem = _storage.GetItem(item.ItemId, Guid.Empty);

            AssertItemEqual(item, foundItem);
        }

        [Test]
        public void TestPurgeItem()
        {
            InventoryFolder folder = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test",
                OwnerId = Guid.NewGuid(),
                ParentId = Guid.Empty,
                Type = 2
            };
            _storage.CreateFolder(folder);

            InventoryItem item = new InventoryItem
            {
                AssetId = Guid.NewGuid(),
                AssetType = 11,
                BasePermissions = 3,
                CreationDate = 4,
                CreatorId = Guid.NewGuid(),
                CurrentPermissions = 5,
                Description = "Description",
                EveryonePermissions = 6,
                Flags = 7,
                FolderId = folder.FolderId,
                GroupId = Guid.NewGuid(),
                GroupOwned = true,
                GroupPermissions = 8,
                InventoryType = 9,
                ItemId = Guid.NewGuid(),
                Name = "Name",
                NextPermissions = int.MaxValue,
                OwnerId = folder.OwnerId,
                SaleType = 10
            };

            _storage.CreateItem(item);
            _storage.PurgeItem(item);

            folder = _storage.GetFolder(folder.FolderId);
            var skelEntry = _storage.GetInventorySkeletonEntry(folder.OwnerId, folder.FolderId);

            Assert.AreEqual(3, skelEntry.Version);
            Assert.AreEqual(0, folder.Items.Count);

            var foundItem = _storage.GetItem(item.ItemId, Guid.Empty);
            Assert.Null(foundItem);
        }

        [Test]
        public void TestPurgeEmptyFolder()
        {
            InventoryFolder folder = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test",
                OwnerId = Guid.NewGuid(),
                ParentId = Guid.Empty,
                Type = 2
            };
            
            _storage.CreateFolder(folder);
            _storage.PurgeFolder(folder);

            var newfolder = _storage.GetFolder(folder.FolderId);
            var skelEntry = _storage.GetInventorySkeletonEntry(folder.OwnerId, folder.FolderId);

            Assert.Null(newfolder);
            Assert.Null(skelEntry);
        }

        [Test]
        public void TestPurgeNonemptyFolderContents()
        {
            var ownerId = Guid.NewGuid();

            InventoryFolder parent = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.Root,
                Name = "Test",
                OwnerId = ownerId,
                ParentId = Guid.Empty,
                Type = 2
            };

            InventoryFolder child = new InventoryFolder
            {
                FolderId = Guid.NewGuid(),
                Level = FolderLevel.TopLevel,
                Name = "Test",
                OwnerId = ownerId,
                ParentId = parent.FolderId,
                Type = 2
            };

            _storage.CreateFolder(parent);
            _storage.CreateFolder(child);

            InventoryItem item = new InventoryItem
            {
                AssetId = Guid.NewGuid(),
                AssetType = 11,
                BasePermissions = 3,
                CreationDate = 4,
                CreatorId = Guid.NewGuid(),
                CurrentPermissions = 5,
                Description = "Description",
                EveryonePermissions = 6,
                Flags = 7,
                FolderId = child.FolderId,
                GroupId = Guid.NewGuid(),
                GroupOwned = true,
                GroupPermissions = 8,
                InventoryType = 9,
                ItemId = Guid.NewGuid(),
                Name = "Name",
                NextPermissions = int.MaxValue,
                OwnerId = ownerId,
                SaleType = 10
            };

            InventoryItem item2 = new InventoryItem
            {
                AssetId = Guid.NewGuid(),
                AssetType = 11,
                BasePermissions = 3,
                CreationDate = 4,
                CreatorId = Guid.NewGuid(),
                CurrentPermissions = 5,
                Description = "Description",
                EveryonePermissions = 6,
                Flags = 7,
                FolderId = parent.FolderId,
                GroupId = Guid.NewGuid(),
                GroupOwned = true,
                GroupPermissions = 8,
                InventoryType = 9,
                ItemId = Guid.NewGuid(),
                Name = "Name",
                NextPermissions = int.MaxValue,
                OwnerId = ownerId,
                SaleType = 10
            };

            _storage.CreateItem(item);
            _storage.CreateItem(item2);

            _storage.PurgeFolderContents(parent);

            Assert.Null(_storage.GetFolder(child.FolderId));
            Assert.Null(_storage.GetInventorySkeletonEntry(parent.OwnerId, child.FolderId));
            Assert.Null(_storage.GetItem(item.ItemId, Guid.Empty));
            Assert.Null(_storage.GetItem(item2.ItemId, Guid.Empty));
        }
    }
}
