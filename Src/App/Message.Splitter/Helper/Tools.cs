using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace Message.Splitter.Helper
{
    public class Tools
    {
        private readonly Random _random;

        public Tools()
        {
            _random = new Random();
        }

        public async Task<(string Id, string Sender, string Message)> GenerateRandomMessage()
        {
            await Task.Delay(200);
            return (Guid.NewGuid().ToString(), "Legal", LoremIpsum(10 + _random.Next(30), 41 + _random.Next(30), 1, 1, 1));
        }

        private string LoremIpsum(int minWords, int maxWords, int minSentences, int maxSentences, int numLines)
        {
            var words = new[] { "lorem", "ipsum", "dolor", "sit", "48948", "amet", "consectetuer", "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "544", "euismod", "tincidunt", "ut", "laoreet", "dolore", "15646546", "magna", "aliquam", "erat" };

            int numSentences = _random.Next(maxSentences - minSentences) + minSentences + 1;
            int numWords = _random.Next(maxWords - minWords) + minWords + 1;

            var sb = new StringBuilder();
            for (int p = 0; p < numLines; p++)
            {
                for (int s = 0; s < numSentences; s++)
                {
                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0) { sb.Append(" "); }
                        sb.Append(words[_random.Next(words.Length)]);
                    }
                    sb.Append(". ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static Guid GenerateGuid()
        {
            var random = new Random();
            var macAddress = GetMacAddress();
            var text = macAddress + random.Next();
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(text));
            return new Guid(hash);
        }

        private static string GetMacAddress()
        {
            Dictionary<string, long> macAddresses = new();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                    macAddresses[nic.GetPhysicalAddress().ToString()] = nic.GetIPStatistics().BytesSent + nic.GetIPStatistics().BytesReceived;
            }
            long maxValue = 0;
            string mac = "";
            foreach (KeyValuePair<string, long> pair in macAddresses)
            {
                if (pair.Value > maxValue)
                {
                    mac = pair.Key;
                    maxValue = pair.Value;
                }
            }
            return mac;
        }
    }
}
