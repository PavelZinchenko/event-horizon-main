using System.Linq;
using NUnit.Framework;
using Utilites.Collections;

namespace Utilites.Tests
{
    [TestFixture]
    public class CollectionsTests
    {
        [Test]
        public void SecureListTest()
        {
            var list = new SecureList<int>();

            for (int i = 0; i < 10; ++i)
                list.Add(i);

            for (int i = 0; i < 10; ++i)
                list[i]++;

            list.Remove(7);
            list.Remove(3);

            list.Insert(5, 10);

            Assert.AreEqual(list.Count, 9);
            Assert.IsTrue(list.SequenceEqual(new int[]{1,2,4,5,6,10,8,9,10}));
        }

        [Test]
        public void SecureInventoryTest()
        {
            var inventory = new SecureInventory<int>();

            inventory.Add(1, 3);
            inventory.Add(2, 2);
            inventory.Add(3, 1);

            Assert.AreEqual(inventory.GetQuantity(2), 2);
            Assert.AreEqual(inventory.GetQuantity(3), 1);

            inventory.Add(3, 5);

            Assert.AreEqual(inventory.Remove(2, 1), 1);
            Assert.AreEqual(inventory.GetQuantity(2), 1);
            Assert.AreEqual(inventory.Remove(2, 5), 1);
            Assert.AreEqual(inventory.GetQuantity(2), 0);

            inventory.Add(2, 1);

            Assert.AreEqual(inventory.GetQuantity(2), 1);
            Assert.AreEqual(inventory.GetQuantity(0), 0);
            Assert.AreEqual(inventory.GetQuantity(1), 3);
            Assert.AreEqual(inventory.GetQuantity(3), 6);
        }
    }
}
