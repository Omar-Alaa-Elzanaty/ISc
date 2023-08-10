using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.ModelsDtos
{
    public class SessionFeedback
    {
        public int SessionId { get; set; }
        public int TraineeId { get; set; }
        [MaxLength(500)]
        public string Feedback { get; set; }
        public DateTime DateTime { get; set; }= DateTime.Now;
        public virtual Session Session { get; set; }
        public virtual Trainee Trainee { get; set; }
    }
}
