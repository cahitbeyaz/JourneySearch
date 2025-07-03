# Obilet Session Middleware Documentation

## Overview

The `ObiletSessionMiddleware` centralizes session management for the Obilet Journey Search application. It ensures that a valid API session is available to all relevant requests while avoiding unnecessary session creation for static resources and error pages.

## Session Lifecycle

1. **Request Initiation:**
   - When a request enters the application pipeline, it first passes through the standard ASP.NET Core session middleware
   - This ensures the HTTP session cookie is available

2. **Obilet Session Middleware Processing:**
   - The request then reaches our custom `ObiletSessionMiddleware`
   - The middleware checks if the request path requires a session (i.e., not a static file or error page)
   - If the request doesn't require a session, it skips processing and continues to the next middleware

3. **Session Retrieval:**
   - If the request needs a session, the middleware first tries to retrieve an existing session from HTTP session storage
   - If a valid session is found, it's placed in `HttpContext.Items["ObiletSession"]` for easy access throughout the request

4. **Session Creation:**
   - If no session exists, the middleware automatically creates a new one by calling the Obilet API
   - The new session is stored in both HTTP session storage (for persistence) and `HttpContext.Items` (for current request)
   - Session creation failures are logged but don't block the request, allowing error pages to function properly

5. **Controller Access:**
   - Controllers access the session using the `HttpContext.GetObiletSession()` extension method
   - This simplifies the code and removes the need for injecting and using `ISessionService`

6. **Session Storage:**
   - Sessions are serialized to JSON and stored in ASP.NET Core's session storage
   - By default, this uses in-memory storage, but can be replaced with distributed storage for production

## Path Exclusion Logic

The middleware intelligently skips session creation for:
- Static files (`/css`, `/js`, `/lib`, `/images`)
- Error pages (`/error`, `/home/error`)
- Favicon requests (`favicon.ico`)

This optimization prevents unnecessary API calls and improves performance.

## Error Handling

- If session creation fails, the middleware logs the error but allows the request to continue
- This ensures error pages work properly even when session creation fails
- Controllers check for null sessions and handle them appropriately with error messages

## Benefits

1. **Centralized Management:**
   - Session logic is handled in one place, not scattered across controllers
   - Consistent session handling across the application

2. **Reduced API Calls:**
   - Intelligent path exclusion prevents unnecessary session creation
   - Static resources load faster without API overhead

3. **Simplified Controller Code:**
   - Controllers no longer need to manage sessions directly
   - Access to sessions through simple extension method

4. **Better Error Handling:**
   - Error pages work even when session creation fails
   - Graceful degradation when API is unavailable

## Middleware Registration

```csharp
// In Program.cs
app.UseSession();
app.UseObiletSession();
app.UseAuthorization();
```

The middleware must be registered after `UseSession()` to ensure HTTP session storage is available.
