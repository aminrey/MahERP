namespace MahERP.DataModelLayer.ViewModels.taskingModualsViewModels
{
    /// <summary>
    /// اطلاعات کامل یک ناظر
    /// </summary>
    public class SupervisorInfoViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImagePath { get; set; }
        
        /// <summary>
        /// نوع نظارت: مدیر تیم، ناظر تیم، سمت بالاتر، مجوز خاص
        /// </summary>
        public string SupervisionType { get; set; }
    }
}