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
        internal interface ChatInterface
        {
            void init();
            Task<List<string>> GetMessageAsync(ChatHistory chatHistory);
        }
    }
}
