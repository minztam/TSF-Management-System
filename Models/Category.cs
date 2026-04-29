using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyQuanAn.Web.Models
{
    [Table("tb_Categories")]
    public class Category
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [Display(Name = "Tên danh mục")]
        [StringLength(100)]
        public required string Name { get; set; } = string.Empty;

        [Display(Name = "Thứ tự hiển thị")]
        [Range(1, 100, ErrorMessage = "Thứ tự từ 1 đến 100")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // public virtual ICollection<Dish> Dishes { get; set; }
    }
}
