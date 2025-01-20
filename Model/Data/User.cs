using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Model.Data;

public class User : IdentityUser
{   
    [Required, MaxLength(255)]
    public string Description { get; set; } = null !;
}
