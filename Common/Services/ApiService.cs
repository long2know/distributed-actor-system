using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public interface IApiService
    {
        T GetFromApi<T>(string url, Dictionary<string, string> kvp = null, string token = "");
        V PutOrPostToApi<T, V>(string url, T obj, HttpMethod verb = null, bool isFormPost = false, string token = "");

        // Simple form post
        T FormPost<T>(string url, Dictionary<string, string> dict, string token = "");
    }

    public class ApiService : IApiService
    {
        private TimeSpan _timeout;
        //private ILog _log;

        private static string _jsonMediaType = "application/json";
        private static string _formMediaType = "application/x-www-form-urlencoded";

        public ApiService(int timeout = 20000)
        {
            //_log = log;
            _timeout = TimeSpan.FromMilliseconds(timeout);
        }

        // <summary>
        /// Get JSON data from an API endpoint
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="kvp"></param>
        /// <returns></returns>
        public T GetFromApi<T>(string url, Dictionary<string, string> kvp = null, string token = "")
        {
            var json = string.Empty;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var requestUri = kvp == null ?
                new Uri(url) :
                new Uri(string.Format("{0}?{1}",
                    url,
                    string.Join("&",
                        kvp.Keys
                        .Where(key => !string.IsNullOrWhiteSpace(kvp[key]))
                        .Select(key => string.Format("{0}={1}", WebUtility.HtmlEncode(key), WebUtility.HtmlEncode(kvp[key]))))
                    )
                );

            var requestMessage = new HttpRequestMessage()
            {
                RequestUri = requestUri,
                Method = HttpMethod.Get
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                requestMessage.AttachBearerToken(token);
            }

            return SendRequest<T>(requestMessage);
        }

        /// <summary>
        /// PUT or POST to an Api endpoint and return a JSON response
        /// </summary>
        /// <typeparam name="T">Type of object being posted</typeparam>
        /// <typeparam name="V">Type of object to deserialize response into</typeparam>
        /// <param name="url"></param>
        /// <param name="obj">Data to send to Api</param>
        /// <param name="verb">HttpVerb to use</param>
        /// <param name="isFormPost">Boolean to indicate whether to send data as JSON or FORM post</param>
        /// <returns></returns>
        public V PutOrPostToApi<T, V>(string url, T obj, HttpMethod verb = null, bool isFormPost = false, string token = "")
        {
            var requestContent = string.Empty;
            verb = verb == HttpMethod.Put ? HttpMethod.Put : HttpMethod.Post;
            var requestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = verb
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                requestMessage.AttachBearerToken(token);
            }

            var json = JsonConvert.SerializeObject(obj);

            if (isFormPost)
            {
                var jObj = (JObject)JsonConvert.DeserializeObject(json);
                var queryParams = String.Join("&",
                                jObj.Children().Cast<JProperty>()
                                .Select(jp => jp.Name + "=" + WebUtility.HtmlEncode(jp.Value.ToString())));
                requestMessage.Content = new StringContent(queryParams, Encoding.ASCII, _formMediaType);

                // Remove passwords from log
                var parsed = QueryHelpers.ParseQuery(requestContent);
                foreach (var key in parsed.Keys)
                {
                    var parsedKey = key.ToLower();
                    if (parsedKey == "client_id" || parsedKey == "client_secret" || parsedKey == "password")
                    {
                        parsed[parsedKey] = "********";
                    }
                }

                // For logging
                requestContent = string.Format("Content: {0}, Type: {1}, Verb: {2}", string.Join("&", parsed), _formMediaType, verb.ToString());
            }
            else
            {
                requestMessage.Content = new StringContent(json, Encoding.UTF8, _jsonMediaType);

                // For logging
                requestContent = string.Format("Content: {0}, Type: {1}, Verb: {2}", json, _formMediaType, verb.ToString());
            }

            requestMessage.Headers.Clear();
            requestMessage.Content.Headers.ContentType = isFormPost ?
                new MediaTypeHeaderValue(_formMediaType) :
                new MediaTypeHeaderValue(_jsonMediaType);

            return SendRequest<V>(requestMessage);
        }

        /// <summary>
        /// Performs a simple Form Post
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        public T FormPost<T>(string url, Dictionary<string, string> dict, string token = "")
        {
            var requestMessage = new HttpRequestMessage()
            {
                Content = new FormUrlEncodedContent(dict),
                RequestUri = new Uri(url),
                Method = HttpMethod.Post
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                requestMessage.AttachBearerToken(token);
            }

            return SendRequest<T>(requestMessage);
        }

        /// <summary>
        /// Deserialized a json string to type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        private T Deserialize<T>(string json)
        {
            T retValue;
            // Deserialize the token response
            var type = typeof(T);
            if (type.IsPrimitive || type == typeof(Decimal) || type == typeof(string))
            {
                json = System.Net.WebUtility.HtmlDecode(json);
                retValue = (T)Convert.ChangeType(json, typeof(T));
            }
            else
            {
                if (type == typeof(ExpandoObject))
                {
                    var converter = new ExpandoObjectConverter();
                    var obj = JsonConvert.DeserializeObject<T>(json, converter);
                    return obj;
                }
                retValue = JsonConvert.DeserializeObject<T>(json);
            }

            return retValue;
        }

        private T SendRequest<T>(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage response = null;
            var json = string.Empty;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                using (var client = new HttpClient() { Timeout = _timeout })
                {
                    // Assume we're dealigng with JSON
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_jsonMediaType));

                    response = client.SendAsync(requestMessage).Result;
                    response.EnsureSuccessStatusCode();
                    Task<Stream> streamTask = response.Content.ReadAsStreamAsync();
                    Stream stream = streamTask.Result;
                    var sr = new StreamReader(stream);
                    json = sr.ReadToEnd();
                    var retValue = Deserialize<T>(json);

                    //_log.Debug(string.Format("API {0} Request Success: {1}, Response: {2}, CommTime: {3}",
                    //    requestMessage.Method,
                    //    requestMessage.RequestUri.ToString(),
                    //    json,
                    //    sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"))
                    //);
                    return retValue;
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is TaskCanceledException)
                {
                    //_log.Debug(string.Format("API {0} Request failed: {1}, {2}, CommTime: {3}",
                    //    requestMessage.Method, requestMessage.RequestUri.ToString(), sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff")), ex.InnerException);
                    throw ex.InnerException;
                }
                else
                {
                    //_log.Debug(string.Format("API {0} Request failed: {1}, {2}, CommTime: {3}",
                    //    requestMessage.Method, requestMessage.RequestUri.ToString(), sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff")), ex);
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                //_log.Debug(string.Format("API {0} Request Bad Status: {1}, Status Code {2}, CommTime: {3}",
                //    requestMessage.Method,
                //    requestMessage.RequestUri.ToString(),
                //    response.StatusCode,
                //    sw.Elapsed.ToString(@"hh\:mm\:ss\.ffff"))
                //);

                throw ex;
            }
        }
    }

    /// <summary>
    /// Basic extensions for dealing with request messages
    /// </summary>
    internal static class RequestMessageExtensions
    {
        public static HttpRequestMessage AttachBearerToken(this HttpRequestMessage requestMessage, string token)
        {
            requestMessage.Headers.Add("Authorization", string.Format("Bearer {0}", token));
            return requestMessage;
        }
    }
}
