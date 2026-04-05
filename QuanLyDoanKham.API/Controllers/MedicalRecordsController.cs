using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MedicalRecords
        [HttpGet]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetMedicalRecords()
        {
            return await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.MedicalGroup)
                .Include(r => r.Services).ThenInclude(s => s.ExamService)
                .Select(r => new {
                    r.RecordId,
                    PatientName = r.Patient.FullName,
                    r.ExamDate,
                    r.Status,
                    r.TotalCost
                }).ToListAsync();
        }

        // POST: api/MedicalRecords/QuickCreate
        [HttpPost("QuickCreate")]
        public async Task<ActionResult<MedicalRecord>> QuickCreateRecord([FromBody] MedicalRecord record)
        {
            record.CreatedAt = DateTime.Now;
            record.Status = "CheckIn";
            
            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(record);
        }
    }
}
