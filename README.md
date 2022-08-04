# Alice Identity Service (AIS)

[Alice Identity Service (AIS)](https://identity.cysun.org/) is an
[OpenID Connect (OIDC)](https://openid.net/connect/) identity provider.
Based on [OpenIddict](https://github.com/openiddict/openiddict-core) and
[ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity),
AIS provides the capabilities to manage users, claims, scopes, and clients, and serves as a single
sign-on (SSO) service for other applications and APIs.

## Installation

1. Create a PostgreSQL database and use `AliceIdentityService/Scripts/CreateSchema.sql` to create the tables.
  AIS uses Entity Framework Core so it is possible to use other DBMS like MySQL or MS SQL Server, though it'll
  need some minor changes (use different package, connection string, and extension method).
2. Copy `AliceIdentityService/appsettings.json.sample` to `AliceIdentityService/appsettings.json`, and change
  `appsettings.json` according to your environment.
3. Run `ConsoleManager`
  * Generate an encryption certificate and a signing certificate.
  * Create a user with the `ais-admin` claim. This user is the administrator who can manage users, scopes, and clients.
4. Run `AliceIdentityService`.

## Current Limitations

AIS only supports Authorization Code and Refresh Token grants and the `code` response type. Client Credentials will be
added when necessary.

All clients use `implicit` consent type, which means that the consent screen is never shown and a user cannot pick and
choose which requested scopes should be granted to a client. Note that this does *not* mean a client can request any
scope it wants - each client is still limited to its "allowed scopes" configured in the system.

## Screenshots

### Edit Client
![Screenshot of Edit Client UI](https://mynotes.cysun.org/files/view/1001488)

### Edit Scope
![Screenshot of Edit Scope UI](https://mynotes.cysun.org/files/view/1001486)

### Edit User
![Screenshot of Edit User UI](https://mynotes.cysun.org/files/view/1001570)