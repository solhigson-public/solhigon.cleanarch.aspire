using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var redisCache = builder.AddRedis("redisCache");
var mongo = builder.AddMongoDB("mongoDb");
var mongoDb = mongo.AddDatabase("SolhigsonAspnetCoreApp");

if (builder.Environment.IsDevelopment())
{
    redisCache.WithRedisCommander();
    mongo.WithMongoExpress();
}

var apiService = builder
    .AddProject<Projects.SolhigsonAspnetCoreApp_ApiService>("apiService")
    .WithExternalHttpEndpoints()
    .WithReference(redisCache)
    .WithReference(mongoDb);

builder.Build().Run();
