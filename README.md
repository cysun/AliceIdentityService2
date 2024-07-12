# Alice Identity Service (AIS)

[Alice Identity Service (AIS)](https://identity.cysun.org/) is an
[OpenID Connect (OIDC)](https://openid.net/connect/) identity provider.
Based on [OpenIddict](https://github.com/openiddict/openiddict-core) and
[ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity),
AIS provides the capabilities to manage users, claims, scopes, and clients, and serves as a single
sign-on (SSO) service for other applications and APIs.

## Installation

1. Create an empty PostgreSQL database. AIS uses Entity Framework Core so it is possible to use other DBMS like MySQL
  or MS SQL Server, though it'll need some changes to the code.
2. Populate the database using the following SQL scripts in the `AliceIdentityService/Scripts` folder:
  * `CreateSchema.sql` - create all the tables.
  * `PopulateSchema.sql` - create additional indexes, stored procedures, and so on.
3. Copy `AliceIdentityService/appsettings.json.sample` to `AliceIdentityService/appsettings.json`, and change
  `appsettings.json` according to your environment.
4. Run `ConsoleManager`
  * Generate an encryption certificate and a signing certificate.
  * Create a user with the `ais-admin` claim. This user is the administrator who can manage users, scopes, and clients.
5. Run `AliceIdentityService`.

## Email Configuration

AIS uses [Alice Mail Service (AMS)](https://github.com/cysun/AliceMailService) to send emails. For the email functions
(e.g. sending verification emails, password reset emails) to work, you need to set up AMS then configure the `RabbitMQ`
section in `appsettings.json` accordingly.

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