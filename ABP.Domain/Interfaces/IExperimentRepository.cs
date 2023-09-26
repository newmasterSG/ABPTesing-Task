namespace ABP.Domain.Repository
{
    public interface IExperimentRepository
    {
        Task<List<string>> GetAllExperimentsAsync();
    }
}