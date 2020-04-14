using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Entities.Models;
using Entities.Models.Parameters;
using Newtonsoft.Json;
using Entities.DataTransferObjects;

namespace AccountOwnerServer.Controllers
{
    [Route("api/Account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private ILoggerManager _logger;
        private IRepositoryWrapper _repository;
        private IMapper _mapper;


        public AccountController(ILoggerManager logger, IRepositoryWrapper repository, IMapper mapper)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
        }

        

        [HttpGet]
        public ActionResult<IEnumerable<Account>> getAccounts([FromQuery] AccountParameters parameters)
        {
            try
            {
                var accounts = _repository.Account.getAccounts(parameters);

                var metadata = new
                {
                    accounts.TotalCount,
                    accounts.PageSize,
                    accounts.CurrentPage,
                    accounts.TotalPages,
                    accounts.HasNext,
                    accounts.HasPrevious
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

                _logger.LogInfo($"Returned {accounts.TotalCount} owners from database.");

                return Ok(accounts);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside getAccounts action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }


        }


        [HttpGet("{Id}")]
        public ActionResult<Account> getAccountById(Guid Id)
        {
            try
            {
                var Account = _repository.Account.getAccountById(Id);

                if (Account == null)
                {
                    _logger.LogError($"Account with id: {Id}, hasn't been found in db.");
                    return NotFound();
                }

                else
                {
                    var AccountToSend = _mapper.Map<AccountDto>(Account);
                    return Ok(AccountToSend);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"Something went wrong inside GetAccountById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }

        [HttpGet("StoredProcedureTest")]
        public List<Account> StoredProcedureTest()
        {
            return _repository.Account.storedProcedureTest();
        }

        [HttpPost]
        public IActionResult CreateAccount([FromBody] AccountForCreationtDto account)
        {
            var accountToSave = _mapper.Map<Account>(account);
            _repository.Account.CreateAccount(accountToSave);
            _repository.Save();
            var accountToSend = _mapper.Map<AccountDto>(accountToSave);
            return CreatedAtAction(nameof(getAccountById), new { Id = accountToSend.Id }, accountToSend);
        }

    }
}