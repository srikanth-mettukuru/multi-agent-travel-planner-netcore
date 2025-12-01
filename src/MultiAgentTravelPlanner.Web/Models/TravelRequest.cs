using System.ComponentModel.DataAnnotations;

namespace MultiAgentTravelPlanner.Web.Models;

public class TravelRequest
{
    [Required(ErrorMessage = "Origin city is required")]
    [Display(Name = "Origin City")]
    public string Origin { get; set; } = string.Empty;

    [Required(ErrorMessage = "Destination city is required")]
    [Display(Name = "Destination City")]
    public string Destination { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "End date is required")]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);
}