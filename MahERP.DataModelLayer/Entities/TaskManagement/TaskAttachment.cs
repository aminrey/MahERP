using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    public class TaskAttachment
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Tasks Task { get; set; }

        [Required]
        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string FileExtension { get; set; }

        public string FileSize { get; set; }

        public string FileUUID { get; set; }

        public DateTime UploadDate { get; set; }

        public string UploaderUserId { get; set; }
        [ForeignKey("UploaderUserId")]
        public virtual AppUsers Uploader { get; set; }
        
        /// <summary>
        /// توضیحات فایل
        /// </summary>
        public string Description { get; set; }
    }
}
