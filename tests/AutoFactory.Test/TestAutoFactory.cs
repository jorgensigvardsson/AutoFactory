using NUnit.Framework;

namespace AutoFactory.Test
{
    [TestFixture]
    public class TestAutoFactory
    {
        [Test]
        public void Test()
        {
            var subservice = new SubService();
            IServiceFactory serviceFactory = new ServiceFactory(subservice);
            var obj = serviceFactory.Create("test", 1, 2);

            Assert.AreSame(subservice, obj.Subservice);
            Assert.AreEqual("test", obj.ServerName);
            Assert.AreEqual(1, obj.X);
            Assert.AreEqual(2, obj.Y);
        }
    }
}
