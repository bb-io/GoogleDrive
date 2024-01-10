using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.GoogleDrive.Webhooks
{
    public class BridgeService
    {
        private const string AppName = "onedrive";

        private readonly RestClient _bridgeClient;

        public BridgeService(string bridgeServiceUrl)
        {
            _bridgeClient = new RestClient(new RestClientOptions(bridgeServiceUrl));
        }

        public async Task StoreValue(string key, string value)
        {
            var storeValueRequest = CreateBridgeRequest($"/storage/{AppName}/{key}", Method.Post);
            storeValueRequest.AddBody(value);
            await _bridgeClient.ExecuteAsync(storeValueRequest);
        }

        public async Task<string> RetrieveValue(string key)
        {
            var deleteValueRequest = CreateBridgeRequest($"/storage/{AppName}/{key}", Method.Get);
            var result = await _bridgeClient.ExecuteAsync(deleteValueRequest);
            return result.Content;
        }

        public async Task DeleteValue(string key)
        {
            var deleteValueRequest = CreateBridgeRequest($"/storage/{AppName}/{key}", Method.Delete);
            await _bridgeClient.ExecuteAsync(deleteValueRequest);
        }

        private RestRequest CreateBridgeRequest(string endpoint, Method method)
        {
            var request = new RestRequest(endpoint, method);
            request.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
            return request;
        }
    }
}
