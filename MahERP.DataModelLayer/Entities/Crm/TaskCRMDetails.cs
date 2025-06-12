using MahERP.DataModelLayer.Entities.AcControl;
using MahERP.DataModelLayer.Entities.TaskManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.Crm
{
    public class TaskCRMDetails
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }


        public int? StakeholderContactId { get; set; }
        [ForeignKey("StakeholderContactId")]
        public virtual StakeholderContact StakeholderContact { get; set; }


        public byte Direction { get; set; }

        public byte Result { get; set; }


        public int? Duration { get; set; }


        public string PhoneNumber { get; set; }

        public string EmailAddress { get; set; }

        public DateTime? NextFollowUpDate { get; set; }
        public string NextFollowUpNote { get; set; }
    }
}