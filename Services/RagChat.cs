using DotnetChatbot.Helpers;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using DotnetChatbot.Interfaces;
using Microsoft.SemanticKernel.Memory;


namespace DotnetChatbot.Services
{
    internal class RagChat : ChatInterface
    {
        public RagChat() { }

        private Kernel _kernel;

        public void init()
        {
            var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
            modelId: Config.OpenAI.ModelId!,
            apiKey: Config.OpenAI.ApiKey!);

            //I used the InMemory API, which is currently marked as deprecated, so I temporarily ignored the warning.
            //In a real project, we’ll replace it with cloud service.
#pragma warning disable SKEXP0010 // Suppresses warnings about obsolete APIs or evaluation features
#pragma warning disable SKEXP0001 // Suppresses warnings about obsolete APIs or evaluation features
#pragma warning disable SKEXP0050 // Suppresses warnings about obsolete APIs or evaluation features
            kernelBuilder.AddOpenAITextEmbeddingGeneration(
            Config.OpenAI.ModelId,
            Config.OpenAI.ApiKey);
            kernelBuilder.AddInMemoryVectorStoreRecordCollection<string, TextSnippet<string>>("TestRag");


            //replace with file content
            string ragFileContent = $"מתי הזריחה? ב6 בבוקר {Environment.NewLine} מתי השקיעה? ב6 בערב";
            var records = new List<MemoryRecord>();
            var lines = ragFileContent.Split(Environment.NewLine);
            for (int i = 0; i < lines.Length; i++)
            {
                records.Add(new MemoryRecord(new MemoryRecordMetadata(true, i.ToString(), lines[i], $"Line:{i}", "", ""), new float[] { 0.1f, 0.2f, 0.3f }, i.ToString()));
            }

            var vectorStoreRecordCollection = new VolatileMemoryStore();


            // Adding sample records to the VectorStoreRecordCollection
            vectorStoreRecordCollection.UpsertBatchAsync("test", records);
            kernelBuilder = kernelBuilder.AddVectorStoreTextSearch<MemoryRecord>();
            _kernel = kernelBuilder.Build();
        }

        public async Task<List<string>> GetMessageAsync(ChatHistory chatHistory)
        {
            var chatResponse = _kernel.InvokePromptStreamingAsync(
                promptTemplate: """
                    Please use this information to answer the question:
                    {{#with (SearchPlugin-GetTextSearchResults question)}}  
                      {{#each this}}  
                        Name: {{Name}}
                        Value: {{Value}}
                        Link: {{Link}}
                        -----------------
                      {{/each}}
                    {{/with}}

                    Include citations to the relevant information where it is referenced in the response.
                    
                    Question: {{question}}
                    """,
                arguments: new KernelArguments()
                {
                    { "question", chatHistory },
                },
                templateFormat: "handlebars");

            List<string> response = new List<string>();
            await foreach (var message in chatResponse.ConfigureAwait(false))
            {
                response.Add(message.ToString());
            }
            return response;
        }
    }
}
