using System.Collections.Generic;

namespace MahERP.DataModelLayer.ViewModels
{
    public class AddRecipientsViewModel
    {
        public int TemplateId { get; set; }
        
        /// <summary>
        /// لیست ترکیبی Contact + Phone
        /// فرمت: "c{contactId}_p{phoneId}"
        /// </summary>
        public List<string> SelectedContacts { get; set; } = new();
        
        /// <summary>
        /// لیست سازمان‌ها
        /// </summary>
        public List<int> OrganizationIds { get; set; } = new();
    }
}
