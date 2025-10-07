using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AbCRetailers.Models;
using AbCRetailers.Models.AbCRetailers.Models;

namespace AbCRetailers.Services
{
    public class AzureStorageService : IAzureStorageService
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly QueueServiceClient _queueServiceClient;
        private readonly ShareServiceClient _shareServiceClient;
        private readonly ILogger<AzureStorageService> _logger;

        public AzureStorageService(IConfiguration configuration, ILogger<AzureStorageService> logger)
        {
            string connectionString = configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException("Azure Storage connection string not found");

            _tableServiceClient = new TableServiceClient(connectionString);
            _blobServiceClient = new BlobServiceClient(connectionString);
            _queueServiceClient = new QueueServiceClient(connectionString);
            _shareServiceClient = new ShareServiceClient(connectionString);
            _logger = logger;

            InitializeStorageAsync().Wait();
        }

        private async Task InitializeStorageAsync()
        {
            await _tableServiceClient.CreateTableIfNotExistsAsync("Customers");
            await _tableServiceClient.CreateTableIfNotExistsAsync("Products");
            await _tableServiceClient.CreateTableIfNotExistsAsync("Orders");
            await _blobServiceClient.GetBlobContainerClient("product-images").CreateIfNotExistsAsync();
            await _blobServiceClient.GetBlobContainerClient("payment-proofs").CreateIfNotExistsAsync();
            await _queueServiceClient.GetQueueClient("order-notifications").CreateIfNotExistsAsync();
            await _queueServiceClient.GetQueueClient("stock-updates").CreateIfNotExistsAsync();
            var shareClient = _shareServiceClient.GetShareClient("contracts");
            await shareClient.CreateIfNotExistsAsync();
            await shareClient.GetDirectoryClient("payments").CreateIfNotExistsAsync();
        }

        private static string GetTableName<T>() => typeof(T).Name switch
        {
            nameof(Customer) => "Customers",
            nameof(Product) => "Products",
            nameof(Order) => "Orders",
            _ => typeof(T).Name + "s"
        };

        // Table Storage Methods

        public async Task<List<T>> GetAllEntitiesAsync<T>() where T : class, ITableEntity, new()
        {
            var tableClient = _tableServiceClient.GetTableClient(GetTableName<T>());
            var results = new List<T>();
            await foreach (var entity in tableClient.QueryAsync<T>())
                results.Add(entity);
            return results;
        }

        public async Task<List<T>> GetAllEntitiesAsync<T>(string tableName) where T : class, ITableEntity, new()
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            var entities = new List<T>();
            await foreach (var entity in tableClient.QueryAsync<T>())
                entities.Add(entity);
            return entities;
        }

        public async Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var tableClient = _tableServiceClient.GetTableClient(GetTableName<T>());
            try
            {
                var result = await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return result.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<T> AddEntityAsync<T>(T entity) where T : class, ITableEntity
        {
            var tableClient = _tableServiceClient.GetTableClient(GetTableName<T>());
            await tableClient.AddEntityAsync(entity);
            return entity;
        }

        public async Task<T> UpdateEntityAsync<T>(T entity) where T : class, ITableEntity
        {
            var tableClient = _tableServiceClient.GetTableClient(GetTableName<T>());
            await tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
            return entity;
        }

        public async Task DeleteEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var tableClient = _tableServiceClient.GetTableClient(GetTableName<T>());
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        // Blob Storage Methods

        public async Task<string> UploadBlobAsync(string containerName, IFormFile file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(file.FileName);
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        public async Task DeleteBlobAsync(string blobName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        // Queue Storage Methods

        public async Task SendMessageAsync(string queueName, string message)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
        }

        public async Task<string?> ReceiveMessageAsync(string queueName)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            var response = await queueClient.ReceiveMessageAsync();
            return response?.Value?.MessageText;
        }

        // Azure File Share Methods

        public async Task<string> UploadToFileShareAsync(IFormFile file, string shareName, string directoryName)
        {
            var shareClient = _shareServiceClient.GetShareClient(shareName);
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            await directoryClient.CreateIfNotExistsAsync();

            var fileClient = directoryClient.GetFileClient(file.FileName);
            using var stream = file.OpenReadStream();
            await fileClient.CreateAsync(stream.Length);
            await fileClient.UploadAsync(stream);

            return fileClient.Uri.ToString();
        }

        public async Task<byte[]> DownloadFromFileShareAsync(string shareName, string fileName, string directoryName)
        {
            var shareClient = _shareServiceClient.GetShareClient(shareName);
            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            var fileClient = directoryClient.GetFileClient(fileName);

            var response = await fileClient.DownloadAsync();
            using var ms = new MemoryStream();
            await response.Value.Content.CopyToAsync(ms);
            return ms.ToArray();
        }

        // Legacy Compatibility Methods

        public Task UpdateEntityAsync(Customer original) => UpdateEntityAsync<Customer>(original);

        public Task UpdateEntityAsync(string v, Product original) => UpdateEntityAsync<Product>(original);

        public Task AddEntityAsync(string v, Product product) => AddEntityAsync<Product>(product);

        public Task UploadBlobAsync(string v, IFormFile file, string fileName) => UploadBlobAsync(v, file);

        public Task<string> UploadImageAsync(IFormFile file, string containerName) => UploadBlobAsync(containerName, file);

        public Task<string> UploadFileAsync(IFormFile file, string containerName) => UploadBlobAsync(containerName, file);

        public Task<object> UpdateEntityAsync(Order original)
        {
            throw new NotImplementedException();
        }
    }
}