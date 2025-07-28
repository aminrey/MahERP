using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using MahERP.DataModelLayer.ViewModels.UserViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.DataModelLayer.Repository
{
    public class BranchRepository : IBranchRepository
    {
        private readonly AppDbContext _context;

        public BranchRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Branch> GetBranches(bool includeInactive = false)
        {
            var query = _context.Branch_Tbl.AsQueryable();

            if (!includeInactive)
                query = query.Where(b => b.IsActive);

            return query.OrderBy(b => b.Name).ToList();
        }

        public Branch GetBranchById(int id, bool includeUsers = false, bool includeStakeholders = false, bool includeTasks = false, bool includeChildBranches = false)
        {
            var query = _context.Branch_Tbl.AsQueryable();

            if (includeUsers)
                query = query.Include(b => b.BranchUsers)
                             .ThenInclude(bu => bu.User);

            if (includeStakeholders)
                query = query.Include(b => b.Stakeholders);

            if (includeTasks)
                query = query.Include(b => b.TaskList);

            if (includeChildBranches)
                query = query.Include(b => b.ChildBranches);

            return query.FirstOrDefault(b => b.Id == id);
        }

        public List<Branch> GetMainBranches(bool includeInactive = false)
        {
            var query = _context.Branch_Tbl.Where(b => b.IsMainBranch);

            if (!includeInactive)
                query = query.Where(b => b.IsActive);

            return query.OrderBy(b => b.Name).ToList();
        }

        public List<Branch> GetChildBranches(int parentId, bool includeInactive = false)
        {
            var query = _context.Branch_Tbl.Where(b => b.ParentId == parentId);

            if (!includeInactive)
                query = query.Where(b => b.IsActive);

            return query.OrderBy(b => b.Name).ToList();
        }

        public List<BranchUser> GetBranchUsers(int branchId, bool includeInactive = false)
        {
            var query = _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Where(bu => bu.BranchId == branchId);

            if (!includeInactive)
                query = query.Where(bu => bu.IsActive);

            return query.OrderBy(bu => bu.User.LastName).ThenBy(bu => bu.User.FirstName).ToList();
        }

        public BranchUser GetBranchUserById(int id)
        {
            return _context.BranchUser_Tbl
                .Include(bu => bu.User)
                .Include(bu => bu.Branch)
                .FirstOrDefault(bu => bu.Id == id);
        }

        public bool IsBranchNameUnique(string name, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true;

            var query = _context.Branch_Tbl.Where(b => b.Name == name);

            if (excludeId.HasValue)
                query = query.Where(b => b.Id != excludeId.Value);

            return !query.Any();
        }

        public List<Branch> SearchBranches(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetBranches();

            var query = _context.Branch_Tbl
                .Where(b => b.IsActive &&
                           (b.Name.Contains(searchTerm) ||
                            b.ManagerName.Contains(searchTerm) ||
                            b.Phone.Contains(searchTerm) ||
                            b.Email.Contains(searchTerm)));

            return query.OrderBy(b => b.Name).ToList();
        }

        public List<Stakeholder> GetBranchStakeholders(int branchId, bool includeInactive = false)
        {
            var stakeholderIds = _context.StakeholderBranch_Tbl
                .Where(sb => sb.BranchId == branchId && sb.IsActive)
                .Select(sb => sb.StakeholderId);

            var query = _context.Stakeholder_Tbl
                .Where(s => stakeholderIds.Contains(s.Id) && !s.IsDeleted);

            if (!includeInactive)
                query = query.Where(s => s.IsActive);

            return query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();
        }


        public List<BranchViewModel> GetBrnachListByUserId(string UserLoginingid)
            
        {

            return null;
        }

    }
}