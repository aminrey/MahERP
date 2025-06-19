using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.Collections.Generic;

namespace MahERP.DataModelLayer.Services
{
    public interface IContractRepository
    {
        List<Contract> GetContracts(bool includeInactive = false);
        Contract GetContractById(int id, bool includeTasks = false);
        List<Contract> GetStakeholderContracts(int stakeholderId, bool includeInactive = false);
        bool IsContractNumberUnique(string contractNumber, int? excludeId = null);
        List<Contract> SearchContracts(string searchTerm);
        List<Contract> GetActiveContracts();
        List<Contract> GetExpiredContracts();
        List<Contract> GetContractsByDateRange(DateTime startDate, DateTime endDate);
        List<Contract> GetContractsByStatus(byte status);
    }
}