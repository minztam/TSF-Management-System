using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanAn.Web.Data;
using QuanLyQuanAn.Web.Models;

namespace QuanLyQuanAn.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class WarehouseController : Controller
    {
        private readonly FoodDBContext _context;
        public WarehouseController(FoodDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.Ingredients
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.Name)
                .ToListAsync();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string trimmedName = ingredient.Name.Trim();
                    bool isExist = await _context.Ingredients
                        .AnyAsync(x => x.Name.ToLower() == trimmedName.ToLower() && x.IsActive == true);

                    if (isExist)
                    {
                        TempData["Error"] = $"Nguyên liệu '{trimmedName}' đã tồn tại trong danh sách!";
                        return RedirectToAction(nameof(Index));
                    }

                    ingredient.Id = Guid.NewGuid();
                    ingredient.Name = trimmedName;
                    ingredient.IsActive = true;

                    _context.Ingredients.Add(ingredient);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Thêm mới nguyên liệu thành công vào kho Trung Sơn Food!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                }
            }
            else
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["Error"] = "Dữ liệu nhập không hợp lệ: " + errors;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetIngredient(Guid id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return NotFound();

            return Json(new
            {
                id = ingredient.Id,
                name = ingredient.Name,
                unit = ingredient.Unit,
                quantity = ingredient.Quantity
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Ingredients.FindAsync(ingredient.Id);
                    if (existing == null) return NotFound();

                    existing.Name = ingredient.Name.Trim();
                    existing.Unit = ingredient.Unit;
                    existing.Quantity = ingredient.Quantity;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật nguyên liệu thành công!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi: " + ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
