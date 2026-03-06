using System.ComponentModel.DataAnnotations;

namespace TripTracker.Models;

public class PackingTemplate
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<PackingTemplateItem> Items { get; set; } = new List<PackingTemplateItem>();
}

