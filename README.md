This repository contains a basic TODO Web API application built with ASP.NET minimal Web APIs.

Check out the article [Securing ASP.NET Minimal Web APIs with Auth0](https://auth0.com/blog/securing-aspnet-minimal-webapis-with-auth0/) for the implementation details.

# Requirements

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) installed on your machine
- Visual Studio 2022 (optional)

# To run this application

1. Clone the repo with the following command: 

   ```bash
   git clone https://github.com/auth0-blog/minimalwebapi.git
   ```

2. Move to the `minimalwebapi` folder 

3. Add your Auth0 domain and API identifier to the `appsettings.json` configuration file (see [Create an API in the Auth0 Dashboard](https://auth0.com/blog/securing-aspnet-minimal-webapis-with-auth0/#Create-an-API-in-the-Auth0-Dashboard) for more details).

4. Type `dotnet run` in a terminal window to launch the Web API.

5. Use curl or any other HTTP client to make requests to the API, as explained in the [Testing the API](https://auth0.com/blog/securing-aspnet-minimal-webapis-with-auth0/#Testing-the-API) section.
