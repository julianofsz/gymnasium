using gymnasium_academia.Models.Treinos;
using MongoDB.Driver;

namespace gymnasium_academia.Models;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<FichaTreino> FichasTreino => _database.GetCollection<FichaTreino>("FichasTreino");
}
