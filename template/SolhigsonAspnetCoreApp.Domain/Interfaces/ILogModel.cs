using Newtonsoft.Json.Linq;
using Solhigson.Framework.Logging;

namespace SolhigsonAspnetCoreApp.Domain.Interfaces;

public interface ILogModel
{
    public string Id { get; set; }

    public string Source { get; set; }

    public string LogLevel { get; set; }

    public string Description { get; set; }

    public string Group { get; set; }

    public string Exception { get; set; }

    public string Data { get; set; }

    public string User { get; set; }

    public string ServiceName { get; set; }

    public string ServiceType { get; set; }

    public string ServiceUrl { get; set; }

    public string Status { get; set; }

    public string ChainId { get; set; }

    public double Timestamp { get; set; }

    public List<ILogModel> ChainLogs { get; }

    public string MessageTruncatedLess { get; }

    public string FormattedData { get; }

    public DateTime Date { get; }

    public JToken ExceptionJson { get; }

    public ExceptionInfo ExceptionObject { get; }

    public string Message { get; }
    public string MachineName { get; set; }
}