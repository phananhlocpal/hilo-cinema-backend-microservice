using System;
using System.Collections.Generic;

namespace AuthenticationService.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Token { get; set; }

    public byte[] TokenGenerationTime { get; set; } = null!;

    public string? EmailValidationStatus { get; set; }
}
