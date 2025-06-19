using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.Crm;
using MahERP.DataModelLayer.Repository;
using System;

namespace MahERP.DataModelLayer.Services
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private readonly AppDbContext _Context;

        public UnitOfWork(AppDbContext Db)
        {
            _Context = Db;
        }

        //User
        private GenereicClass<AppUsers> _userManager;
        private GenereicClass<AppRoles> _RoleManager;
        private GenereicClass<RolePattern> _rolePattern;
        private GenereicClass<RolePatternDetails> _rolePatterDetails;

        //Stakeholder
        private GenereicClass<Stakeholder> _stakeholder;
        private GenereicClass<StakeholderCRM> _stakeholderCRM;
        private GenereicClass<StakeholderContact> _stakeholderContact;

        //Branch and Contract
        private GenereicClass<Branch> _branch;
        private GenereicClass<BranchUser> _branchUser;
        private GenereicClass<Contract> _contract;
        private GenereicClass<StakeholderBranch> _stakeholderBranch;

        public GenereicClass<RolePatternDetails> rolePatternDetailsUW
        {
            get
            {
                if (_rolePatterDetails == null)
                {
                    _rolePatterDetails = new GenereicClass<RolePatternDetails>(_Context);
                }
                return _rolePatterDetails;
            }
        }
        public GenereicClass<RolePattern> rolePatternUW
        {
            get
            {
                if (_rolePattern == null)
                {
                    _rolePattern = new GenereicClass<RolePattern>(_Context);
                }
                return _rolePattern;
            }
        }
        public GenereicClass<AppUsers> UserManagerUW
        {
            get
            {
                if (_userManager == null)
                {
                    _userManager = new GenereicClass<AppUsers>(_Context);
                }
                return _userManager;
            }
        }
        public GenereicClass<AppRoles> RoleManagerUW
        {
            get
            {
                if (_RoleManager == null)
                {
                    _RoleManager = new GenereicClass<AppRoles>(_Context);
                }
                return _RoleManager;
            }
        }

        public GenereicClass<Stakeholder> StakeholderUW
        {
            get
            {
                if (_stakeholder == null)
                {
                    _stakeholder = new GenereicClass<Stakeholder>(_Context);
                }
                return _stakeholder;
            }
        }

        public GenereicClass<StakeholderCRM> StakeholderCRMUW
        {
            get
            {
                if (_stakeholderCRM == null)
                {
                    _stakeholderCRM = new GenereicClass<StakeholderCRM>(_Context);
                }
                return _stakeholderCRM;
            }
        }

        public GenereicClass<StakeholderContact> StakeholderContactUW
        {
            get
            {
                if (_stakeholderContact == null)
                {
                    _stakeholderContact = new GenereicClass<StakeholderContact>(_Context);
                }
                return _stakeholderContact;
            }
        }

        public GenereicClass<Branch> BranchUW
        {
            get
            {
                if (_branch == null)
                {
                    _branch = new GenereicClass<Branch>(_Context);
                }
                return _branch;
            }
        }

        public GenereicClass<BranchUser> BranchUserUW
        {
            get
            {
                if (_branchUser == null)
                {
                    _branchUser = new GenereicClass<BranchUser>(_Context);
                }
                return _branchUser;
            }
        }

        public GenereicClass<Contract> ContractUW
        {
            get
            {
                if (_contract == null)
                {
                    _contract = new GenereicClass<Contract>(_Context);
                }
                return _contract;
            }
        }

        public GenereicClass<StakeholderBranch> StakeholderBranchUW
        {
            get
            {
                if (_stakeholderBranch == null)
                {
                    _stakeholderBranch = new GenereicClass<StakeholderBranch>(_Context);
                }
                return _stakeholderBranch;
            }
        }

        public IEntityDataBaseTransaction BeginTransaction()
        {
            return new EntityDataBaseTransaction(_Context);
        }

        public void Save()
        {
            _Context.SaveChanges();
        }

        public void Dispose()
        {
            _Context.Dispose();
        }
    }
}