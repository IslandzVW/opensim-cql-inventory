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
            _storage = new CQLInventoryStorage(new string[] {"172.16.166.189"});
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _storage.Dispose();
        }
    }
}
