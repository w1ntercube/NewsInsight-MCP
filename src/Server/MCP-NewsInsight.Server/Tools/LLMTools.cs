using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MCPNewsInsight.Server.Tools
{
    [McpServerToolType]
    public static class LLMTools
    {
        private static readonly HttpClient httpClient = new HttpClient();

        // MCP 工具：与 LLM API 交互进行新闻内容分析
        [McpServerTool, Description("Analyze news content with LLM.")]
        public static async Task<string> AnalyzeNewsContentWithLLM(string newsContent)
        {
            // LLM API URL，假设这是提供情感分析或摘要的接口
            string llmApiUrl = "https://your-llm-api-endpoint.com/analyze";

            var requestBody = new
            {
                text = newsContent,
                model = "llm-model-id" // 模型ID，具体取决于您使用的 LLM 服务
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(llmApiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result; // 返回 LLM 分析结果
                }
                else
                {
                    return "LLM analysis failed. Please try again later.";
                }
            }
            catch (Exception ex)
            {
                return $"Error while calling LLM API: {ex.Message}";
            }
        }
    }
}
