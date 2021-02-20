﻿using System;
using System.IO;
using Azure.Storage.Blobs;
using CharlieBackend.Core;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using CharlieBackend.Business.Services.Interfaces;
using CharlieBackend.Core.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace CharlieBackend.Business.Services
{
    public class BlobService : IBlobService
    {
        private readonly AzureStorageBlobAccount _blobAccount;
        private readonly ILogger<BlobService> _logger;

        public BlobService(
                        AzureStorageBlobAccount blobAccount,
                        ILogger<BlobService> logger
                           )
        {
            _blobAccount = blobAccount;
            _logger = logger;
        }

        public string GetUrl(Attachment attachment)
        {
            //StorageCredentials credentials = new StorageCredentials("csb10032000fbf86473", "3Naz0PXXBe0Lie7HV51jdZsSFCqThDMsqGWdENueI/d2OoV14j6o9Hh0lY1TvAtM8g0VIuPQLDDmEruu951NZA==");
            //CloudStorageAccount storageAccount = new CloudStorageAccount(credentials, "core.windows.net",true);

            //var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            //var container = cloudBlobClient.GetContainerReference(attachment.ContainerName);
            //var blob = container.GetBlockBlobReference(attachment.FileName);

            //BlobContainerClient container2 =
            //            new BlobContainerClient(_blobAccount.ConnectionString, attachment.ContainerName);

            //var client = container2.GetBlobClient(attachment.FileName);
            //client.

            //return blob.Uri.AbsoluteUri;

            BlobClient blob = new BlobClient
                       (
                       _blobAccount.ConnectionString,
                       attachment.ContainerName,
                       attachment.FileName
                       );

            return blob.Uri.AbsoluteUri;
        }

        public async Task<BlobClient> UploadAsync(string fileName, Stream fileStream)
        {
            string containerName = Guid.NewGuid().ToString("N");

            BlobContainerClient container =
                        new BlobContainerClient(_blobAccount.ConnectionString, containerName);

            await container.CreateIfNotExistsAsync();

            BlobClient blob = container.GetBlobClient(fileName);

            _logger.LogInformation("FileName: " + fileName);
            _logger.LogInformation("Uri: " + blob.Uri);

            await blob.UploadAsync(fileStream);

            return blob;
        }

        public async Task<BlobDownloadInfo> DownloadAsync(string containerName, string fileName)
        {
            BlobClient blob = new BlobClient
                       (
                       _blobAccount.ConnectionString,
                       containerName,
                       fileName
                       );

            BlobDownloadInfo download = await blob.DownloadAsync();

            return download;
        }

        public async Task<bool> DeleteAsync(string containerName)
        {
            BlobContainerClient container = new BlobContainerClient
                        (
                        _blobAccount.ConnectionString,
                        containerName
                        );

            var response = await container.DeleteIfExistsAsync();

            return response;
        }
    }
}
