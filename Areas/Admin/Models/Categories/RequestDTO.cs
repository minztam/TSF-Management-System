namespace QuanLyQuanAn.Web.Areas.Admin.Models.Categories
{
    public class RequestDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
