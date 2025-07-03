using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ObiletJourneySearch.Models.ViewModels
{
    [ModelBinder(BinderType = typeof(SearchViewModelBinder))]
    public class SearchViewModel
    {
        [Required(ErrorMessage = "Origin location is required")]
        [Display(Name = "From")]
        public int? OriginId { get; set; }
        
        [BindNever]
        public string OriginName { get; set; }
        
        [Required(ErrorMessage = "Destination location is required")]
        [Display(Name = "To")]
        public int? DestinationId { get; set; }
        
        [BindNever]
        public string DestinationName { get; set; }
        
        [Required(ErrorMessage = "Departure date is required")]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; } = DateTime.Now.AddDays(1);
        
        [BindNever]
        public List<SelectListItem> LocationOptions { get; set; } = new List<SelectListItem>();
        
        [BindNever]
        public string SearchOrigin { get; set; }
        
        [BindNever]
        public string SearchDestination { get; set; }
    }
}
