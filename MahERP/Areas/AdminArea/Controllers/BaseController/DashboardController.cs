using AutoMapper;
using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;


namespace MahERP.Areas.AdminArea.Controllers.BaseController
{
    [Authorize]
    [Area("AdminArea")]
    public class DashboardController : BaseController
    {

        private readonly IUnitOfWork _Context;
        private readonly IMapper _Mapper;
        private readonly IWebHostEnvironment _env;

        private readonly UserManager<AppUsers> _UserManager;

        public DashboardController(IWebHostEnvironment env, IUnitOfWork Context, IMapper Mapper, UserManager<AppUsers> UserManager) : base(Context, UserManager)
        {
            _Context = Context;
            _UserManager = UserManager;
            _Mapper = Mapper;
            _env = env;

        }

        public IActionResult Index()
        {       
            return View();
        }


    }
}
