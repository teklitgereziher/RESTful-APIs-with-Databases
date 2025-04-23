namespace AzureCosmos.CRUD.IntegrationTests.Setup
{
  /// <summary>
  /// This marks IntegrationTestFactory as a collection fixture for all integration tests.
  /// </summary>
  [CollectionDefinition("IntegrationTestCollection")]
  public class IntegrationTestCollection : IClassFixture<IntegrationTestFactory>
  {

  }
}
