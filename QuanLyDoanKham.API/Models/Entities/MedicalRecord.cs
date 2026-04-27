using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class MedicalRecord
    {
        [Key]
        public int MedicalRecordId { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup? MedicalGroup { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(20)]
        public string? IDCardNumber { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        /// <summary>JWT-signed token for check-in</summary>
        [Required]
        [MaxLength(512)]
        public string QrToken { get; set; } = null!;

        /// <summary>
        /// CREATED, READY, CHECKED_IN, IN_PROGRESS, STATION_DONE, QC_PENDING, QC_PASSED, REPORTED, CLOSED
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "CREATED";


        /// <summary>Link to master patient record after dedup</summary>
        public int? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        public DateTime? CheckInAt { get; set; }
        public int? CheckInByUserId { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

    }
}
