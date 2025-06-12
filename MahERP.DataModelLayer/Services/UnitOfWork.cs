using MahERP.DataModelLayer.AcControl;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Repository;



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
