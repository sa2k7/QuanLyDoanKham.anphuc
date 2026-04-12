using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.DTOs
{
    public class MedicalRecordIngestDto
    {
        [Required]
        public required string FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? IDCardNumber { get; set; }

        public string? Department { get; set; }
    }

    public class MedicalRecordBatchIngestRequestDto
    {
        [Required]
        public int GroupId { get; set; }

        [Required]
        public required List<MedicalRecordIngestDto> Records { get; set; }
    }
}
