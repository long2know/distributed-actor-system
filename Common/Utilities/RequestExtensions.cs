using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;

namespace Common.Utilities
{
    public static class MapperExtentions
    {
        public async static Task<T> ReadAsAsync<T>(this HttpContext context)
        {
            var initialBody = context.Request.Body; // Workaround

            context.Request.EnableRewind();
            var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            var json = Encoding.UTF8.GetString(buffer);

            context.Request.Body = initialBody; // Workaround

            T retValue = JsonConvert.DeserializeObject<T>(json);
            return retValue;
        }

        public async static Task<string> ReadAsString(this HttpRequest request)
        {
            var initialBody = request.Body; // Workaround

            request.EnableRewind();
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var body = Encoding.UTF8.GetString(buffer);
            request.Body = initialBody; // Workaround
            return body;
        }

        public async static Task<T> ReadAsAsync<T>(this HttpRequest request)
        {
            var json = await request.ReadAsString();
            T retValue = JsonConvert.DeserializeObject<T>(json);
            return retValue;
        }
    }
}