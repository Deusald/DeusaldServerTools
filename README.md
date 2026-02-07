# Deusald Server Tools — Server & Client Libraries

**Deusald Server Tools** is a pair of lightweight .NET libraries for building strongly-typed backends and clients using **ASP.NET Core + SignalR**.

This repository contains two main libraries:

* **DeusaldServerToolsBackend** → server framework
* **DeusaldServerToolsClient** → client SDK

They are designed to work together using shared request/response contracts and attribute-based resolvers.

---

## Overview

The libraries provide:

* Strongly-typed REST endpoints
* Strongly-typed SignalR hub messaging
* Shared contracts between client and server
* JWT authentication helpers
* Automatic serialization & error handling
* Simple client API for REST and hub calls
* Attribute-driven endpoint registration

The goal is to remove boilerplate and make backend/client communication predictable and type-safe.

---

# DeusaldServerToolsBackend

## Purpose

The backend library provides infrastructure for building ASP.NET Core servers with:

* Attribute-based REST resolvers
* SignalR hub request resolvers
* Authentication helpers
* Centralized error handling
* Security stamp validation
* Typed endpoint routing

---

## Installation

Add a reference to:

```
DeusaldServerToolsBackend
```

and your shared contracts project.

---

## Backend Setup

### Basic server bootstrap

In `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.SetupDefaultWebHost();

var app = builder.Build();

app.MapResolverEndpoints();
app.MapHub<GameHub>("/hub");

app.Run();
```

This automatically registers all resolvers marked with attributes.

---

## Creating a REST Endpoint

### 1. Define shared contracts

```csharp
public class LoginRequest : ProtoMsg<LoginRequest>, IAPIRequest<LoginRequest, LoginResponse>
{
    public const string         URL              = "/auth/login";
    public const HttpMethodType HTTP_METHOD_TYPE = HttpMethodType.POST;
    
    public string Username = "";
    
    static LoginRequest()
        {
            _model = new ProtoModel<LoginRequest>(
                ProtoField.String(1, static (ref LoginRequest o) => ref o.Username)
            );
        }

        public void VerifyData()
        {
            Username.VerifyStringLength(nameof(Username), 0, 16);
        }
}

public class LoginResponse : ProtoMsg<LoginResponse>, IResponse
{
     public Guid     UserId;
     public string   Username = "";
     public string   JwtToken = "";
     public DateTime ExpiresAt;
     
     static LoginResponse()
        {
            _model = new ProtoModel<LoginResponse>(
                ProtoField.Guid(1, static (ref LoginResponse o) => ref o.UserId),
                ProtoField.String(2, static (ref LoginResponse o) => ref o.Username),
                ProtoField.String(3, static (ref LoginResponse o) => ref o.JwtToken),
                ProtoField.DateTime(4, static (ref LoginResponse o) => ref o.ExpiresAt)
            );
        }
}
```

### 2. Create a resolver

```csharp
[Endpoint(typeof(LoginRequest))]
[EndpointAllowAnonymous]
public class LoginResolver(ServerResponseHelper serverResponseHelper, ITokenProvider tokenProvider)
    : BoxedBinaryEndpointResolver<LoginRequest, LoginResponse>(serverResponseHelper)
{
    protected override int RequestMaxBytesCount => 1024;
    
    protected override Task<LoginResponse> HandleAsync(ClaimsPrincipal? claimsPrincipal, LoginRequest request)
    {
        UsernameVerificator verificator = new UsernameVerificator(
            minCharacters: 3,
            maxCharacters: 16,
            whitespaceRequirement: true,
            charactersRequirementRegex: "A-Za-z0-9 _"
        );

        if (!verificator.CheckUsernameRequirements(request.Username)) 
            throw new ServerException(ErrorCode.CustomError, "Username do not fulfill requirements!");

        Guid userId = new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(request.Username))[..16]);

        TokenModel model = new TokenModel
        {
            Username       = request.Username,
            ClientVersion  = claimsPrincipal.GetClientVersion(),
            UserId         = userId,
            BackendVersion = BaseEnvVariables.BACKEND_VERSION.GetEnvironmentVariable()!,
            SecurityStamp  = Guid.NewGuid()
        };

        string token = TokenBuilder.CreateToken(tokenProvider, model, out DateTime expirationTime);
        return Task.FromResult(new LoginResponse
        {
            UserId    = userId,
            Username  = request.Username,
            JwtToken  = token,
            ExpiresAt = expirationTime
        });
}
```

The endpoint is automatically registered at `/login`.

---

## Creating a Hub Resolver

### 1. Define hub request/response

