using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyQuanAn.Web.Models
{
    [Table("tb_Ingredient")]
    public class Ingredient
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên nguyên liệu không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn vị tính không được để trống")]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public double Quantity { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<ProductIngredient> ProductIngredients { get; set; } = new List<ProductIngredient>();
    }
}
