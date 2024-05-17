using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace Message.Splitter.Helper
{
    public class Tools
    {

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
