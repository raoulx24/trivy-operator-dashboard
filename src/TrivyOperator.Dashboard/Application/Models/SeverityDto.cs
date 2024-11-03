﻿using System.ComponentModel.DataAnnotations;

namespace TrivyOperator.Dashboard.Application.Models;

public class SeverityDto
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string Name { get; init; } = string.Empty;
}
