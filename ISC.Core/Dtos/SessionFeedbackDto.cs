using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.Dtos
{
    public class SessionFeedbackDto
    {
        public int SessionId { get; set; }
        public string Feedback { get; set; }
        public int Rate { get; set; }
    }
}
