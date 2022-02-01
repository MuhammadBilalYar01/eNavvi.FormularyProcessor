namespace eNavvi.FormularyProcessor.Interfaces
{
    public interface IRxNormUtility
    {
        Task<List<T>> GetData<T>(string blobId) where T : class;
    }
}
