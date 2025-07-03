using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ObiletJourneySearch.Models.ViewModels
{
    public class SearchViewModel
    {
        [Required(ErrorMessage = "Please select an origin.")]
        [Display(Name = "From")]
        public int? OriginId { get; set; }

        [BindNever]
        public string? OriginName { get; set; }

        [Required(ErrorMessage = "Please select a destination.")]
        [Display(Name = "To")]
        public int? DestinationId { get; set; }

        [BindNever]
        public string? DestinationName { get; set; }

        [Required(ErrorMessage = "Please select a date.")]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Now.AddDays(1);

        [BindNever]
        public List<SelectListItem> LocationOptions { get; set; } = new List<SelectListItem>();

        [BindNever]
        public string? SearchOrigin { get; set; }

        [BindNever]
        public string? SearchDestination { get; set; }
    }
}
