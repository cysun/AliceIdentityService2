# Alice Identity Service (AIS)

[Alice Identity Service (AIS)](https://identity.cysun.org/) is an OpenID Connect (OIDC) identity provider.
Based on [OpenIddict](https://documentation.openiddict.com/) and
[ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity),
AIS can provide single sign-on (SSO) for various applications with a web UI to manage users, claims, scopes,
and clients.

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

All clients use `implicit` consent type, which means the consent screen is never shown and a user cannot pick and choose
which requested scopes should be granted to a client. This is sufficent for now as AIS is mainly used for SSO.

## Screenshot

![Screenshot of Edit Client UI](https://mynotes.cysun.org/files/view/1001488)