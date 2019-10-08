// ReSharper disable UnusedMember.Local
using System;
using System.IO;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace FileProcessing
{
    public static class Process
    {
        [FunctionName("ProcessFromQueue")]
        public static void ProcessFromQueue(
            [ServiceBusTrigger("ProcessFile01", Connection = "ServiceBusConnection")]Message incomingMessage,
            ILogger log)
        {
            log.LogInformation($"{incomingMessage.MessageId} received");
        }


        private static byte[] GetFileContent(Stream file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        private static string GetFileContent(string storageAccountConnectionString, string fileName)
        {
            var storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("files");
            cloudBlobContainer.CreateIfNotExists();
            var blob = cloudBlobContainer.GetBlockBlobReference(fileName);
            return blob.DownloadText();
        }
    }

    public class ProcessFileMessage
    {
        public string FileName { get; set; }
    }

    public class FileContentEntity : TableEntity
    {
        public FileContentEntity(string fileName, string content)
        {
            Content = content;
            PartitionKey = fileName;
            RowKey = Guid.NewGuid().ToString();
        }
        public string Content { get; set; }
    }
}