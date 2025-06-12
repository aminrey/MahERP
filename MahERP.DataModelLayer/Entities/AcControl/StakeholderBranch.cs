using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.AcControl
{
    public class StakeholderBranch
    {
        [Key]
        public int Id { get; set; }

        public int StakeholderId { get; set; }
        [ForeignKey("StakeholderId")]
        public virtual Stakeholder Stakeholder { get; set; }

        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public bool IsActive { get; set; } = true;
        
        public string CreatorUserId { get; set; }
        [ForeignKey("CreatorUserId")]
        public virtual AppUsers Creator { get; set; }
        
        public DateTime CreateDate { get; set; }
    }
}
