# Obilet Journey Search

ASP.NET Core MVC app for searching bus journeys using the Obilet.com API.

## Overview

Searchable interface for bus journeys between locations. Users select origin and destination locations, choose a departure date, and view available journeys with pricing and details.

## Features

### Core Functionality
- **Bus Location Search**: Text-based search for origin and destination locations
- **Journey Search**: Find available journeys between selected locations on a specific date
- **Journey Results**: View journey details (times, prices, seats, duration, bus features)
- **Caching System**: Memory cache for locations to improve performance

### User Experience
- **Modern UI Design**: Clean, responsive card-based layout matching the reference design
- **Persistent Search Parameters**: Last searched locations and dates are remembered using localStorage
- **Quick Date Selection**: "Today" and "Tomorrow" buttons for rapid date selection
- **Location Swapping**: Convenient button to swap origin and destination
- **Expandable Result Cards**: Click to expand journey cards for more details
- **Loading Indicators**: Visual feedback during search operations
- **Input Validation**: Prevents selecting same origin and destination

### Error Handling and Reliability
- **Global Error Handling**: Centralized error management via middleware
- **User-Friendly Error Messages**: Clear, actionable error information
- **Session Management**: Robust session handling for API authentication via custom middleware

## Additional Enhancements

- **Comprehensive Session Management**: Maintains device sessions for each user via ASP.NET Core session storage
- **Responsive Design**: Mobile-friendly layout with appropriate spacing and sizing
- **Fixed Price Formatting**: Consistent display of prices with exact two decimal places
- **Fixed-Width Price Containers**: Price containers sized to handle 5 digits, comma, and two decimal places
- **AJAX-Powered Dropdowns**: Fast, responsive location search with Select2
- **Development/Production Environment Detection**: Different error detail levels based on environment

## Technologies Used

- **Framework**: ASP.NET Core MVC
- **Frontend**: Bootstrap, jQuery, Select2, Bootstrap Icons
- **API Integration**: Obilet.com Business API
- **Caching**: In-memory caching for location data
- **Session Management**: ASP.NET Core Session with JSON serialization
- **Error Handling**: Custom middleware and MVC error pages
- **State Management**: LocalStorage for client-side persistence

## Obilet Session Middleware

The application uses a custom middleware for managing Obilet API sessions across requests.

### Key Features

- **Automated Session Management**: Automatically creates and maintains Obilet API sessions
- **Performance Optimization**: Skips session creation for static resources and error pages
- **Request Pipeline Integration**: Integrates seamlessly into the ASP.NET Core pipeline
- **Context Sharing**: Makes sessions available to controllers via HttpContext

### Registration

Registered in `Program.cs` after the session middleware:

```csharp
app.UseSession(); // ASP.NET Core session middleware
app.UseObiletSession(); // Custom session middleware
```

### Usage in Controllers

Access the session using the extension method:

```csharp
public async Task<IActionResult> Search(JourneySearchModel model)
{
    // Get the device session from the context
    var session = HttpContext.GetObiletSession();
    
    // Use session in API calls
    var journeyRequest = new JourneyRequest
    {
        DeviceSession = session,
        // Other properties...
    };
    
    // Continue with API call...
}
```

### Path Exclusion

The middleware automatically skips session creation for:
- Static resources (`/css`, `/js`, `/lib`, `/images`)
- Error pages (`/error`, `/home/error`)
- Favicon requests

## Deployment

### Live Demo
A live version of the application is deployed and accessible at:

**[https://journeysearch.onrender.com/](https://journeysearch.onrender.com/)**

This deployment is hosted on Render and automatically updates when changes are pushed to the main branch via GitHub Actions.

### Deployment Configuration
- **Platform**: Render Web Service
- **Deployment Method**: Docker container via GitHub Actions
- **Automated Deployment**: Configured through `.github/workflows/deploy.yml`
- **Manual Deployment**: Available through GitHub Actions workflow_dispatch event

## Setup and Configuration

### Prerequisites
- .NET 9.0 SDK or newer
- Visual Studio 2022 or Visual Studio Code with C# extensions

### Configuration
The application uses the following configuration settings:

```json
{
  "ObiletApi": {
    "BaseUrl": "https://v2-api.obilet.com/api/",
    "Token": "Your-API-Token",
    "ClientType": 1,
    "DeviceType": 7
  }
}
```

### Running the Application
1. Clone the repository
2. Update API configuration in `appsettings.json` with your Obilet API credentials
3. Run the application using Visual Studio or `dotnet run` command

## Usage

1. Open the application in a web browser
2. Enter or search for origin and destination locations
3. Select a departure date using the date picker or quick buttons
4. Click "Search" to find available journeys
5. View journey results with times, prices, and bus information
6. Click on a journey card to expand and see more details

## Future Improvements

- Distributed session storage (e.g., Redis) for multi-instance deployments
- User authentication to link Obilet sessions across devices
- Comprehensive testing (unit and integration tests)
- Accessibility improvements
- Centralized logging and monitoring
