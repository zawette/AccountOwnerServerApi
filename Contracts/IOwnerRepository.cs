using Entities.Models;
using Entities.Models.Parameters;
using helpers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Contracts
{
    public interface IOwnerRepository : IRepositoryBase<Owner>
    {
        public PagedList<ShapedEntity> GetOwners(OwnerParameters ownerParameters);
        public ShapedEntity GetOwnerById(Guid ownerId,OwnerParameters ownerParameters);
        public Owner GetOwnerById(Guid ownerId);
        public Owner GetOwnerWithDetails(Guid ownerId);
        public void CreateOwner(Owner owner);
        public void UpdateOwner(Owner owner);
        public void DeleteOwner(Owner owner);
    }
}
