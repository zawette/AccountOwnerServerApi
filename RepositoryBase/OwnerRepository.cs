using Contracts;
using Entities;
using Entities.Helpers;
using Entities.Models;
using Entities.Models.Parameters;
using helpers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Repository
{
    public class OwnerRepository : RepositoryBase<Owner>, IOwnerRepository
    {
        private SortHelper<Owner> _sortHelper = new SortHelper<Owner>();
        private DataShaper<Owner> _dataShaper = new DataShaper<Owner>();
        public OwnerRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {

        }

        public void CreateOwner(Owner owner)
        {
            Create(owner);
        }

        public void DeleteOwner(Owner owner)
        {
            Delete(owner);
        }

        public PagedList<ShapedEntity> GetOwners(OwnerParameters ownerParameters)
        {
            var owners = FindByCondition(o => o.DateOfBirth.Year >= ownerParameters.MinYearOfBirth &&
                             o.DateOfBirth.Year <= ownerParameters.MaxYearOfBirth);

            SearchByName(ref owners, ownerParameters.Name);
            _sortHelper.ApplySort(ref owners,ownerParameters.OrderBy);
            var ShapedOwnersData = _dataShaper.ShapeData(owners,ownerParameters.fields);
            return PagedList<ShapedEntity>.ToPagedList(ShapedOwnersData,
                ownerParameters.PageNumber,
                ownerParameters.PageSize);
        }

        private void SearchByName(ref IQueryable<Owner> owners, string ownerName)
        {
            if (!owners.Any() || string.IsNullOrWhiteSpace(ownerName))
                return;

            owners = owners.Where(o => o.Name.ToLower().Contains(ownerName.Trim().ToLower()));
        }

        public ShapedEntity GetOwnerById(Guid ownerId,OwnerParameters ownerParameters)
        {
            return _dataShaper.ShapeData(FindByCondition(o => o.Id.Equals(ownerId)).FirstOrDefault(),
                ownerParameters.fields);
        }

        public Owner GetOwnerById(Guid ownerId)
        {
            return FindByCondition(o => o.Id.Equals(ownerId)).FirstOrDefault();
        }


        public Owner GetOwnerWithDetails(Guid ownerId)
        {
            throw new NotImplementedException();
        }

        public void UpdateOwner(Owner owner)
        {
            Update(owner);
        }
    }
}