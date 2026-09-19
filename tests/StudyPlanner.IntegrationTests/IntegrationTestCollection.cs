namespace StudyPlanner.IntegrationTests;

/// <summary>Um único Postgres/host compartilhado entre todos os testes desta coleção — não um container por teste.</summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<StudyPlannerWebApplicationFactory>;
