using System.ComponentModel.DataAnnotations;

namespace CaterinGO.Data;

public sealed class WaitingListEntry
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    public DateTime DateOfEntry { get; set; }
}
