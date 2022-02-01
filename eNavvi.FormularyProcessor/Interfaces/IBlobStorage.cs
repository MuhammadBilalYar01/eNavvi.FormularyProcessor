using eNavvi.FormularyProcessor.Models;

namespace eNavvi.FormularyProcessor.Interfaces
{
    public interface IBlobStorage
    {
        public Task<string> DownloadBlob(string containerId, string blobId);
        public Task UploadBlob(string containerId, string blobId, string content);
        public Task MergeFormulary(string blobId, List<StandardizePlan> content, bool IsProcessed);
        public Task MergeRxcui(List<string> rxcuis);
        public List<string> ListAllBlobs(string containerId);
        void DeleteBlobs(string containerId, List<string> planSettings);
    }
}
