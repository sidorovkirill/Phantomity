using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Phantom.DTO;
using UnityEngine;

namespace Phantom
{
    public class DeepLinkProtocol : ConfigurableLink
    {
        private const string DefaultTargetUrl = "https://phantom.app/ul/v1";
        
        private readonly Dictionary<string, TaskCompletionSource<DeepLinkData>> _requests;
        private readonly string _targetUrl;

        public DeepLinkProtocol(string targetUrl = DefaultTargetUrl, LinkConfig linkConfig = null) : base(linkConfig)
        {
            _targetUrl = targetUrl;
            _requests = new Dictionary<string, TaskCompletionSource<DeepLinkData>>();
            Application.deepLinkActivated += OnResponse;
        }
        
        public DeepLinkProtocol(string targetUrl) : this()
        {
            _targetUrl = targetUrl;
        }

        public Task<DeepLinkData> Send(DeepLinkData payload)
        {
            var requestCompletionSource = new TaskCompletionSource<DeepLinkData>();
            var method = GetMethodName(payload.Method);
            
            _requests.Add(method, requestCompletionSource);

            var url = Serialize(payload);
            Application.OpenURL(url);
            
            return requestCompletionSource.Task;
        }

        private void OnResponse(string url)
        {
            var data = Deserialize(url);
            Debug.Log(JsonConvert.SerializeObject(data));
            Debug.Log(_requests.Keys.ToString());
            if (_requests.ContainsKey(data.Method))
            {
                _requests[data.Method].TrySetResult(data);
                _requests.Remove(data.Method);
            }
        }

        private string Serialize(DeepLinkData data)
        {
            var url = $"{_targetUrl}/{data.Method}";
            if (data.Params.Count > 0)
            {
                var query = new List<string>();
                foreach (var param in data.Params)
                {
                    query.Add($"{param.Key}={param.Value}");
                }
                url += "?" + String.Join("&", query.ToArray());   
            }

            return url;
        }
        
        private DeepLinkData Deserialize(string url)
        {
            var res = new DeepLinkData();

            var parsedUrl = new UrlParser(url);
            var method = parsedUrl.GetMethodName(LinkConfig.PathPrefix);
            var queryParams = parsedUrl.GetUrlPart(UrlRegexVariables.Params);

            if (string.IsNullOrEmpty(method))
            {
                throw new Exception("Received message doesn't have appropriate method");
            }
            
            res.Method = method;
            if (!string.IsNullOrEmpty(queryParams))
            {
                res.Params = ParseQueryParams(queryParams);
            }

            return res;
        }

        private Dictionary<string, string> ParseQueryParams(string queryParams)
        {
            var parsedParams = new Dictionary<string, string>();
            var parameters = queryParams.Split('&');
            foreach (var param in parameters)
            {
                var parts = param.Split('=');
                parsedParams.Add(parts[0], parts[1]);
            }

            return parsedParams;
        }

        private void CloseConnections()
        {
            foreach (var request in _requests)
            {
                Debug.LogError($"Request '{request.Key}' to Phantom wallet was not execute");
                request.Value.SetCanceled();
            }
        }
        
        ~DeepLinkProtocol()
        {
            Application.deepLinkActivated -= OnResponse;
            CloseConnections();
            _requests.Clear();
        }
    }
}