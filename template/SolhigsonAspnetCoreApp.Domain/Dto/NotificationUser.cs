namespace SolhigsonAspnetCoreApp.Domain.Dto;

public record NotificationUser
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}