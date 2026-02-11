# Deusald Server Tools — Backend & Client Libraries

**Deusald Server Tools** is a small pair of .NET libraries that help you build **strongly-typed** backend endpoints and a **typed client SDK** for:
- **HTTP (REST-like) requests**
- **SignalR hub requests + hub messages**

The intended workflow is: **shared contracts** → **backend resolvers** → **client sends strongly-typed requests**.

This repo contains two projects:

- **DeusaldServerToolsBackend** — server-side helpers for ASP.NET Core + SignalR
- **DeusaldServerToolsClient** — client SDK (REST + SignalR) + shared request abstractions

---

## Requirements / Target frameworks

- **Backend:** `net10.0`
- **Client:** `netstandard2.1` and `net10.0`

(Your app can target a newer framework as long as it can reference these.)

---

## Core ideas

### 1) Shared contracts
Requests and responses live in a shared project referenced by both server and client.

- HTTP request: derive from `RequestBase<TResponse>`
- Hub request: derive from `HubRequestBase<TResponse>`
- Validate inputs by overriding `VerifyData()`
- Define routing by overriding `Address` (and `SendMethod` for HTTP)

### 2) Backend resolvers
Each request type is handled by a resolver class on the server:
- HTTP resolver: `ProtoPairBinaryEndpointResolver<TRequest, TResponse>`
- Hub resolver:  `ProtoPairBinaryHubEndpointResolver<TRequest, TResponse>`

Resolvers are discovered and mapped automatically when decorated with `[Endpoint]`.

### 3) Client sends typed requests
From the client, you call `.SendAsync(apiClient)` on the request instance.  
The SDK handles serialization, error callbacks, and (optionally) JWT.

---

# DeusaldServerToolsBackend

## Install

Reference the project or NuGet package used by your solution.

---

## Minimal server setup

A typical `Program.cs`:

```csharp
using DeusaldServerToolsBackend;

var builder = WebApplication.CreateBuilder(args);

// Base ASP.NET Core setup (controllers/json, cors in dev, etc.)
builder.DeusaldBaseBuilderConfigure();

// JWT auth helper (issuer/audience/signing key via env var)
builder.DeusaldAuthBuilderConfigure(
    issuer:  Auth.ISSUER,
    audience: Auth.AUDIENCE,
    expireTimeDays: 7,
    registeredClaimNames: TokenModel.Empty.GetData().Keys.ToList()
);

// Register resolvers (scan one or more assemblies)
builder.Services.AddEndpointResolvers(typeof(Program).Assembly);

// Your SignalR hub
builder.Services.AddSignalR();

var app = builder.Build();

// Standard middleware (auth, routing, health, etc.)
app.DeusaldBaseWebAppConfigure(emptyFavicon: true);

// Map resolvers + hub
app.MapEndpointResolvers(typeof(Program).Assembly);
app.DeusaldBaseHubWebAppConfigure<GameHub>();

app.Run();
```

### Environment variables used by the built-in helpers

- `AUTH_SIGNING_KEY` (required for JWT signing/validation)
- `SERVICE_VERSION`, `COMMIT_HASH` (used by logging helpers)
- Your app may also define its own env vars (the example uses `BACKEND_VERSION` inside `TokenModel`).

---

## Defining a HTTP request + response (shared contracts)

```csharp
using DeusaldServerToolsClient;
using DeusaldSharp;

public partial class LoginRequest : RequestBase<LoginResponse>
{
    public override SendMethodType SendMethod => SendMethodType.Http_Post;
    public override string Address            => "auth/login";

    [ProtoField(1)] public string Username = string.Empty;

    public override void VerifyData()
        => Username.VerifyStringLength(nameof(Username), 0, 16);
}

public partial class LoginResponse : ResponseBase
{
    [ProtoField(1)] public Guid     UserId;
    [ProtoField(2)] public string   Username = string.Empty;
    [ProtoField(3)] public string   JwtToken = string.Empty;
    [ProtoField(4)] public DateTime ExpiresAt;
}
```

