using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Solhigson.Framework.Dto;
using Solhigson.Framework.Utilities;

namespace SolhigsonAspnetCoreApp.Domain.Dto;

public record UserDetailsInfo
{
    public bool RequirePasswordChange { get; set; }

    [PasswordPropertyText]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }

    [PasswordPropertyText]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required] public string Role { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(HelperFunctions.MatchEmailPattern, ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Firstname is required")]
    [JsonProperty("firstName")]
    [JsonPropertyName("firstName")]
    public string Firstname { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [JsonProperty("lastName")]
    [JsonPropertyName("lastName")]
    public string Lastname { get; set; }

    public string OtherNames { get; set; }

    public string InstitutionId { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; }


    public ResponseInfo IsValid()
    {
        var context = new ValidationContext(this);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(this, context, results, true);
        if (!isValid)
        {
            var msg = "";
            foreach (var vResult in results) msg += $"{vResult.ErrorMessage}, ";
            return ResponseInfo.FailedResult(msg);
        }


        return ResponseInfo.SuccessResult();
    }
}