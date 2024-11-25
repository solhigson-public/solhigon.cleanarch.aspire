using Mapster;
using Newtonsoft.Json.Linq;
using Solhigson.Framework.Extensions;
using Solhigson.Framework.Logging;
using Solhigson.Framework.Utilities;
using SolhigsonAspnetCoreApp.Domain.Interfaces;

namespace SolhigsonAspnetCoreApp.Domain.ViewModels;

public record LogModel : ILogModel
{
    private List<ILogModel> _chainLogs;

    private JToken _exceptionJson;
    private ExceptionInfo _exceptionObject;
    public string Id { get; set; }
    public string Source { get; set; }
    public string LogLevel { get; set; }

    public string Description { get; set; }

    public string Group { get; set; }

    public string Data { get; set; }

    public string User { get; set; }

    public string ServiceName { get; set; }

    public string ServiceType { get; set; }

    public string ServiceUrl { get; set; }

    public string Status { get; set; }

    public string ChainId { get; set; }

    public double Timestamp { get; set; }

    public string Exception { get; set; }

    public string Message => Description ?? ExceptionObject?.Message;
    public string MachineName { get; set; }

    public ExceptionInfo ExceptionObject
    {
        get
        {
            try
            {
                return _exceptionObject ??= Exception.DeserializeFromJson<ExceptionInfo>();
            }
            catch (Exception e)
            {
                _exceptionObject = new ExceptionInfo
                {
                    Message = "Unable to deserialize exception json string, see inner exception for details",
                    InnerException = e.Adapt<ExceptionInfo>()
                };
                return _exceptionObject;
            }
        }
    }

    public JToken ExceptionJson
    {
        get
        {
            try
            {
                return _exceptionJson ??= JToken.Parse(Exception);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }


    public DateTime Date
    {
        get
        {
            try
            {
                return Timestamp.FromUnixTimestamp();
            }
            catch (Exception e)
            {
                this.ELogError(e);
            }

            return DateTime.UtcNow;
        }
    }

    public string MessageTruncatedLess
    {
        get
        {
            var text = Description ?? ExceptionObject?.Message;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var maxLength = text?.Length > 200 ? 200 : text.Length;
                if (text.Length > maxLength)
                    text = text.Substring(0, maxLength) + "... (more -> " + (text.Length - maxLength) + ")";

                return text;
            }

            return string.Empty;
        }
    }

    public string FormattedData
    {
        get
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Data)) return JToken.Parse(Data).ToString();
            }
            catch
            {
            }

            return Data;
        }
    }

    public List<ILogModel> ChainLogs => _chainLogs ??= new List<ILogModel>();
}