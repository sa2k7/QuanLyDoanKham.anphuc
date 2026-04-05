using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExamServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamService>>> GetServices()
        {
            return await _context.ExamServices.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ExamService>> CreateService(ExamService service)
        {
            _context.ExamServices.Add(service);
            await _context.SaveChangesAsync();
            return Ok(service);
        }
    }
}
