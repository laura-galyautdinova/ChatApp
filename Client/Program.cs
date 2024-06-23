using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

class Program
{
    private static readonly byte[] key = Encoding.UTF8.GetBytes("1234567890123456");
    private static readonly byte[] iv = Encoding.UTF8.GetBytes("6543210987654321");

    static async Task Main(string[] args)
    {
        try
        {
            TcpClient client = new TcpClient("127.0.0.1", 8888);
            NetworkStream stream = client.GetStream();

            Console.Write("Enter message: ");
            string? message = Console.ReadLine();

            if (!string.IsNullOrEmpty(message))
            {
                byte[] encryptedMessage = Encrypt(Encoding.UTF8.GetBytes(message));
                Console.WriteLine("Encrypted message sent: " + BitConverter.ToString(encryptedMessage));
                await stream.WriteAsync(encryptedMessage, 0, encryptedMessage.Length);

                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                byte[] decryptedMessage = Decrypt(buffer, bytesRead);

                Console.WriteLine("Encrypted response received: " + BitConverter.ToString(buffer, 0, bytesRead));
                Console.WriteLine("Server response: " + Encoding.UTF8.GetString(decryptedMessage));
            }
            else
            {
                Console.WriteLine("Message cannot be null or empty.");
            }

            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static byte[] Encrypt(byte[] plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            return encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
        }
    }

    static byte[] Decrypt(byte[] cipherText, int count)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            return decryptor.TransformFinalBlock(cipherText, 0, count);
        }
    }
}
