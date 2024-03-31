using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISC.Core.Models;

namespace ISC.Core.Dtos
{
    public class TraineeInfoDto
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public DateTime BirthDate { get; set; }
        public int Grade { get; set; }
        public string College { get; set; }
        public DateTime JoinDate { get; set; }
        public string Gender { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string? PhotoUrl { get; set; }
        public string CodeForceHandle { get; set; }
        public string? FacebookLink { get; set; }
        public string? VjudgeHandle { get; set; }
        public int points { get; set; }
        public int Rank { get; set; }
    }
}
