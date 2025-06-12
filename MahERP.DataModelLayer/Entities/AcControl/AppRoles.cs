using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MahERP.DataModelLayer.AcControl
{
  public  class AppRoles:IdentityRole
    {

        public string? RoleLevel { get; set; }
        public string? Description { get; set; }

    }
}
