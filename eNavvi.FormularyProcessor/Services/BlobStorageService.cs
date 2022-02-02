using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using eNavvi.FormularyProcessor.Interfaces;
using eNavvi.FormularyProcessor.Models;
using eNavvi.FormularyProcessor.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace eNavvi.FormularyProcessor.Services
{
    public class BlobStorageService : IBlobStorage
    {
        private readonly Configurations _config;
        private readonly ILogger<BlobStorageService> log;

        public BlobStorageService(IConfigurationRoot config, ILogger<BlobStorageService> log)
        {
            Configurations option = new Configurations();
            config.Bind(option);
            this._config = option;
            this.log = log;
        }

        private BlobContainerClient Container(string containerId)
        {

            BlobContainerClient containerClient = new BlobContainerClient(this._config.AzureStorageConnection, containerId);
            containerClient.CreateIfNotExists(PublicAccessType.None);
            this.log.LogInformation($"Container connected to {containerId}");
            return containerClient;
        }

        public async Task<string> DownloadBlob(string containerId, string blobId)
        {
            try
            {
                BlobClient client = this.Container(containerId).GetBlobClient(blobId);

                if (await client.ExistsAsync())
                {
                    var blobToDownload = client.Download().Value;

                    var downloadBuffer = new byte[81920];
                    int bytesRead;
                    int totalBytesDownloaded = 0;
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var progress = new ProgressBar())
                        {
                            while ((bytesRead = blobToDownload.Content.Read(downloadBuffer, 0, downloadBuffer.Length)) != 0) //Step 3
                            {
                                memoryStream.Write(downloadBuffer, 0, bytesRead);
                                totalBytesDownloaded += bytesRead;

                                progress.Report(GetProgressPercentage(blobToDownload.ContentLength, totalBytesDownloaded));
                            }
                        }
                        blobToDownload.Content.Close();
                        string content = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                        return content;
                    }
                }
                else
                {
                    this.log.LogWarning($"Blob({blobId}) does not exist.");
                    return "[]";
                }
            }
            catch (Exception ex)
            {
                this.log.LogError(ex, ex.Message);
                throw;
            }
        }
        private double GetProgressPercentage(double totalSize, double currentSize)
        {
            return (currentSize / totalSize);
        }
        public async Task UploadBlob(string containerId, string blobId, string content)
        {
            try
            {
                BlobClient client = this.Container(containerId).GetBlobClient(blobId);
                await client.DeleteIfExistsAsync();

                var stream = this.GetStream(content);
                await client.UploadAsync(stream);
            }
            catch (Exception ex)
            {
                this.log.LogError(ex, ex.Message);
                throw;
            }
        }

        private Stream GetStream(object data)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public async Task MergeRxcui(List<string> rxcuis)
        {
            string existing = await this.DownloadBlob(this._config.Related_Container, Constants.UN_ROCESSED_RXCUI);
            List<string> existingRxcuis = JsonConvert.DeserializeObject<List<string>>(existing);
            log.LogInformation("Existing Rxcuis: " + existingRxcuis.Count());
            log.LogInformation("New Rxcuis: " + rxcuis.Count());
            List<string> newRxcuis = rxcuis.Union(existingRxcuis).ToList();
            log.LogInformation("Total unique Rxcuis: " + rxcuis.Count());
            string content = JsonConvert.SerializeObject(newRxcuis);
            await this.UploadBlob(this._config.Related_Container, Constants.UN_ROCESSED_RXCUI, content);
        }

        public async Task MergeFormulary(string blobId, List<StandardizePlan> content)
        {
            //if (IsProcessed)
            //{
            var data = JsonConvert.SerializeObject(content);
            await this.UploadBlob(this._config.Formulary_Container, blobId, data);
            //}
            //else
            //{
            //    string existing = await this.DownloadBlob(this._config.Formulary_Container, blobId);
            //    if (existing != "[]")
            //    {
            //        var existingPlan = JsonConvert.DeserializeObject<List<StandardizePlan>>(existing);
            //        log.LogInformation("Existing Formulary: " + existingPlan.Count());
            //        log.LogInformation("New Formulary: " + content.Count());
            //        var unique = existingPlan.Union(content);
            //        log.LogInformation("Total unique Formulary: " + unique.Count());
            //        content = unique.ToList();
            //    }
            //    var data = JsonConvert.SerializeObject(content);
            //    await this.UploadBlob(this._config.Formulary_Container, blobId, data);
            //}
        }

        public List<string> ListAllBlobs(string containerId)
        {
            var container = this.Container(containerId);
            List<string> names = new List<string>();
            foreach (BlobItem blob in container.GetBlobs())
            {
                names.Add(blob.Name);
            }

            return names;
        }

        public void DeleteBlobs(string containerId, List<string> blobs)
        {
            try
            {
                var container = this.Container(containerId);
                foreach (var blob in blobs)
                {
                    log.LogInformation("Deleting: " + containerId + "/" + blob);
                    container.DeleteBlobIfExists(blob);
                }
            }
            catch (Exception ex)
            {
                log.LogWarning(ex.Message, ex);
            }
        }
    }
}
