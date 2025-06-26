using System;
using System.ComponentModel.DataAnnotations;

namespace MahERP.DataModelLayer.ViewModels.CRMViewModels
{
    public class CRMAttachmentViewModel
    {
        public int Id { get; set; }
        public int CRMInteractionId { get; set; }

        [Required(ErrorMessage = "نام فایل الزامی است")]
        [Display(Name = "نام فایل")]
        public string FileName { get; set; }

        [Display(Name = "نوع فایل")]
        public string FileType { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploaderUserId { get; set; }
        public string? UploaderName { get; set; }

        public string FormattedFileSize => FormatBytes(FileSize);

        private string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }
    }
}