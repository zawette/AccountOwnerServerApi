using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.Models.Parameters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace AccountOwnerServer.Controllers
{
    [Route("api/owner")]
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class OwnerController : ControllerBase
    {
        private ILoggerManager _logger;
        private IRepositoryWrapper _repository;
        private IMapper _mapper;
        private LinkGenerator _linkGenerator;

        public OwnerController(ILoggerManager logger, IRepositoryWrapper repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }


        private IEnumerable<Link> CreateLinksForOwner(Guid id, string fields = "")
        {
            var links = new List<Link>
                {
                    new Link(_linkGenerator.GetUriByAction(HttpContext, nameof(GetOwnerById), values: new { id, fields }),
                    "self",
                    "GET"),

                    new Link(_linkGenerator.GetUriByAction(HttpContext, nameof(DeleteOwner), values: new { id }),
                    "delete_owner",
                    "DELETE"),

                    new Link(_linkGenerator.GetUriByAction(HttpContext, nameof(UpdateOwner), values: new { id }),
                    "update_owner",
                    "PUT")
                };

            return links;
        }

        private LinkCollectionWrapper<Entity> CreateLinksForOwners(LinkCollectionWrapper<Entity> ownersWrapper)
        {
            ownersWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(HttpContext, nameof(GetAllOwners), values: new { }),
                    "self",
                    "GET"));

            return ownersWrapper;
        }



        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllOwners([FromQuery] OwnerParameters ownerParameters)
        {
            try
            {
                if (!ownerParameters.ValidYearRange)
                {
                    return BadRequest("Max year of birth cannot be less than min year of birth");
                }

                var owners = _repository.Owner.GetOwners(ownerParameters);

                var metadata = new
                {
                    owners.TotalCount,
                    owners.PageSize,
                    owners.CurrentPage,
                    owners.TotalPages,
                    owners.HasNext,
                    owners.HasPrevious
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

                var shapedOwners = owners.Select(o => o.Entity).ToList();

                for (var index = 0; index < owners.Count(); index++)
                {
                    var ownerLinks = CreateLinksForOwner(owners[index].Id, ownerParameters.fields);
                    shapedOwners[index].Add("Links", ownerLinks);
                }

                var ownersWrapper = new LinkCollectionWrapper<Entity>(shapedOwners);

                return Ok(CreateLinksForOwners(ownersWrapper));
            }

            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetAllOwners action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult GetOwnerById(Guid id, [FromQuery] OwnerParameters ownerParameters)
        {
            try
            {
                var owner = _repository.Owner.GetOwnerById(id, ownerParameters);

                if (owner.Id == Guid.Empty)
                {
                    _logger.LogError($"Owner with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    _logger.LogInfo($"Returned owner with id: {id}");
                    return Ok(owner.Entity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetOwnerById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        [Consumes("application/json")]
        public IActionResult CreateOwner([FromBody]OwnerForCreationDto owner)
        {
            try
            {
                if (owner == null)
                {
                    _logger.LogError("Owner object sent from client is null.");
                    return BadRequest("Owner object is null");
                }

                // i think this is redundant with the ApiController attribute
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid owner object sent from client.");
                    return BadRequest("Invalid model object");
                }

                var ownerEntity = _mapper.Map<Owner>(owner);

                _repository.Owner.CreateOwner(ownerEntity);
                _repository.Save();

                var createdOwner = _mapper.Map<OwnerDto>(ownerEntity);

                return CreatedAtAction(nameof(GetOwnerById), new { id = createdOwner.Id }, createdOwner);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside CreateOwner action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        public IActionResult UpdateOwner(Guid id, [FromBody]OwnerForUpdateDto owner)
        {
            try
            {
                if (owner == null)
                {
                    _logger.LogError("Owner object sent from client is null.");
                    return BadRequest("Owner object is null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid owner object sent from client.");
                    return BadRequest("Invalid model object");
                }

                var ownerEntity = _repository.Owner.GetOwnerById(id);
                if (ownerEntity == null)
                {
                    _logger.LogError($"Owner with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                _mapper.Map(owner, ownerEntity);

                _repository.Owner.UpdateOwner(ownerEntity);
                _repository.Save();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside UpdateOwner action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteOwner(Guid id)
        {
            try
            {
                var owner = _repository.Owner.GetOwnerById(id);
                if (owner == null)
                {
                    _logger.LogError($"Owner with id: {id}, hasn't been found in db.");
                    return NotFound();
                }

                if (_repository.Account.AccountsByOwner(id, null).Any())
                {
                    _logger.LogError($"Cannot delete owner with id: {id}. It has related accounts. Delete those accounts first");
                    return BadRequest("Cannot delete owner. It has related accounts. Delete those accounts first");
                }

                _repository.Owner.DeleteOwner(owner);
                _repository.Save();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside DeleteOwner action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{ownerId}/Accounts")]
        public IActionResult GetAccountsForOwner(Guid ownerId, [FromQuery] AccountParameters parameters)
        {
            var accounts = _repository.Account.AccountsByOwner(ownerId, parameters);

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
    }
}