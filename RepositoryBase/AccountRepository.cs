using Contracts;
using Entities;
using Entities.Models;
using Entities.Models.Parameters;
using helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class AccountRepository : RepositoryBase<Account>, IAccountRepository
    {
        public AccountRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }

        public void CreateAccount(Account account)
        {
            Create(account);
        }


        public PagedList<Account> AccountsByOwner(Guid ownerId, AccountParameters accountParameters)
        {
            return PagedList<Account>.ToPagedList(FindByCondition(ac=>ac.OwnerId.Equals(ownerId)).ToList().OrderBy(ac => ac.Owner),
               accountParameters.PageNumber,
               accountParameters.PageSize);
        }

        public Account getAccountById(Guid Id)
        {
            return FindByCondition(ac => ac.Id.Equals(Id)).FirstOrDefault();
        }

        public PagedList<Account> getAccounts(AccountParameters accountParameters)
        {
            return PagedList<Account>.ToPagedList(FindAll().OrderBy(ac => ac.Owner),
               accountParameters.PageNumber,
               accountParameters.PageSize);

        }

        public List<Account> storedProcedureTest()
        {
            return StoredProcedure();
        }
    }
}