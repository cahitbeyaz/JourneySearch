using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ObiletJourneySearch.Models.ViewModels;

namespace ObiletJourneySearch.Models.ViewModels
{
    public class SearchViewModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // Create the model
            var model = new SearchViewModel();

            // Only bind the properties we care about for form submission
            var originIdResult = bindingContext.ValueProvider.GetValue("OriginId");
            if (originIdResult != ValueProviderResult.None && int.TryParse(originIdResult.FirstValue, out int originId))
            {
                model.OriginId = originId;
            }

            var destinationIdResult = bindingContext.ValueProvider.GetValue("DestinationId");
            if (destinationIdResult != ValueProviderResult.None && int.TryParse(destinationIdResult.FirstValue, out int destinationId))
            {
                model.DestinationId = destinationId;
            }

            var departureDateResult = bindingContext.ValueProvider.GetValue("DepartureDate");
            if (departureDateResult != ValueProviderResult.None && DateTime.TryParse(departureDateResult.FirstValue, out DateTime departureDate))
            {
                model.DepartureDate = departureDate;
            }

            // Set the result
            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}
