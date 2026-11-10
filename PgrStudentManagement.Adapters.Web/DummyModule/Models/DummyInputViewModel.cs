using System.ComponentModel.DataAnnotations;

namespace PgrStudentManagement.Adapters.Web.DummyModule.Models;

public sealed class DummyInputViewModel
{
    [Required]
    [StringLength(200)]
    public string Input { get; set; } = string.Empty;
}
