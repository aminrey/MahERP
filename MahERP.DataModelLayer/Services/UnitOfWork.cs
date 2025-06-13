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

        public GenereicClass<RolePatternDetails> rolePatternDetailsUW
        {
            //فقط خواندنی    
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
            //فقط خواندنی    
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