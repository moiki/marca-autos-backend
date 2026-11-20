namespace autos.Api.Features.MarcaAutos;

public interface ISoftDeletable
{
    bool IsActive { get; set; }
}
public class MarcaAuto: ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OriginCountry { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}