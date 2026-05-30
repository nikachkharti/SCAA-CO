namespace SCAA_API.Models.Category;

public record CategoryForGettingDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
}

public record CategoryForCreatingDto
{
    public string CategoryName { get; set; }
}

public record CategoryForUpdatingDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
}
