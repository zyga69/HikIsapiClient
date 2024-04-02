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
        private static XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace

        };

        public SeriRestClient(RestClientOptions options, Logger logger) : base(options)
        {
            this.logger = logger;
        }


        public RestResponse<T> LogGet<T>(RestRequest request)
        {
            return LogRequest<T>(request, Method.Get);
        }

        public RestResponse<T> LogPost<T>(RestRequest request)
        {
            return LogRequest<T>(request, Method.Post);
        }

        public RestResponse<TResponse> LogRequest<TResponse>(RestRequest request, Method method)
        {
            logger.Debug($"[REST] REQUEST  [{method}][{request.Resource}]");

            foreach (Parameter parameter in request.Parameters)
            {
                if (parameter.Type == ParameterType.RequestBody)
                {

                    LogBody(parameter.Value, request.RequestFormat);
                }
                else
                {
                    logger.Debug($"[REST]  [{parameter.Type}]->[{parameter.Name}]: [{parameter.Value}]");
                }

            }

            var response = this.Execute<TResponse>(request, method);

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

            LogBody(response.Content, DataFormat.None);

            return response;

        }

        private void LogBody(object? data, DataFormat format)
        {
            if (data == null)
                return;

            logger.Debug($"[REST]  [Payload]: {FormatBody(data, format)}");
        }

        private static string FormatBody(object? data, DataFormat format)
        {
            if (data == null)
                return string.Empty;

            switch (format) 
            {
                case DataFormat.Json: 
                    return JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);

                case DataFormat.Xml:
                    return GetXml(data);

                default:
                    return data.ToString();
                    
            }

        }

        private static string GetXml(object? data) 
        {
            XmlSerializer xsSubmit = new XmlSerializer(data.GetType());

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww, settings))
                {
                    xsSubmit.Serialize(writer, data);
                    return sww.ToString(); 
                }
            }
        }

    }
}
