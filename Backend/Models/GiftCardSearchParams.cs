namespace Backend.Models
{
public class GiftCardSearchParams
{
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DisabledAt { get; set; }
    public decimal? Balance { get; set; }
    public decimal? InitialValue { get; set; }
    public decimal? AmountSpent { get; set; }
    public string? Email { get; set; }
    public string? LastCharacters { get; set; }
}

}
