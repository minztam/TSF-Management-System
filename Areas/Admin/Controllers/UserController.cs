using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuanLyQuanAn.Web.Data;
using QuanLyQuanAn.Web.Models;
using System.Drawing;
using System.Security.Claims;

namespace QuanLyQuanAn.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly FoodDBContext _context;

        public UserController(FoodDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, string role, string status, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(u => u.FullName.Contains(searchTerm)
                                      || u.Email.Contains(searchTerm)
                                      || u.PhoneNumber!.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(role) && role != "Tất cả vai trò")
            {
                query = query.Where(u => u.Role == role);
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả trạng thái")
            {
                if (status == "locked")
                    query = query.Where(u => u.IsActive == false); // Hoặc logic status của bạn
                else if (status == "active")
                    query = query.Where(u => u.IsActive == true);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var users = await query
                .OrderByDescending(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentRole"] = role;
            ViewData["CurrentStatus"] = status;

            var model = new UserListViewModel
            {
                Users = users,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,PhoneNumber,Role,Password")] User user)
        {
            ModelState.Remove("Username");

            if (ModelState.IsValid)
            {
                try
                {
                    user.Id = Guid.NewGuid();
                    user.Username = user.Email;
                    user.CreatedAt = DateTime.Now;
                    user.IsActive = true;

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Thêm nhân viên thành công!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi lưu Database: " + ex.Message });
                }
            }

            var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = errorMessages });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [FromBody] User user)
        {
            if (id != user.Id)
            {
                return Json(new { success = false, message = "ID người dùng không khớp!" });
            }

            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (existingUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên này." });
                }

                if (!string.IsNullOrWhiteSpace(user.FullName))
                {
                    existingUser.FullName = user.FullName;
                }

                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    existingUser.Email = user.Email;
                }

                if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    existingUser.PhoneNumber = user.PhoneNumber;
                }

                if (!string.IsNullOrWhiteSpace(user.Role))
                {
                    existingUser.Role = user.Role;
                }

                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    existingUser.Password = user.Password;
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Cập nhật thông tin nhân viên {existingUser.FullName} thành công!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên này." });
                }

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                string statusText = user.IsActive ? "Mở khóa" : "Khóa";
                return Json(new
                {
                    success = true,
                    message = $"Đã {statusText} tài khoản {user.FullName} thành công!",
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return PartialView("_UserDetailsPartial", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Json(new { success = false, message = "Phiên làm việc hết hạn, vui lòng đăng nhập lại." });
            }

            var currentUserId = Guid.Parse(userIdClaim);

            if (id == currentUserId)
            {
                return Json(new { success = false, message = "Bạn không thể tự xóa chính tài khoản của mình!" });
            }

            if (id == Guid.Empty)
            {
                return Json(new { success = false, message = "ID không hợp lệ." });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy nhân viên này." });
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa nhân viên thành công!" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Không thể xóa nhân viên này vì có dữ liệu liên quan (hóa đơn, lịch sử trực...)." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            var userList = await _context.Users
                .OrderByDescending(u => u.Role)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("NhanSu_TrungSonFood");

                worksheet.Cells["A1:F1"].Merge = true;
                worksheet.Cells["A1"].Value = "DANH SÁCH NHÂN SỰ TRUNG SƠN FOOD";
                worksheet.Cells["A1"].Style.Font.Size = 18;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.DarkBlue);
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                string[] headers = { "STT", "Họ Tên", "Email", "Số Điện Thoại", "Chức Vụ", "Trạng Thái" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[3, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 102, 204));
                    cell.Style.Font.Color.SetColor(Color.White);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                int startRow = 4;
                for (int i = 0; i < userList.Count; i++)
                {
                    var item = userList[i];
                    worksheet.Cells[startRow, 1].Value = i + 1;
                    worksheet.Cells[startRow, 2].Value = item.FullName;
                    worksheet.Cells[startRow, 3].Value = item.Email;
                    worksheet.Cells[startRow, 4].Value = item.PhoneNumber;
                    worksheet.Cells[startRow, 5].Value = item.Role;
                    worksheet.Cells[startRow, 6].Value = item.IsActive ? "Đang làm việc" : "Đã khóa";

                    for (int col = 1; col <= 6; col++)
                    {
                        worksheet.Cells[startRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    worksheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[startRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    startRow++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileData = package.GetAsByteArray();
                var fileName = $"NhanSu_TrungSon_{DateTime.Now:ddMMyyyy_HHmm}.xlsx";

                return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    } 
}
