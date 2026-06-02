using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace HuggingfaceHub
{
    public static partial class HFDownloader
    {
        private static HttpClient HttpClient { get; set; }

        public static ILogger? Logger { get; set; }

        static HFDownloader()
        {
#if WINDOWS
        // In Windows, you can use HttpClientHandler to use the system's certificate store
        HttpClientHandler handler = new HttpClientHandler()
        {
            UseDefaultCredentials = true
        };
#else
            // If you're not in Windows, default handler can be used.
            HttpClientHandler handler = new HttpClientHandler();
#endif
            HttpClient = new HttpClient(handler);
        }

        private static string GetFileUrl(string repoId, string revision, string filename, string? endpoint = null)
        {
            if(endpoint is null){
                endpoint = HFGlobalConfig.EndPoint;
            }
            return $"{endpoint}/{repoId}/resolve/{revision}/{filename}";
        }

        /// <summary>
        /// Build the headers sent in a Hugging Face Hub request.
        /// </summary>
        /// <remarks>
        /// By default, the authorization token is provided either from the argument or from the
        /// local cache. To explicitly avoid sending the token to the Hub, set <c>token</c> to
        /// <see langword="null"/>.
        ///
        /// If the API call requires write access, an error is thrown when the token is missing
        /// or is an organization token starting with <c>api_org</c>.
        ///
        /// In addition to the authorization header, a user-agent value is added to describe
        /// the installed client packages.
        /// </remarks>
        /// <param name="token"></param>
        /// <param name="isWriteAction"></param>
        /// <param name="userAgent"></param>
        private static IDictionary<string, string> BuildHFHeaders(string? token = null, bool isWriteAction = false, IDictionary<string, string>? userAgent = null)
        {
            Dictionary<string, string> headers = new();
            userAgent ??= new Dictionary<string, string>();
            headers["user-agent"] = GetHttpUserAgentStr(userAgent);
            if(token is not null){
                var tokenToSend = token;
                ValidateTokenToSend(tokenToSend, isWriteAction);
                headers["authorization"] = $"Bearer {tokenToSend}";
            }

            return headers;
        }

        private static string GetHttpUserAgentStr(IDictionary<string, string> userAgent){
            string res = "unknown/None";
            res += ";" + string.Join(";", userAgent.Select(kv => $"{kv.Key}/{kv.Value}").ToArray());
            return res;
        }

        private static string GetTokenToSend(string token){
            // TODO: deal with token cache here.
            return token;
        }

        private static void ValidateTokenToSend(string token, bool isWriteAction) { 
            if(isWriteAction){
                if(token.StartsWith("api_org")){
                    throw new ArgumentException("You must use your personal account token for write-access methods. To " +
                    " generate a write-access token, go to  https://huggingface.co/settings/tokens");
                }
            }
        }
    }
}
