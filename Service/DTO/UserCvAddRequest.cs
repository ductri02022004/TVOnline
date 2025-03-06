namespace TVOnline.Service.DTO
{
    public class UserCvAddRequest
    {
        public string CvId { get; set; }
        public string? UserId { get; set; }
        public string? CvFileUrl { get; set; }
        public string? CvStatus { get; set; }
        public string? PostId { get; set; }
        public DateTime ApplicationDate { get; set; }

        public UserCV ToUserCv()
        {
            return new UserCV
            {
                CvID = CvId,
                UserId = UserId,
                CVFileUrl = CvFileUrl,
                CVStatus = CvStatus,
                PostId = PostId,
                ApplicationDate = ApplicationDate
            };
        }
    }
}
