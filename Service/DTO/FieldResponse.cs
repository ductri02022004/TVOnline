namespace TVOnline.Service.DTO
{
    public class FieldResponse
    {
        public string Name { get; set; }

        public FieldResponse(string name)
        {
            Name = name;
        }
    }
}