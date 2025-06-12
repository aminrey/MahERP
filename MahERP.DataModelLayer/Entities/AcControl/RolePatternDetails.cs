using MahERP.DataModelLayer;
using MahERP.DataModelLayer.Entities.AcControl;


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahERP.DataModelLayer.AcControl
{
    public class RolePatternDetails
    {
        [Key]
        public int RolePatternDetailsID { get; set; }
        public int RolePatternID { get; set; }
        public string? RoleID { get; set; }

        [ForeignKey("RolePatternID")]
        public virtual RolePattern? RP { get; set; }

        [ForeignKey("RoleID")]

        public virtual AppRoles? Roles { get; set; }
    }
}
