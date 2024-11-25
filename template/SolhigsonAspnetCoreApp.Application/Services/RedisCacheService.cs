using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Solhigson.Framework.Dto;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Infrastructure.Dependency;
using Solhigson.Framework.Utilities;
using SolhigsonAspnetCoreApp.Domain.Dto;
using SolhigsonAspnetCoreApp.Infrastructure.Repositories.Abstractions;
using StackExchange.Redis;

namespace SolhigsonAspnetCoreApp.Application.Services;

[DependencyInject(DependencyType = DependencyType.Singleton)]
public class RedisCacheService : ServiceBase
{
    private readonly IConfiguration _configuration;
    private IDatabase? _database;

    public RedisCacheService(IRepositoryWrapper repositoryWrapper, IConfiguration configuration) : base(repositoryWrapper)
    {
        _configuration = configuration;
        Initialize();
    }

    private static string GetKey(string key)
    {
        return "SolhigsonAspnetCoreApp-" + key;
    }

    private void Initialize()
    {
        try
        {
            var tsqRedisConnectionString = _configuration.GetConnectionString("Redis");
            if (tsqRedisConnectionString is null)
            {
                return;
            }
            var redisConnectionOptions = ConfigurationOptions.Parse(tsqRedisConnectionString);
            /*
             * setting to 1 second, for faster fall back to database (for tsq)
             * as well as not delaying dashboard page refresh
             */
            redisConnectionOptions.AsyncTimeout = 1000; // 
            if (!string.IsNullOrWhiteSpace(tsqRedisConnectionString))
            {
                _database = ConnectionMultiplexer.Connect(redisConnectionOptions)?.GetDatabase();
                //_database = ConnectionMultiplexer.Connect(tsqRedisConnectionString)?.GetDatabase();
            }
        }
        catch (Exception e)
        {
            this.LogError(e);
        }
    }

    public async Task<ResponseInfo<RedisEntry<T>>> GetDataAsync<T>(string? key) where T : class
    {
        var result = new ResponseInfo<RedisEntry<T>>();
        try
        {
            if (_database is null || string.IsNullOrWhiteSpace(key))
            {
                return result.Fail();
            }
            var resp = await _database.StringGetAsync(GetKey(key));
            var entry = new RedisEntry<T>();
            string? json = resp;
            if (string.IsNullOrWhiteSpace(json))
            {
                return result.Success(entry);
            }
            
            try
            {
                entry.Value = json.DeserializeFromJson<T>();
            }
            catch (Exception e)
            {
                this.LogError(e, "While trying to deserialize {entry} into type {type}", json, typeof(T));
            }
            return result.Success(entry);
        }
        catch (Exception e)
        {
            this.LogError(e);
        }

        return result.Fail();
    }

    public async Task<ResponseInfo<bool>> SetDataAsync<T>(string? key, T data, TimeSpan? timeSpan = null)
    {
        var status = false;
        try
        {
            if (_database is not null && !string.IsNullOrWhiteSpace(key))
            {
                status = await _database.StringSetAsync(GetKey(key), data?.SerializeToJson(), timeSpan);
            }
        }
        catch (Exception e)
        {
            this.LogError(e);
            return ResponseInfo.FailedResult<bool>(e.Message);
        }

        return ResponseInfo.SuccessResult(status);
    }
    
    public async Task<ResponseInfo<bool>> DeleteKeyAsync(string? key)
    {
        var status = false;
        try
        {
            if (_database is not null && !string.IsNullOrWhiteSpace(key))
            {
                status = await _database.KeyDeleteAsync(GetKey(key));
            }
        }
        catch (Exception e)
        {
            this.LogError(e);
            return ResponseInfo.FailedResult<bool>(e.Message);
        }
        return ResponseInfo.SuccessResult(status);
    }
}