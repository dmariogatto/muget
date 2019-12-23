using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuGet.Forms.Models;

namespace Muget.Forms.Tests
{
    [TestClass]
    public class PackageVersionTests
    {
        [TestMethod]
        public void IsPrerelease()
        {
            var pack = new PackageVersion("4.4.0.991265-pre1");
            Assert.IsTrue(pack.IsPrerelease);
        }

        [TestMethod]
        public void IsNotPrerelease()
        {
            var pack = new PackageVersion("4.4.0.991265+398-sha.ee1eca51d-azdo.3306453");
            Assert.IsFalse(pack.IsPrerelease);
        }
    }
}
