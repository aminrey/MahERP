using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MahERP.DataModelLayer.Repository
{
    public class ContractRepository : IContractRepository
    {
        private readonly AppDbContext _context;

        public ContractRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Contract> GetContracts(bool includeInactive = false)
        {
            var query = _context.Contract_Tbl
                .Include(c => c.Stakeholder)
                .AsQueryable(); // Explicitly convert to IQueryable to resolve CS0266

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            return query.OrderByDescending(c => c.CreateDate).ToList();
        }

        public Contract GetContractById(int id, bool includeTasks = false)
        {
            var query = _context.Contract_Tbl
                .Include(c => c.Stakeholder)
                .Include(c => c.Creator)
                .Include(c => c.LastUpdater)
                .AsQueryable(); // Explicitly convert to IQueryable to resolve CS0266

            if (includeTasks)
                query = query.Include(c => c.TaskList);

            return query.FirstOrDefault(c => c.Id == id);
        }

        public List<Contract> GetStakeholderContracts(int stakeholderId, bool includeInactive = false)
        {
            var query = _context.Contract_Tbl
                .Include(c => c.Stakeholder)
                .AsQueryable(); // Explicitly convert to IQueryable to resolve CS0266

            query = query.Where(c => c.StakeholderId == stakeholderId);

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            return query.OrderByDescending(c => c.CreateDate).ToList();
        }

        public bool IsContractNumberUnique(string contractNumber, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(contractNumber))
                return true;

            var query = _context.Contract_Tbl.Where(c => c.ContractNumber == contractNumber);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return !query.Any();
        }

        public List<Contract> SearchContracts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetContracts();

            var query = _context.Contract_Tbl
                .Include(c => c.Stakeholder)
                .AsQueryable(); // Explicitly convert to IQueryable to resolve CS0266

            query = query.Where(c => c.IsActive &&
                           (c.Title.Contains(searchTerm) ||
                            c.ContractNumber.Contains(searchTerm) ||
                            c.Stakeholder.FirstName.Contains(searchTerm) ||
                            c.Stakeholder.LastName.Contains(searchTerm) ||
                            c.Stakeholder.CompanyName.Contains(searchTerm)));

            return query.OrderByDescending(c => c.CreateDate).ToList();
        }

        public List<Contract> GetActiveContracts()
        {
            return _context.Contract_Tbl
                .Include(c => c.Stakeholder)
                .Where(c => c.IsActive && c.Status == 1 && (c.EndDate == null || c.EndDate > DateTime.Now))
                .OrderBy(c => c.EndDate)
                .ToList();
        }

        public List<Contract> GetExpiredContracts()
        {
            return _context.Contract_Tbl
                .Include(c => c.Stakeholder)
                .Where(c => c.IsActive && c.EndDate != null && c.EndDate < DateTime.Now)
                .OrderByDescending(c => c.EndDate)
                .ToList();
        }

        public List<Contract> GetContractsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Contract_Tbl
                .Include(c => c.Stakeholder)
                .Where(c => c.IsActive &&
                    ((c.StartDate >= startDate && c.StartDate <= endDate) ||
                     (c.EndDate >= startDate && c.EndDate <= endDate) ||
                     (c.StartDate <= startDate && (c.EndDate == null || c.EndDate >= endDate))))
                .OrderBy(c => c.StartDate)
                .ToList();
        }

        public List<Contract> GetContractsByStatus(byte status)
        {
            return _context.Contract_Tbl
                .Include(c => c.Stakeholder)
                .Where(c => c.IsActive && c.Status == status)
                .OrderByDescending(c => c.CreateDate)
                .ToList();
        }
    }
}