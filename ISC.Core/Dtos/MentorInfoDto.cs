using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
    public class MentorInfoDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? FacebookLink { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string CodeforceHandle { get; set; }
        public string? VjudgeHandle { get; set; }
        public string Gander { get; set; }
        public string Collage { get; set; }
        public List<string> Camps { get; set; }
    }
}
