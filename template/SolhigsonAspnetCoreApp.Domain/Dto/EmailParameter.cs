using Solhigson.Framework.Notification;

namespace SolhigsonAspnetCoreApp.Domain.Dto;

public record EmailParameter
{
    public string Template { get; set; }
    public string Subject { get; set; }

    public IEnumerable<string> Roles { get; set; }

    public IEnumerable<string> SystemRoleKeys { get; set; }

    public string InstitutionId { get; set; }

    public string Permission { get; set; }

    public Dictionary<string, string> TemplatePlaceholders { get; set; }

    public IEnumerable<string> To { get; set; }
    public IEnumerable<string> Cc { get; set; }
    public IEnumerable<string> Bcc { get; set; }

    public IList<AttachmentHelper> Attachments { get; set; }
    public string FromDisplayAddress { get; set; }
}