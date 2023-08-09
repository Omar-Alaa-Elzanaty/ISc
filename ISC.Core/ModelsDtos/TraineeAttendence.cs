using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.ModelsDtos
{
    public class TraineeAttendence
    {
        public int TraineeId { get; set; }
        public int SessionId { get; set; }
        public virtual Trainee Trainee { get; set; }
        public virtual Session Session { get; set; }
    }
}
