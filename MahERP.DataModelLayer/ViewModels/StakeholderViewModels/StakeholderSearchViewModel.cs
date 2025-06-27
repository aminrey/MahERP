public class StakeholderSearchViewModel
{
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? NationalCode { get; set; }
    public byte? StakeholderType { get; set; }
    public bool? IsActive { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    
    // CRM Fields
    public byte? SalesStage { get; set; }
    public byte? LeadSource { get; set; }
    public string? Industry { get; set; }
    public string? CreditRating { get; set; }
    public decimal? MinPotentialValue { get; set; }
    public decimal? MaxPotentialValue { get; set; }
    public string? SalesRepUserId { get; set; }
}