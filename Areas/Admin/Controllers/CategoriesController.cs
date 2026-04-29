using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanAn.Web.Areas.Admin.Models.Categories;
using QuanLyQuanAn.Web.Data;
using QuanLyQuanAn.Web.Models;
using System.Threading.Tasks;

namespace QuanLyQuanAn.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly FoodDBContext _context;
        public CategoriesController(FoodDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            ViewBag.Title = "Quản lý Danh mục Món ăn";
            int pageSize = 5; // Bạn có thể chỉnh lên 10 hoặc 20 tùy ý

            var query = _context.Categories.OrderBy(c => c.DisplayOrder);

            // 1. Tính tổng số bản ghi
            int totalItems = await query.CountAsync();

            // 2. Tính tổng số trang
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // 3. Lấy dữ liệu của trang hiện tại
            var categories = await query
                .Skip((page - 1) * pageSize) // Bỏ qua các dòng của các trang trước
                .Take(pageSize)               // Lấy số dòng đúng bằng pageSize
                .ToListAsync();

            // 4. Truyền thông tin phân trang qua ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RequestDTO category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var newCategory = new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = category.Name,
                        DisplayOrder = category.DisplayOrder,
                        IsActive = category.IsActive,
                        CreatedDate = DateTime.Now,
                    };

                    _context.Categories.Add(newCategory);
                    _context.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        message = "Thêm danh mục món ăn thành công!"
                    });
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Lỗi khi thêm danh mục món ăn: {ex.Message}"
                    });
                }
            }

            return Json(new
            {
                success = false,
                message = "Vui lòng kiểm tra lại dữ liệu nhập vào."
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var model = new RequestDTO
            {
                Id = category.Id,
                Name = category.Name,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, errors = errors });
            }

            try
            {
                var category = await _context.Categories.FindAsync(model.Id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Danh mục món ăn không tồn tại." });
                }

                bool isChanged = false;

                if (category.Name != model.Name)
                {
                    category.Name = model.Name;
                    isChanged = true;
                }

                if (category.DisplayOrder != model.DisplayOrder)
                {
                    category.DisplayOrder = model.DisplayOrder;
                    isChanged = true;
                }

                if (category.IsActive != model.IsActive)
                {
                    category.IsActive = model.IsActive;
                    isChanged = true;
                }

                if (isChanged)
                {
                    category.CreatedDate = DateTime.Now;

                    _context.Categories.Update(category);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Đã cập nhật các thay đổi mới." });
                }

                return Json(new { success = true, message = "Không có thay đổi nào được thực hiện." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi cập nhật: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục cần xóa." });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã xóa danh mục {category.Name} thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Không thể xóa danh mục này vì đã có dữ liệu liên quan {ex.Message}."
                });
            }
        }
    } 
}
