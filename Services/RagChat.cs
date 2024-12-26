using DotnetChatbot.Helpers;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using DotnetChatbot.Interfaces;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;


namespace DotnetChatbot.Services
{
    internal class RagChat : ChatInterface
    {

        //I used the InMemory API, which is currently marked as deprecated, so I temporarily ignored the warning.
        //In a real project, we’ll replace it with cloud service.
#pragma warning disable SKEXP0010 // Suppresses warnings about obsolete APIs or evaluation features
#pragma warning disable SKEXP0001 // Suppresses warnings about obsolete APIs or evaluation features
#pragma warning disable SKEXP0050 // Suppresses warnings about obsolete APIs or evaluation features
        public RagChat() { }

        
        private Kernel _kernel;
        private SemanticTextMemory memory;


        public void init()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var httpClient = new HttpClient(handler);

            var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
            modelId: Config.OpenAI.ModelId!,
            apiKey: Config.OpenAI.ApiKey!,httpClient:httpClient);

            string collectionName = "testRag";

     
            kernelBuilder.AddOpenAITextEmbeddingGeneration(
            Config.OpenAI.ModelId,
            Config.OpenAI.ApiKey);

            kernelBuilder.AddInMemoryVectorStore();
            kernelBuilder.AddVectorStoreTextSearch<ISemanticTextMemory>();

            _kernel = kernelBuilder.Build();
            
            //replace with file content
            string ragFileContent = $"מתי הזריחה? ב6 בבוקר {Environment.NewLine} מתי השקיעה? ב6 בערב";
            var records = new List<MemoryRecord>();
            var lines = ragFileContent.Split(Environment.NewLine);
            for (int i = 0; i < lines.Length; i++)
            {
                records.Add(new MemoryRecord(new MemoryRecordMetadata(true, i.ToString(), lines[i], $"Line:{i}", "", ""), new float[] { 0.1f, 0.2f, 0.3f }, i.ToString()));
            }

            var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
             memory = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGenerator);


            var tasks = records.Select(record =>
                memory.SaveInformationAsync(
                    collection: collectionName,
                    id: record.Key,
                    text: record.Metadata.Text,
                    description: record.Metadata.Description
                )
            );
        }

        public async Task<List<string>> GetMessageAsync(ChatHistory chatHistory)
        {
            var lastMessageContent = chatHistory.LastOrDefault()?.InnerContent?.ToString() ?? string.Empty;

            var searchResults = memory.SearchAsync("testRag",lastMessageContent , limit: 5,kernel:_kernel);

            var context = string.Join("\n", searchResults.ToBlockingEnumerable().Select(result => result.Metadata.Text));

                            var prompt = $@"
                Based on the following retrieved documents:
                {context}

                Answer the user's question {chatHistory } in detail.";

            var response = await  _kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(prompt);
            return new List<string>([((ChatMessageContent)response).Content]);

        }
    }
}