using MahERP.DataModelLayer.Entities.AcControl;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahERP.DataModelLayer.Entities.TaskManagement
{
    public class TaskCommentAttachment
    {
        [Key]
        public int Id { get; set; }

        public int CommentId { get; set; }
        [ForeignKey("CommentId")]
        public virtual TaskComment Comment { get; set; }

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
    }
}
