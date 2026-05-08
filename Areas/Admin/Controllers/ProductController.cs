using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyQuanAn.Web.Areas.Admin.Models.Product;
using QuanLyQuanAn.Web.Data;
using QuanLyQuanAn.Web.Models;

namespace QuanLyQuanAn.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly FoodDBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(FoodDBContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, Guid? categoryId, bool? status, int page = 1)
        {
            ViewBag.Title = "Quản lý thực đơn";
            int pageSize = 10;

            var query = _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Id.ToString() == searchTerm);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.IsActive == status.Value);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1) page = 1;

            var items = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;
            ViewBag.TotalCount = totalItems;

            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentStatus = status;

            return View(items);
        }

        [HttpGet]
        public IActionResult Create() 
        {
            var categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
            }

            try
            {
                string uniqueFileName = "default-food.png";

                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                }

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Price = model.Price,
                    Description = model.Description,
                    IsAvailable = model.IsAvailable,
                    CategoryId = model.CategoryId,
                    ImageUrl = "/images/products/" + uniqueFileName,
                    CreatedDate = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm món ăn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var viewModel = new ProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                IsAvailable = product.IsAvailable,
                IsActive = product.IsActive,
                ImageUrl = product.ImageUrl
            };

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductVM productVM)
        {
            if (id != productVM.Id)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ (ID mismatch)." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

                    if (existingProduct == null)
                    {
                        return Json(new { success = false, message = "Sản phẩm không tồn tại trên hệ thống." });
                    }

                    if (productVM.ImageFile != null && productVM.ImageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProduct.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productVM.ImageFile.FileName);
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        var fullPath = Path.Combine(folderPath, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await productVM.ImageFile.CopyToAsync(stream);
                        }

                        existingProduct.ImageUrl = "/images/products/" + fileName;
                    }

                    existingProduct.Name = productVM.Name;
                    existingProduct.Price = productVM.Price;
                    existingProduct.CategoryId = productVM.CategoryId;
                    existingProduct.Description = productVM.Description;
                    existingProduct.IsAvailable = productVM.IsAvailable;
                    existingProduct.IsActive = productVM.IsActive;

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Đã cập nhật thông tin món ăn thành công!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
                }
            }

            var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            return Json(new { success = false, message = firstError ?? "Vui lòng kiểm tra lại thông tin nhập vào." });
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();
            return PartialView("_DetailsPartial", product);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecipePartial(Guid id, string name)
        {
            var recipes = await _context.ProductIngredients
                .Include(r => r.Ingredient)
                .Where(r => r.ProductId == id)
                .ToListAsync();

            ViewBag.Ingredients = await _context.Ingredients
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToListAsync();

            ViewBag.ProductId = id;
            ViewBag.ProductName = name;

            return PartialView("_RecipeModalPartial", recipes);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRecipe([FromBody] List<ProductIngredient> ingredients)
        {
            try
            {
                if (ingredients == null || !ingredients.Any())
                {
                    return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ." });
                }

                var productId = ingredients.First().ProductId;

                var oldIngredients = _context.ProductIngredients
                                             .Where(x => x.ProductId == productId);

                _context.ProductIngredients.RemoveRange(oldIngredients);

                foreach (var item in ingredients)
                {
                    if (item.IngredientId != Guid.Empty && !string.IsNullOrEmpty(item.Quantity))
                    {
                        var newEntry = new ProductIngredient
                        {
                            ProductId = productId,
                            IngredientId = item.IngredientId,
                            Quantity = item.Quantity.Trim()
                        };
                        _context.ProductIngredients.Add(newEntry);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật định lượng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
