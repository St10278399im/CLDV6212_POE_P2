using AbCRetailers.Models;
using AbCRetailers.Models.AbCRetailers.Models;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;

namespace AbCRetailers.Services
{
    public interface IAzureStorageService
    {
        // Table Storage
        Task<List<T>> GetAllEntitiesAsync<T>() where T : class, ITableEntity, new();
        Task<List<T>> GetAllEntitiesAsync<T>(string tableName) where T : class, ITableEntity, new();
        Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new();
        Task<T> AddEntityAsync<T>(T entity) where T : class, ITableEntity;
        Task<T> UpdateEntityAsync<T>(T entity) where T : class, ITableEntity;
        Task DeleteEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new();

        // Blob Storage
        Task<string> UploadBlobAsync(string containerName, IFormFile file);
        Task DeleteBlobAsync(string blobName, string containerName);

        // Optional: Shortcuts (for naming clarity)
        Task<string> UploadImageAsync(IFormFile file, string containerName);
        Task<string> UploadFileAsync(IFormFile file, string containerName);

        // Queue Storage
        Task SendMessageAsync(string queueName, string message);
        Task<string?> ReceiveMessageAsync(string queueName);

        // File Share Storage
        Task<string> UploadToFileShareAsync(IFormFile file, string shareName, string directoryName = "");
        Task<byte[]> DownloadFromFileShareAsync(string shareName, string fileName, string directoryName = "");

        // Legacy method shortcuts (optional if used in controller)
        Task UpdateEntityAsync(Customer original);
        Task UpdateEntityAsync(string tableName, Product original);
        Task AddEntityAsync(string tableName, Product product);
        Task UploadBlobAsync(string containerName, IFormFile file, string fileName);
        Task<object> UpdateEntityAsync(Order original);
    }
}