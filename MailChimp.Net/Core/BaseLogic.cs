// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseLogic.cs" company="Brandon Seydel">
//   N/A
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
#pragma warning disable 1584, 1711, 1572, 1581, 1580

namespace MailChimp.Net.Core
{
    /// <summary>
    /// The base logic.
    /// </summary>
    public abstract class BaseLogic
    {
        /// <summary>
        /// The _api key.
        /// </summary>
        private readonly string _apiKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLogic"/> class.
        /// </summary>
        /// <param name="apiKey">
        /// The api key.
        /// </param>
        protected BaseLogic(string apiKey)
        {
            this._apiKey = apiKey;
        }

        /// <summary>
        /// The _data center.
        /// </summary>
        private string DataCenter
            =>
                this._apiKey.Substring(
                    this._apiKey.LastIndexOf("-", StringComparison.Ordinal) + 1, 
                    this._apiKey.Length - this._apiKey.LastIndexOf("-", StringComparison.Ordinal) - 1);

        /// <summary>
        /// The create mail client.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see cref="HttpClient"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref>
        ///         <name>uriString</name>
        ///     </paramref>
        ///     is null. </exception>
        /// <exception cref="UriFormatException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.<paramref name="uriString" /> is empty.-or- The scheme specified in <paramref name="uriString" /> is not correctly formed. See <see cref="M:System.Uri.CheckSchemeName(System.String)" />.-or- <paramref name="uriString" /> contains too many slashes.-or- The password specified in <paramref name="uriString" /> is not valid.-or- The host name specified in <paramref name="uriString" /> is not valid.-or- The file name specified in <paramref name="uriString" /> is not valid. -or- The user name specified in <paramref name="uriString" /> is not valid.-or- The host or authority name specified in <paramref name="uriString" /> cannot be terminated by backslashes.-or- The port number specified in <paramref name="uriString" /> is not valid or cannot be parsed.-or- The length of <paramref name="uriString" /> exceeds 65519 characters.-or- The length of the scheme specified in <paramref name="uriString" /> exceeds 1023 characters.-or- There is an invalid character sequence in <paramref name="uriString" />.-or- The MS-DOS path specified in <paramref name="uriString" /> must start with c:\\.</exception>
        protected HttpClient CreateMailClient(string resource)
        {
            var client = new HttpClient
                             {
                                 BaseAddress =
                                     new Uri($"https://{this.DataCenter}.api.mailchimp.com/3.0/{resource}")
                             };            
            client.DefaultRequestHeaders.Add("Authorization", $"apikey {this._apiKey}");
            return client;
        }
        /// <summary>
        /// Create Syncronized HttpWebRequest
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="method"></param>
        /// <returns>HttpWebRequest</returns>
        protected HttpWebRequest CreateJsonWebRequest(string resource, string httpMethod)
        {
            var client = new WebClient();            

            //client.Headers.Add("Authorization", $"apikey {this._apiKey}");

            string baseAddress = $"https://{this.DataCenter}.api.mailchimp.com/3.0/{resource}";

            var request = (HttpWebRequest)WebRequest.Create(baseAddress);

            request.Headers.Add("Authorization", $"apikey {this._apiKey}");
            
            request.Accept = "application/json"; 
            request.ContentType = "application/json";
            request.Method = httpMethod;

            return request;
        }

        /// <summary>
        /// This memthod takes a webreqest and object, write Stram and get the response
        /// </summary>
        /// <param name="request"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected string GetJsonResponseFromRequest(HttpWebRequest request, Object obj)
        {
            //Get JSON string for the Member
            string requestContent = JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            //Encode JSON to Byte[] using UTF8 (ASCII can be used but it will limit the range of characters can be used)
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] bytes = encoding.GetBytes(requestContent);

            //Set he content lenth for the Headers
            request.ContentLength = bytes.LongLength;

            //Create a new stream from the request
            Stream newStream = request.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            //Get response from the request
            var response = request.GetResponse();

            var responseStream = response.GetResponseStream();
            var responseStreamReader = new StreamReader(responseStream);

            //Return the response stream
            return responseStreamReader.ReadToEnd();
        }

        /// <summary>
        /// The hash.
        /// </summary>
        /// <param name="emailAddress">
        /// The email address.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="TargetInvocationException">The algorithm was used with Federal Information Processing Standards (FIPS) mode enabled, but is not FIPS compatible.</exception>
        /// <exception cref="ObjectDisposedException">
        /// The object has already been disposed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref>
        ///         <name>s</name>
        ///     </paramref>
        ///     is null. 
        /// </exception>
        /// <exception cref="EncoderFallbackException">
        /// A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback"/> is set to <see cref="T:System.Text.EncoderExceptionFallback"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity"/>. 
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref>
        ///         <name>format</name>
        ///     </paramref>
        /// includes an unsupported specifier. Supported format specifiers are listed in the Remarks section.
        /// </exception>
        public string Hash(string emailAddress)
        {
            using (var md5 = MD5.Create()) return md5.GetHash(emailAddress.ToLower());
        }
    }
}