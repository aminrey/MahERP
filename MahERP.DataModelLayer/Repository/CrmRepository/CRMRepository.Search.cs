using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository
{
    /// <summary>
    /// CRM Repository - Search Helpers
    /// </summary>
    public partial class CRMRepository
    {
        /// <summary>
        /// دریافت لیست‌های Dropdown برای جستجوی پیشرفته
        /// </summary>
        public async Task<(SelectList stakeholders, SelectList branches)> GetSearchDropdownsAsync()
        {
            // دریافت Stakeholders
            var stakeholders = await _context.Stakeholder_Tbl
                .Where(s => s.IsActive && !s.IsDeleted)
                .Select(s => new
                {
                    Id = s.Id,
                    FullName = s.FirstName + " " + s.LastName
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();

            var stakeholdersList = new SelectList(stakeholders, "Id", "FullName");

            // دریافت Branches
            var branches = await _context.Branch_Tbl
                .Where(b => b.IsActive)
                .Select(b => new
                {
                    Id = b.Id,
                    Name = b.Name
                })
                .OrderBy(b => b.Name)
                .ToListAsync();

            var branchesList = new SelectList(branches, "Id", "Name");

            return (stakeholdersList, branchesList);
        }
    }
}
