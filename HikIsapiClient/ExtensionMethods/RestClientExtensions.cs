using RestSharp;

namespace HikIsapi.Client.ExtensionMethods;

// public class RestClientExtensions
// {
//     public static RestResponse<T> Get<T>(this RestClient client, RestRequest request)
//     {
//         request.Method = Method.Get;
//         var capabilitiesRequest = new RestRequest("Security/sessionLogin/capabilities");
//         capabilitiesRequest.AddQueryParameter("username", user);
//         capabilitiesRequest.AddQueryParameter("random", GetRandom());

//         logger.Debug($"Executing {request.Method} [{request}]");

//         var response = client.ExecuteGet<SessionLoginCap>(capabilitiesRequest);

//         if (!response.IsSuccessful)
//         {
//             logger.Error("Could not GET Login Capabilities");
//             logger.Error($"[{response.StatusCode}]: [{response.ErrorMessage}]");
//             return null;
//         }

//         return response.Data;
//     }
// }
