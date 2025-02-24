using TVOnline.Repository.Job;

namespace TVOnline.Service.Jobs
{
    public class JobsService(IJobsRepository jobsRepository) : IJobsService
    {
        private readonly IJobsRepository _jobsRepository = jobsRepository;
    }
}
