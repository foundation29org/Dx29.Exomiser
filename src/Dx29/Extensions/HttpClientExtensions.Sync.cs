using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

using Newtonsoft.Json;

namespace Dx29
{
    static partial class HttpClientExtensions
    {
        //
        //  GET
        //
        static public string GetString(this HttpClient http, string action, params (string, string)[] headers) => Send(http, action, HttpMethod.Get, headers);
        static public TValue Get<TValue>(this HttpClient http, string action, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Get, headers);

        //
        //  POST
        //
        static public string Post(this HttpClient http, string action, object value, params (string, string)[] headers) => Send(http, action, HttpMethod.Post, value, headers);
        static public string Post(this HttpClient http, string action, Stream stream, params (string, string)[] headers) => Send(http, action, HttpMethod.Post, stream, headers);
        static public string Post(this HttpClient http, string action, HttpContent content, params (string, string)[] headers) => Send(http, action, HttpMethod.Post, content, headers);

        static public TValue Post<TValue>(this HttpClient http, string action, object value, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Post, value, headers);
        static public TValue Post<TValue>(this HttpClient http, string action, Stream stream, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Post, stream, headers);
        static public TValue Post<TValue>(this HttpClient http, string action, HttpContent content, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Post, content, headers);

        //
        //  PUT
        //
        static public string Put(this HttpClient http, string action, object value, params (string, string)[] headers) => Send(http, action, HttpMethod.Put, value, headers);
        static public string Put(this HttpClient http, string action, Stream stream, params (string, string)[] headers) => Send(http, action, HttpMethod.Put, stream, headers);
        static public string Put(this HttpClient http, string action, HttpContent content, params (string, string)[] headers) => Send(http, action, HttpMethod.Put, content, headers);

        static public TValue Put<TValue>(this HttpClient http, string action, object value, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Put, value, headers);
        static public TValue Put<TValue>(this HttpClient http, string action, Stream stream, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Put, stream, headers);
        static public TValue Put<TValue>(this HttpClient http, string action, HttpContent content, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Put, content, headers);

        //
        //  PATCH
        //
        static public string Patch(this HttpClient http, string action, object value, params (string, string)[] headers) => Send(http, action, HttpMethod.Patch, value, headers);
        static public string Patch(this HttpClient http, string action, Stream stream, params (string, string)[] headers) => Send(http, action, HttpMethod.Patch, stream, headers);
        static public string Patch(this HttpClient http, string action, HttpContent content, params (string, string)[] headers) => Send(http, action, HttpMethod.Patch, content, headers);

        static public TValue Patch<TValue>(this HttpClient http, string action, object value, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Patch, value, headers);
        static public TValue Patch<TValue>(this HttpClient http, string action, Stream stream, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Patch, stream, headers);
        static public TValue Patch<TValue>(this HttpClient http, string action, HttpContent content, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Patch, content, headers);

        //
        //  DELETE
        //
        static public string Delete(this HttpClient http, string action, params (string, string)[] headers) => Send(http, action, HttpMethod.Delete, headers);
        static public TValue Delete<TValue>(this HttpClient http, string action, params (string, string)[] headers) => Send<TValue>(http, action, HttpMethod.Delete, headers);


        static public TValue Send<TValue>(this HttpClient http, string action, HttpMethod method, params (string, string)[] headers)
        {
            return Send<TValue>(http, action, method, (HttpContent)null, headers);
        }
        static public TValue Send<TValue>(this HttpClient http, string action, HttpMethod method, object obj, params (string, string)[] headers)
        {
            var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            return Send<TValue>(http, action, method, content, headers);
        }
        static public TValue Send<TValue>(this HttpClient http, string action, HttpMethod method, Stream stream, params (string, string)[] headers)
        {
            var content = new StreamContent(stream);
            return Send<TValue>(http, action, method, content, headers);
        }
        static public TValue Send<TValue>(this HttpClient http, string action, HttpMethod method, HttpContent content, params (string, string)[] headers)
        {
            string json = Send(http, action, method, content, headers);
            return JsonConvert.DeserializeObject<TValue>(json);
        }

        static public string Send(this HttpClient http, string action, HttpMethod method, params (string, string)[] headers)
        {
            return Send(http, action, method, (HttpContent)null, headers);
        }
        static public string Send(this HttpClient http, string action, HttpMethod method, object obj, params (string, string)[] headers)
        {
            var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            return Send(http, action, method, content, headers);
        }
        static public string Send(this HttpClient http, string action, HttpMethod method, Stream stream, params (string, string)[] headers)
        {
            var content = new StreamContent(stream);
            return Send(http, action, method, content, headers);
        }
        static public string Send(this HttpClient http, string action, HttpMethod method, HttpContent content, params (string, string)[] headers)
        {
            var request = CreateRequest(action, method, headers);
            if (content != null) request.Content = content;
            (var resp, var status) = Send(http, request);
            if (status == HttpStatusCode.OK)
            {
                return resp;
            }
            resp = $"{status}. {resp}";
            throw new HttpRequestException(resp, null, status);
        }

        static public (string, HttpStatusCode) Send(this HttpClient http, HttpRequestMessage request)
        {
            var response = http.Send(request);
            using (var reader = new StreamReader(response.Content.ReadAsStream()))
            {
                return (reader.ReadToEnd(), response.StatusCode);
            }
        }

        static private HttpRequestMessage CreateRequest(string action, HttpMethod method, params (string, string)[] headers)
        {
            var request = new HttpRequestMessage(method, action);
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Item1, header.Item2);
                }
            }
            return request;
        }
    }
}
