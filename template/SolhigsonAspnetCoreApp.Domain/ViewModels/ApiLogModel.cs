using System.Text;
using Solhigson.Framework.Logging;
using Solhigson.Framework.Utilities;
using SolhigsonAspnetCoreApp.Domain.Interfaces;

namespace SolhigsonAspnetCoreApp.Domain.ViewModels;

public record ApiLogModel
{
    private static readonly LogWrapper Logger = LogManager.GetLogger(typeof(ApiLogModel).FullName);

    public string Id { get; set; }
    public string Caller { get; set; }
    public string Method { get; set; }

    public string Url { get; set; }

    public string StatusCode { get; set; }
    public string StatusCodeDescription { get; set; }
    public object RequestMessage { get; set; }
    public object RequestHeaders { get; set; }
    public object ResponseMessage { get; set; }
    public object ResponseHeaders { get; set; }
    public DateTime RequestTime { get; set; }
    public DateTime ResponseTime { get; set; }
    public string TimeTaken { get; set; }

    public string ResponseHeadersFormatted => ExtractHeaders(ResponseHeaders);

    public string RequestHeadersFormatted => ExtractHeaders(RequestHeaders);

    private string ExtractHeaders(object jObject)
    {
        try
        {
            if (jObject != null)
            {
                var dic = jObject.SerializeToKeyValue();
                var sBuilder = new StringBuilder();
                foreach (var key in dic.Keys)
                {
                    if (string.IsNullOrWhiteSpace(key)) continue;

                    sBuilder.AppendLine($"<b>{key}:</b>&nbsp&nbsp;{dic[key]}<br />");
                }

                return sBuilder.ToString();
            }
        }
        catch

        {
        }

        return string.Empty;
    }


    public static ApiLogModel Adapt(ILogModel model)
    {
        try
        {
            var apiModel = model.Data.DeserializeFromJson<ApiLogModel>();
            if (apiModel is not null)
            {
                apiModel.Url = model.ServiceUrl;
                apiModel.Id = model.Id;
                return apiModel;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }

        return new ApiLogModel { Caller = "**error***" };
    }

    public string MethodColour()
    {
        return Method.ToLower() switch
        {
            "get" => "#61affe",
            "post" => "#49cc90",
            "put" => "#fca130",
            "delete" => "#f93e3e",
            _ => "#000"
        };
    }

    public record LogDataField
    {
        public string Type { get; set; }
        public string Data { get; set; }
    }
}