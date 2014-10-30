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
            var contactPoints = new string[] {"172.16.166.190"};

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
    }
}
