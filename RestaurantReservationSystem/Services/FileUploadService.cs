using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RestaurantReservationSystem.Services
{
    public class FileUploadService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public FileUploadService(string ConnectionString, string ContainerName)
        {
            _connectionString = ConnectionString;
            _containerName = ContainerName;
        }

        public async Task<string> UploadLogoToBlobAsync(IFormFile logoFile)
        {
            if (logoFile == null)
            {
                return null; // return null if no file is uploaded
            }

            // set a maximum file size limit of 2MB
            const long maxFileSize = 2 * 1024 * 1024; // 2MB
            if (logoFile.Length > maxFileSize)
            {
                throw new Exception("File size exceeds the maximum limit of 2MB.");
            }

            // generate a unique file name
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFile.FileName);

            // get a reference to the Blob Service Client
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            
            // ensure the container exists
            await blobContainerClient.CreateIfNotExistsAsync();

            // get a reference to the Blob Client
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            // upload the file to Azure Blob Storage
            using (var stream = logoFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true); // set overwrite to true to replace existing file
            }

            // return the URL of the uploaded file
            return blobClient.Uri.ToString();
        }
    }
}
