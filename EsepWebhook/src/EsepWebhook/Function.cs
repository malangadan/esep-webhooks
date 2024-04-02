using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        public string FunctionHandler(object input, ILambdaContext context)
        {
            
            dynamic payload = JsonConvert.DeserializeObject<dynamic>(input.ToString());

            
            string issueUrl = payload?.issue?.html_url;

           
            if (!string.IsNullOrEmpty(issueUrl))
            {

                string slackPayload = $"{{\"text\": \"Issue Created: {issueUrl}\"}}";

                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"));

                    request.Content = new StringContent(slackPayload, Encoding.UTF8, "application/json");

                    var response = client.SendAsync(request).Result;

                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    context.Logger.LogLine($"Slack response: {responseContent}");

                    return responseContent;
                }
            }
            else
            {
                context.Logger.LogLine("Issue URL not found in payload.");
                return "Issue URL not found in payload.";
            }
        }
    }
}