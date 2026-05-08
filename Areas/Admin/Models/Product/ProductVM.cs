using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanAn.Web.Areas.Admin.Models.Product
{
    public class ProductVM
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        public string? Description { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsActive { get; set; } = true;


        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public Guid CategoryId { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }
    }
}
