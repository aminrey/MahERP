using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Repository;

namespace MahERP.DataModelLayer.Services
{

    public interface IUnitOfWork
    {
        //Users
        GenereicClass<AppUsers> UserManagerUW { get; }
        GenereicClass<AppRoles> RoleManagerUW { get; }
        GenereicClass<RolePattern> rolePatternUW { get; }
        GenereicClass<RolePatternDetails> rolePatternDetailsUW { get; }
        
        //Stakeholders
        GenereicClass<Stakeholder> StakeholderUW { get; }
        GenereicClass<StakeholderCRM> StakeholderCRMUW { get; }
        GenereicClass<StakeholderContact> StakeholderContactUW { get; }
       
        IEntityDataBaseTransaction BeginTransaction();
        void Save();
        void Dispose();
    }


}
