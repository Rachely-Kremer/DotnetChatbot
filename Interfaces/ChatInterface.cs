using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetChatbot.Interfaces
{
    internal interface ChatInterface
    {

        public void init();
        public Task<List<string>> GetMessageAsync(ChatHistory chatHistory);

    }
}
