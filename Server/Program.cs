using System;
using System.Net;
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
        TcpListener server = new TcpListener(IPAddress.Any, 8888);
        server.Start();
        Console.WriteLine("Server started...");

        while (true)
        {
            TcpClient client = await server.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
            Console.WriteLine("Encrypted message received: " + BitConverter.ToString(buffer, 0, bytesRead));

            byte[] decryptedMessage = Decrypt(buffer, bytesRead);
            Console.WriteLine("Decrypted message: " + Encoding.UTF8.GetString(decryptedMessage));

            byte[] responseMessage = Encrypt(Encoding.UTF8.GetBytes("Message received"));
            await stream.WriteAsync(responseMessage, 0, responseMessage.Length);
        }

        client.Close();
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
