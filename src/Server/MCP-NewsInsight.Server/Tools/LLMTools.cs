using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace MCPNewsInsight.Server.Tools
{
    [McpServerToolType]
    public static class LLMTools
    {
        private const string LLM_API_URL = "https://ark.cn-beijing.volces.com/api/v3/chat/completions";
        private const string API_KEY = "your-api-key-here";
        private const string MODEL_ID = "doubao-1.5-pro-32k-250115";

        [McpServerTool(Name = "LLMAgent"),
         Description("使用大型语言模型分析问题并生成回答")]
        public static async Task<string> AnalyzeQuestion(
            [Description("用户的问题")] string question,
            [Description("可用工具列表")] List<string> availableTools)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");

            // 构建系统提示词
            string systemPrompt = BuildSystemPrompt(availableTools);

            // 构建LLM请求
            var request = new
            {
                model = MODEL_ID,
                messages = new List<object>
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = question }
                },
                max_tokens = 1024,
                temperature = 0.3,
                top_p = 0.9,
                tools = new[]
                {
                    new
                    {
                        type = "function",
                        function = new
                        {
                            name = "ExecuteTool",
                            description = "执行指定的工具并获取结果",
                            parameters = new
                            {
                                type = "object",
                                properties = new
                                {
                                    tool_name = new
                                    {
                                        type = "string",
                                        description = "要执行的工具名称",
                                        list = availableTools
                                    },
                                    arguments = new
                                    {
                                        type = "object",
                                        description = "工具的输入参数",
                                        properties = new
                                        {
                                            // 动态参数将在提示词中说明
                                        }
                                    }
                                },
                                required = new[] { "tool_name" }
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(LLM_API_URL, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseContent);

            // 解析工具调用指令
            var toolCalls = responseJson.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("tool_calls");

            if (toolCalls.ValueKind == JsonValueKind.Array && toolCalls.GetArrayLength() > 0)
            {
                var firstCall = toolCalls[0];
                var function = firstCall.GetProperty("function");

                return JsonSerializer.Serialize(new
                {
                    tool_name = function.GetProperty("name").GetString(),
                    arguments = function.GetProperty("arguments").GetString()
                });
            }

            // 如果没有工具调用，返回文本回答
            return responseJson.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }

        private static string BuildSystemPrompt(List<string> availableTools)
        {
            var sb = new StringBuilder();
            sb.AppendLine("你是一个新闻分析助手，可以调用以下工具来回答问题：");
            sb.AppendLine(string.Join(", ", availableTools));
            sb.AppendLine();
            sb.AppendLine("指令规则：");
            sb.AppendLine("1. 当需要获取数据时，使用ExecuteTool函数指定工具名称和参数");
            sb.AppendLine("2. 工具执行后，我会将结果返回给你");
            sb.AppendLine("3. 根据结果生成最终回答");
            sb.AppendLine();
            sb.AppendLine("可用工具说明：");
            sb.AppendLine("- GetNewsHeadlines: 获取新闻头条，参数: maxCount(数量), category(分类)");
            sb.AppendLine("- GetNewsContent: 获取新闻内容，参数: headline(标题)");
            sb.AppendLine("- GetNewsCategories: 获取所有新闻分类");
            sb.AppendLine("- GetPopularTopics: 获取热门话题");
            sb.AppendLine();
            sb.AppendLine("示例：");
            sb.AppendLine("用户: 科技类的最新新闻有哪些？");
            sb.AppendLine("助手: 调用GetNewsHeadlines工具，参数: { \"category\": \"科技\", \"maxCount\": 5 }");

            return sb.ToString();
        }
    }
}