using ISC.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISC.EF
{
    public interface IAccountRepository : IBaseRepository<UserAccount>
    {
        Task<bool> tryCreateTraineeAccountAsync(UserAccount account, string password);
    }
}
