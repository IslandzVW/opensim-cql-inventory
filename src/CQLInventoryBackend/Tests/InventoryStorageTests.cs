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

            Assert.AreEqual(item.AssetId, folder.Items[0].AssetId);
            Assert.AreEqual(item.AssetType, folder.Items[0].AssetType);
            Assert.AreEqual(item.BasePermissions, folder.Items[0].BasePermissions);
            Assert.AreEqual(item.CreationDate, folder.Items[0].CreationDate);
            Assert.AreEqual(item.CreatorId, folder.Items[0].CreatorId);
            Assert.AreEqual(item.CurrentPermissions, folder.Items[0].CurrentPermissions);
            Assert.AreEqual(item.Description, folder.Items[0].Description);
            Assert.AreEqual(item.EveryonePermissions, folder.Items[0].EveryonePermissions);
            Assert.AreEqual(item.Flags, folder.Items[0].Flags);
            Assert.AreEqual(item.FolderId, folder.Items[0].FolderId);
            Assert.AreEqual(item.GroupId, folder.Items[0].GroupId);
            Assert.AreEqual(item.GroupOwned, folder.Items[0].GroupOwned);
            Assert.AreEqual(item.GroupPermissions, folder.Items[0].GroupPermissions);
            Assert.AreEqual(item.InventoryType, folder.Items[0].InventoryType);
            Assert.AreEqual(item.ItemId, folder.Items[0].ItemId);
            Assert.AreEqual(item.Name, folder.Items[0].Name);
            Assert.AreEqual(item.NextPermissions, folder.Items[0].NextPermissions);
            Assert.AreEqual(item.OwnerId, folder.Items[0].OwnerId);
            Assert.AreEqual(item.SaleType, folder.Items[0].SaleType);
        }
    }
}
