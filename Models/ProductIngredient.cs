using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyQuanAn.Web.Models
{
    [Table("tb_ProductIngredient")]
    public class ProductIngredient
    {
        [Required]
        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        public Guid IngredientId { get; set; }

        [ForeignKey("IngredientId")]
        public virtual Ingredient? Ingredient { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập định lượng")]
        [StringLength(100)]
        public string? Quantity { get; set; }
    }
}