namespace SolhigsonAspnetCoreApp.Domain.Dto;

public struct RedisEntry<T>
{
    public T Value { get; set; }
}