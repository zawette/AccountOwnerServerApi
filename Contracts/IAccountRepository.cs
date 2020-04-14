using Entities.Models;
using Entities.Models.Parameters;
using helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public interface IAccountRepository : IRepositoryBase<Account>
    {
        public void CreateAccount(Account account);
        public List<Account> storedProcedureTest();

        public PagedList<Account> AccountsByOwner(Guid ownerId, AccountParameters accountParameters);
        public Account getAccountById(Guid Id);
        PagedList<Account> getAccounts(AccountParameters accountParameters);
    }
}
