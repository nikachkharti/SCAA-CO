namespace SCAA_API.Models.Supplier;

public record SupplierForGettingDto
{
    public int Id { get; set; }
    public string SupplierName { get; set; }
}

public record SupplierForCreatingDto
{
    public string SupplierName { get; set; }
}

public record SupplierForUpdatingDto
{
    public int Id { get; set; }
    public string SupplierName { get; set; }
}
