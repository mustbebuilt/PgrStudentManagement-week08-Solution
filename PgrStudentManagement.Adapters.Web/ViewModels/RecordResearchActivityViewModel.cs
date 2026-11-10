using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PgrStudentManagement.Adapters.Web.ViewModels;

public class RecordResearchActivityViewModel
{
    [Required(ErrorMessage = "Student number is required.")]
    [Display(Name = "Student Number")]
    public string StudentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Activity type is required.")]
    [Display(Name = "Activity Type")]
    public string ActivityType { get; set; } = "ResearchReflection";

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Activity date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Activity Date")]
    public DateTime ActivityDate { get; set; } = DateTime.Today;

    [Display(Name = "Tags (comma-separated)")]
    public string? TagsInput { get; set; }

    [Display(Name = "Notes / Details")]
    public string? Notes { get; set; }

    public string? ErrorMessage { get; set; }

    public IEnumerable<SelectListItem> AvailableActivityTypes { get; set; } =
    [
        new SelectListItem("Research Reflection", "ResearchReflection"),
        new SelectListItem("Supervision Meeting", "SupervisionMeeting"),
        new SelectListItem("Conference Attendance", "ConferenceAttendance"),
        new SelectListItem("Publication", "Publication"),
        new SelectListItem("Experiment", "Experiment"),
        new SelectListItem("Training Event", "TrainingEvent")
    ];
}
