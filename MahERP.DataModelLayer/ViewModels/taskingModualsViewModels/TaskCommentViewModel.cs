public class TaskCommentViewModel
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string? CommentText { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsImportant { get; set; }
    public byte CommentType { get; set; }
    public DateTime CreateDate { get; set; }
    public string? CreatorUserId { get; set; }
    public string? CreatorName { get; set; }
    
    // ⭐⭐⭐ فیلد عکس پروفایل
    public string? CreatorProfileImage { get; set; }
    
    public int? ParentCommentId { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditDate { get; set; }
    
    // برای نمایش replies
    public List<TaskCommentViewModel> Replies { get; set; } = new();
    
    // فایل‌های پیوست
    public List<TaskCommentAttachmentViewModel> Attachments { get; set; } = new();
}

public class TaskCommentAttachmentViewModel
{
    public int Id { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileExtension { get; set; }
    public long FileSize { get; set; }
}