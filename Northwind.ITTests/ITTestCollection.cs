using Xunit;

namespace Northwind.ITTests
{
    [CollectionDefinition(TestConstants.ItTests)]
    public class ITTestCollection : ICollectionFixture<ITTestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}