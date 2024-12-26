using DotnetChatbot.Helpers;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using DotnetChatbot.Interfaces;

namespace DotnetChatbot.Services
{
    internal class BasicChat : ChatInterface
    {
        public BasicChat() { }

        private IChatCompletionService chatCompletionService;
        private Kernel _kernel;
        private OpenAIPromptExecutionSettings openAIPromptExecutionSettings;

        public void init()
        {
            var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
            modelId: Config.OpenAI.ModelId!,
            apiKey: Config.OpenAI.ApiKey!);

            _kernel = kernelBuilder.Build();



            chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };
        }

        public async Task<List<string>> GetMessageAsync(ChatHistory chatHistory)
        {
            var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, openAIPromptExecutionSettings, _kernel);
            return new List<string>([response.ToString()]);
        }
    }
}