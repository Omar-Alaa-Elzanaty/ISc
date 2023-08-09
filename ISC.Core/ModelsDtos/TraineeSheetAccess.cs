using ISC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.Core.ModelsDtos
{
    public class TraineeSheetAccess
    {
        public int TraineeId { get; set; }
        public int SheetId { get; set; }
        public int NumberOfProblems { get; set; } = 0;
        public virtual Trainee Trainee { get; set; }
        public virtual Sheet Sheet { get; set; }
    }
}