> The examples use **DeusaldSharp** proto attributes (`[ProtoField]`) for binary serialization.

---

## Implementing the HTTP resolver

```csharp
using System.Security.Claims;
using DeusaldServerToolsBackend;
using DeusaldServerToolsClient;

[Endpoint]
[EndpointAllowAnonymous]
public class LoginResolver(
    ServerResponseHelper serverResponseHelper,
    ITokenProvider tokenProvider
) : ProtoPairBinaryEndpointResolver<LoginRequest, LoginResponse>(serverResponseHelper)
{
    protected override int _RequestMaxBytesCount => 1024;

    protected override Task<LoginResponse> HandleAsync(ClaimsPrincipal? user, LoginRequest request)
    {
        // validate, build token, etc...
        // throw new ServerException(ErrorCode.CustomError, "...") to return typed errors

        return Task.FromResult(new LoginResponse { /* ... */ });
    }
}
```

### Authentication attributes

- Protect an endpoint: `[EndpointAuthorize]`
- Allow anonymous: `[EndpointAllowAnonymous]`

---

## Defining & handling a hub request

```csharp
using DeusaldServerToolsClient;

public partial class HeartbeatRequest : HubRequestBase<HeartbeatResponse>
{
    public override string Address => "heartbeat";
    public override void VerifyData() { }
}
```

Resolver:

```csharp
using DeusaldServerToolsBackend;

[Endpoint]
public class HeartBeatResolver(ServerResponseHelper serverResponseHelper)
    : ProtoPairBinaryHubEndpointResolver<HeartbeatRequest, HeartbeatResponse>(serverResponseHelper)
{
    protected override int  _RequestMaxBytesCount => 1024;
    protected override bool _HeartBeat            => true;

    protected override Task<HeartbeatResponse> HandleAsync(HubRequestContext ctx, HeartbeatRequest request)
        => Task.FromResult(new HeartbeatResponse());
}
```

---

# DeusaldServerToolsClient

## Install

Reference the project or NuGet package:

- Package id: **DeusaldServerToolsClient**

---

## Creating an APIClient

```csharp
using System.Net;
using DeusaldServerToolsClient;

Dictionary<BackendAddress, string> addresses = new()
{
    { BackendAddress.Localhost, "http://127.0.0.1:50001" }
};

void OnRequestError(string requestType, HttpStatusCode code, ErrorCode errorCode, string message, Guid id)
    => Console.WriteLine($"Request {requestType} error {code}/{errorCode}: {message} ({id}).");

APIClient apiClient = new APIClient(
    addresses,
    withDispatcher: false,
    onRequestError: OnRequestError,
    logProvider: new ConsoleLogProvider(LogLevel.Information),
    clientVersion: new Version(0, 1, 0)
);

apiClient.SetBaseAddress(BackendAddress.Localhost);
```

---

## Sending a HTTP request

```csharp
LoginResponse? response = await new LoginRequest
{
    Username = "user1"
}.SendAsync(apiClient);

if (response != null)
{
    apiClient.UpdateJwtToken(response.JwtToken);
}
```

`SendAsync` returns `null` if the request fails (and triggers your error callback).

---

## Connecting to SignalR

```csharp
Exception? ex = await apiClient.ConnectToHubAsync("/hub");
if (ex != null) Console.WriteLine(ex);
```

Useful events:

```csharp
apiClient.OnConnectedToHub      += Console.WriteLine;
apiClient.OnDisconnectedFromHub += Console.WriteLine;
apiClient.OnExceptionOnMessage  += Console.WriteLine;
apiClient.OnUnauthorizedError   += () => Console.WriteLine("Unauthorized!");
```

---

## Sending hub requests

```csharp
var hb = await new HeartbeatRequest().SendAsync(apiClient);
```

Fire-and-forget:

```csharp
new HeartbeatRequest().SendAsync(apiClient, ignoreError: true).Forget();
```

---

## License

MIT License