```csharp
public class HeartBeatRequest : ProtoMsg<HeartBeatRequest>, IHubRequest<HeartBeatRequest, HeartBeatResponse>
    {
        public const string HUB_URL = nameof(HeartBeatRequest);
        
        static HeartBeatRequest()
        {
            _model = new ProtoModel<HeartBeatRequest>();
        }
        
        public void VerifyData() { }
    }

    public class HeartBeatResponse : ProtoMsg<HeartBeatResponse>, IResponse
    {
        static HeartBeatResponse()
        {
            _model = new ProtoModel<HeartBeatResponse>();
        }
    }
```

### 2. Implement resolver

```csharp
[HubRequest(typeof(HeartBeatRequest))]
public class HeartBeatResolver(ServerResponseHelper serverResponseHelper) 
    : BoxedHubRequestResolver<HeartBeatRequest, HeartBeatResponse>(serverResponseHelper)
{
    protected override int  RequestMaxBytesCount => 1024;
    protected override bool HeartBeat            => true;

    protected override Task<HeartBeatResponse> HandleAsync(HubRequestContext ctx, HeartBeatRequest request)
    {
        return Task.FromResult(new HeartBeatResponse());
    }
}
```

---

## Authentication

JWT helpers are included:

```csharp
builder.DeusaldAuthBuilderConfigure(Auth.ISSUER, Auth.AUDIENCE, 7, TokenModel.Empty.GetData().Keys.ToList());
```

You can protect endpoints with:

```csharp
[EndpointAuthorize]
```

or allow anonymous access:

```csharp
[EndpointAllowAnonymous]
```

---

## Error Handling

Throw a `ServerException` inside resolvers to return structured errors to clients:

```csharp
throw new ServerException(
    ErrorCode.InvalidRequest,
    "Invalid credentials");
```

---

# DeusaldServerToolsClient

## Purpose

The client library provides a simple API for:

* Calling REST endpoints
* Connecting to SignalR hubs
* Sending typed hub requests
* Receiving hub messages
* Managing JWT authentication
* Centralized error callbacks

It is designed for desktop, mobile, and WebAssembly clients.

---

## Installation

Add a reference to:

```
DeusaldServerToolsClient
```

and your shared contracts project.

---

## Creating an APIClient

Example setup (Blazor/WebAssembly style):

```csharp
Dictionary<BackendAddress, string> addresses = new()
{
    { BackendAddress.Localhost, "http://127.0.0.1:50001" }
};

void OnRequestError(
    string requestType,
    HttpStatusCode code,
    ErrorCode errorCode,
    string message,
    Guid id)
{
    Console.WriteLine(
        $"Error {requestType}: {message}");
}

var apiClient = new APIClient(
    addresses,
    false,
    OnRequestError,
    new ConsoleLogProvider(LogLevel.Information),
    new Version(0, 1, 0)
);

apiClient.SetBaseAddress(BackendAddress.Localhost);
```

---

## Calling a REST Endpoint

```csharp
LoginResponse? response =
    await new LoginRequest
    {
        Username = "user1"
    }.SendRESTAsync(apiClient);

if (response != null)
{
    apiClient.UpdateJwtToken(response.JwtToken);
}
```

`SendRESTAsync` returns `null` if the request fails.

---

## Connecting to a Hub

```csharp
Exception? exception =
    await apiClient.ConnectToHubAsync("/hub");

if (exception != null)
{
    Console.WriteLine(exception);
}
```

You can subscribe to connection events:

```csharp
apiClient.OnConnectedToHub      += Console.WriteLine;
apiClient.OnDisconnectedFromHub += Console.WriteLine;
apiClient.OnUnauthorizedError   += () =>
    Console.WriteLine("Unauthorized!");
```

---

## Sending Hub Requests

```csharp
var response =
    await new HeartBeatRequest()
        .SendHubAsync(apiClient);

if (response != null)
{
    Console.WriteLine(response.ServerTime);
}
```

You can also send fire-and-forget messages:

```csharp
new HeartBeatRequest()
    .SendHubAsync(apiClient, true)
    .Forget();
```

---

## Receiving Hub Messages

Register handlers for hub messages:

```csharp
apiClient.RegisterToHubMsg<NotificationHubMsg>(
    msg => Console.WriteLine(msg.Message));
```

---

## JWT Management

Update or clear authentication:

```csharp
apiClient.UpdateJwtToken(jwtToken);
apiClient.UpdateJwtToken(string.Empty); // logout
```

The token is automatically attached to requests.

---

## Logging

The client supports pluggable logging:

```csharp
new ConsoleLogProvider(LogLevel.Information)
```

You can implement your own provider if needed.

---

# Recommended Workflow

1. Define shared contracts in a common project
2. Implement resolvers in the backend
3. Call them using the client SDK
4. Use attributes for routing and security

No manual routing or serialization setup is required.

---

## License

MIT License
