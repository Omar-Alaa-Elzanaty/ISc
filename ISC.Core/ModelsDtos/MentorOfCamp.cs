using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.ModelsDtos
{
    public class MentorOfCamp
    {
        public int MentorId { get; set; }
        public int CampId { get; set; }
        public Mentor Mentor { get; set; }
        public Camp Camp { get; set; }
    }
}
