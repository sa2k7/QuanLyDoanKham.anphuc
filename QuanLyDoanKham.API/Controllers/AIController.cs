using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Services;
using QuanLyDoanKham.API.Services.MedicalRecords;
using System.Text;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly IGeminiService _aiService;
        private readonly IExamService _examService;
        private readonly IMedicalRecordService _medicalRecordService;

        public AIController(IGeminiService aiService, IExamService examService, IMedicalRecordService medicalRecordService)
        {
            _aiService = aiService;
            _examService = examService;
            _medicalRecordService = medicalRecordService;
        }

        /// <summary>
        /// POST api/AI/summarize-record/{recordId}
        /// Tóm tắt kết quả khám và đưa ra lời khuyên y tế dựa trên dữ liệu hiện có.
        /// </summary>
        [HttpPost("summarize-record/{recordId}")]
        public async Task<IActionResult> SummarizeMedicalRecord(int recordId)
        {
            try
            {
                // 1. Lấy thông tin hồ sơ và kết quả khám
                var record = await _medicalRecordService.GetByIdAsync(recordId);
                if (record == null) return NotFound("Không tìm thấy hồ sơ bệnh nhân.");

                var examResults = await _examService.GetResultsByRecordIdAsync(recordId);
                if (!examResults.IsSuccess || examResults.Data == null || !examResults.Data.Any())
                {
                    return BadRequest("Chưa có kết quả khám để tóm tắt.");
                }

                // 2. Xây dựng Prompt cho AI
                var promptBuilder = new StringBuilder();
                promptBuilder.AppendLine("Bạn là một trợ lý y tế thông minh. Hãy tóm tắt kết quả khám sức khỏe sau đây một cách chuyên nghiệp, ngắn gọn và đưa ra lời khuyên phù hợp.");
                promptBuilder.AppendLine($"Bệnh nhân: {record.FullName}, Giới tính: {record.Gender}, Năm sinh: {record.DateOfBirth?.Year}");
                promptBuilder.AppendLine("Kết quả các chuyên khoa:");

                foreach (var res in examResults.Data)
                {
                    promptBuilder.AppendLine($"- {res.ExamType}: {res.Diagnosis}");
                    if (res.ResultData != null)
                    {
                        promptBuilder.AppendLine($"  Chi tiết: {System.Text.Json.JsonSerializer.Serialize(res.ResultData)}");
                    }
                }

                promptBuilder.AppendLine("\nYêu cầu phản hồi bằng tiếng Việt, định dạng JSON gồm các trường: summary (tóm tắt ngắn), healthAdvice (lời khuyên), riskLevel (Thấp/Trung bình/Cao).");

                // 3. Gọi AI Service
                var aiResponse = await _aiService.GetStaffSuggestionAsync(promptBuilder.ToString());

                return Ok(new { recordId, aiAnalysis = aiResponse });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xử lý AI: {ex.Message}");
            }
        }

        /// <summary>
        /// POST api/AI/chat
        /// Chatbot hỗ trợ giải đáp thắc mắc y tế cơ bản hoặc quy trình.
        /// </summary>
        [HttpPost("chat")]
        public async Task<IActionResult> AIChat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message)) return BadRequest("Tin nhắn không được để trống.");

            try
            {
                var prompt = $"Bạn là trợ lý AI trong hệ thống Quản lý đoàn khám Đa khoa An Phúc. Hãy trả lời câu hỏi sau của người dùng một cách lịch sự và chính xác: {request.Message}";
                var response = await _aiService.GetStaffSuggestionAsync(prompt);
                return Ok(new { reply = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi chatbot: {ex.Message}");
            }
        }

        public class ChatRequest
        {
            public string Message { get; set; } = null!;
        }
    }
}
