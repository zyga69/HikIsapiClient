using Newtonsoft.Json;
using RestSharp;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using HikIsapi.Client.Model.Request;

namespace hikisapiclient.SeriRestClient
{
    public  class SeriRestClient : RestClient
    {
        private readonly Logger logger;

        public SeriRestClient(RestClientOptions options) : base(options)
        {
            logger = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, applyThemeToRedirectedOutput: true)
                .MinimumLevel.Debug()
                .CreateLogger();
        }


        public RestResponse<T> LogGet<T>(RestRequest request)
        {
            return LogRequest<T>(request, Method.Get);
        }

        public RestResponse LogPost(RestRequest request)
        {
            return LogRequest<object>(request, Method.Post);
        }

        public RestResponse<T> LogRequest<T>(RestRequest request, Method method)
        {
            logger.Debug($"[REST] REQUEST  [{method}][{request.Resource}]");

            foreach(Parameter parameter in request.Parameters)
            {
                if (parameter.Type == ParameterType.RequestBody)
                {
                    logger.Debug($"[REST]          [Payload]: {GetXml<SessionLogin>(parameter.Value as SessionLogin)}");
                    //logger.Debug($"[REST]          [Payload]: {JsonConvert.SerializeObject(parameter.Value, Formatting.Indented)}");
                }
                else
                {
                    logger.Debug($"[REST]          [{parameter.Type}]->[{parameter.Name}]: [{parameter.Value}]");
                }
                
            }

            var response = this.Execute<T>(request, method);

            if (response.IsSuccessful)
            {
                logger.Debug($"[REST] RESPONSE [{response.StatusCode}]");
            }
            else if (response.IsSuccessStatusCode)
            {
                logger.Warning($"[REST] RESPONSE [{response.StatusCode}]: [{response.ErrorMessage}]");
            }
            else
            {
                logger.Error($"[REST] RESPONSE [{response.StatusCode}]: [{response.ErrorMessage}]");
            }

            if (response.Data != null)
            {
                var payload = JsonConvert.SerializeObject(response.Data, Newtonsoft.Json.Formatting.Indented);
                logger.Debug($"[REST] [Payload]: {payload}");
            }

            return response;

        }

        private static string GetXml<T>(T data) 
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, data);
                    return sww.ToString(); 
                }
            }
        }
    }
}
