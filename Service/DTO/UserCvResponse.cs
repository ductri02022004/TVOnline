namespace TVOnline.Service.DTO
{
    public class UserCvResponse
    {
        public string CvId { get; set; }
        public string? UserId { get; set; }
        public string? CvFileUrl { get; set; }
        public string? CvStatus { get; set; }
        public string? PostId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            UserCvResponse userCvResponse = (UserCvResponse)obj;
            return (CvId == userCvResponse.CvId) && (UserId == userCvResponse.UserId) && (CvFileUrl == userCvResponse.CvFileUrl) && (CvStatus == userCvResponse.CvStatus) && (PostId == userCvResponse.PostId);
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    public static class UserCvResponseExtension
    {
        public static UserCvResponse ToUserCvResponse(this Models.UserCV userCv)
        {
            return new UserCvResponse
            {
                CvId = userCv.CvID,
                UserId = userCv.UserId,
                CvFileUrl = userCv.CVFileUrl,
                CvStatus = userCv.CVStatus,
                PostId = userCv.PostId
            };
        }
    }
}
