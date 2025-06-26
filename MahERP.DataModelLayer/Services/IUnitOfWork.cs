using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Entities.TaskManagement;
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
        
        //Branches
        GenereicClass<Branch> BranchUW { get; }
        GenereicClass<BranchUser> BranchUserUW { get; }
        GenereicClass<Contract> ContractUW { get; }
        GenereicClass<StakeholderBranch> StakeholderBranchUW { get; }

        // Task Management repositories
        GenereicClass<Tasks> TaskUW { get; }
        GenereicClass<TaskOperation> TaskOperationUW { get; }
        GenereicClass<TaskCategory> TaskCategoryUW { get; }
        GenereicClass<TaskAssignment> TaskAssignmentUW { get; }
        GenereicClass<TaskAttachment> TaskAttachmentUW { get; }
        GenereicClass<TaskComment> TaskCommentUW { get; }

        IEntityDataBaseTransaction BeginTransaction();
        void Save();
        void Dispose();
    }


}
