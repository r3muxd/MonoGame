using NUnit.Framework;

namespace MonoGame.Tests.Utilities
{
    public class SetWorkingDirFixture
    {
        [SetUp]
        public void SetUp()
        {
            Paths.SetStandardWorkingDirectory();
        }
    }
}