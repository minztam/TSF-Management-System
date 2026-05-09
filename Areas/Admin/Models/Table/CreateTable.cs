using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanAn.Web.Areas.Admin.Models.Table
{
    public class CreateTable
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int Capacity { get; set; }
        public string? AreaName { get; set; }
        public string Status { get; set; } = "Available";
    }
}
