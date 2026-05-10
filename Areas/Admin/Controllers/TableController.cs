using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanAn.Web.Areas.Admin.Models.Table;
using QuanLyQuanAn.Web.Data;
using QuanLyQuanAn.Web.Models;
using System.Threading.Tasks;

namespace QuanLyQuanAn.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TableController : Controller
    {
        private readonly FoodDBContext _context;

        public TableController(FoodDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var tables = await _context.Tables
                .OrderBy(t => t.AreaName)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View(tables);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTable table)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var newTable = new Table
                    {
                        Id = Guid.NewGuid(),
                        Name = table.Name,
                        Capacity = table.Capacity,
                        AreaName = table.AreaName,
                        Status = table.Status,
                        CheckInTime = null
                    };

                    _context.Tables.Add(newTable);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Thêm bàn ăn thành công!" });

                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Lỗi khi lưu dữ liệu: " + ex.Message
                    });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new
            {
                success = false,
                message = "Dữ liệu không hợp lệ",
                errors = errors
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            var editTable = new CreateTable
            {
                Id = table.Id,
                Name = table.Name,
                Capacity = table.Capacity,
                AreaName = table.AreaName,
                Status = table.Status
            };

            return View(editTable);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateTable table)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
            }

            try
            {
                var tableInDb = await _context.Tables.FindAsync(table.Id);
                if (tableInDb == null)
                {
                    return Json(new { success = false, message = "Bàn ăn không tồn tại" });
                }

                if (!string.IsNullOrEmpty(table.Name) && tableInDb.Name != table.Name)
                    tableInDb.Name = table.Name;

                if (table.Capacity > 0 && tableInDb.Capacity != table.Capacity)
                    tableInDb.Capacity = table.Capacity;

                if (!string.IsNullOrEmpty(table.AreaName) && tableInDb.AreaName != table.AreaName)
                    tableInDb.AreaName = table.AreaName;

                if (!string.IsNullOrEmpty(table.Status) && tableInDb.Status != table.Status)
                {
                    if (table.Status == "Occupied" && tableInDb.Status != "Occupied")
                    {
                        tableInDb.CheckInTime = DateTime.Now;
                    }

                    else if (table.Status == "Available")
                    {
                        tableInDb.CheckInTime = null;
                    }

                    tableInDb.Status = table.Status;
                }

                if (_context.Entry(tableInDb).State == EntityState.Modified)
                {
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Cập nhật thay đổi thành công!" });
                }

                return Json(new { success = true, message = "Không có thay đổi nào được thực hiện." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var table = await _context.Tables.FindAsync(id);

                if (table == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bàn ăn này hoặc đã bị xóa trước đó." });
                }

                if (table.Status == "Occupied")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa bàn đang có khách! Vui lòng thanh toán hoặc chuyển bàn trước."
                    });
                }

                _context.Tables.Remove(table);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa bàn ăn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi xóa: " + ex.Message });
            }
        }
    }
}
