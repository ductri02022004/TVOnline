namespace TVOnline.Repository.Employers
{
    public interface IEmployerRepository
    {
        Task<List<Models.Employers>> GetAllEmployers();
        Task<Models.Employers?> GetEmployerById(string employerId);
    }
}
