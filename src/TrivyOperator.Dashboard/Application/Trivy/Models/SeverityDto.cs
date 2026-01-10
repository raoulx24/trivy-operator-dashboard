using System.ComponentModel.DataAnnotations;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

public class SeverityDto
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string Name { get; init; } = string.Empty;
}
