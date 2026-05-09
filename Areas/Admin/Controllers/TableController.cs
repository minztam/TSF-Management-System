using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
        {
            return View();
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
    }
}
