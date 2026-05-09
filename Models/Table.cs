using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyQuanAn.Web.Models
{
    [Table("tb_Table")]
    public class Table
    {
        [Key]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int Capacity { get; set; }
        public string? AreaName { get; set; }
        public string Status { get; set; } = "Available";
        public DateTime? CheckInTime { get; set; }
        
        [NotMapped]
        public string SittingTime
        {
            get
            {
                if (Status == "Occupied" && CheckInTime.HasValue)
                {
                    var duration = DateTime.Now - CheckInTime.Value;
                    int minutes = (int)Math.Max(0, duration.TotalMinutes);
                    return $"{minutes}'";
                }
                return "0'";
            }
        }
    }
}
