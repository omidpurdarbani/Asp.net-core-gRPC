using System.Text;
using Microsoft.Extensions.Logging;

namespace Message.Splitter.Services
{
    public class MessageService
    {
        private readonly ILogger<MessageService> _logger;
        private readonly Random _random;

        private async Task<(string Id, string Sender, string Message)> GenerateRandomMessage()
        {
            await Task.Delay(200);
            return (Guid.NewGuid().ToString(), "Legal", LoremIpsum(10 + _random.Next(30), 41 + _random.Next(30), 1, 1, 1));
        }

        private static string LoremIpsum(int minWords, int maxWords, int minSentences, int maxSentences, int numLines)
        {
            var words = new[] { "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer", "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat" };

            var rand = new Random();
            int numSentences = rand.Next(maxSentences - minSentences) + minSentences + 1;
            int numWords = rand.Next(maxWords - minWords) + minWords + 1;

            var sb = new StringBuilder();
            for (int p = 0; p < numLines; p++)
            {
                for (int s = 0; s < numSentences; s++)
                {
                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0) { sb.Append(" "); }
                        sb.Append(words[rand.Next(words.Length)]);
                    }
                    sb.Append(". ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
