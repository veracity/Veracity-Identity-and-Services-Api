# Veracity Identity and Services Api

## Veracity Identity

> Veracity Identity can be used standalone or as the foundation for calling the Veracity API's.

### Veracity Identity (Standalone)

In this part we will go through the steps you need in order to connect your asp.net or aspnetcore application to Veracity Identity. The Veracity Identity libraries contain helper methods and tools to build 
applications that authenticate with Veracity and the necessary infrastructure to obtain an access token and protect these. 

### aspnetcore

Install the Veracity Identity Libraries (VIL)

```NUGET
PM> Install-Package Veracity.Common.Authentication.AspNetCore -version 2.0.0
```
modify your startup.cs 

Add the folowing methods:
```CS
private IDistributedCache ConstructDistributedCache(IServiceProvider s)
{
    return new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
}

private IDataProtector ConstructDataProtector(IServiceProvider s)
{
    return new DataProtector<IDataProtectionProvider>(s.GetDataProtectionProvider(), (p, data) => p.CreateProtector("token").Protect(data), (p, data) => p.CreateProtector("token").Unprotect(data));
}

```
these two methods allow you to control how and were the access tokens are stored. In this example we use a memory cache with basic protection. For 
distributed/scalable applications you need to change *ConstructDistributedCache* to use a persistent store like Redis or SQL, Microsoft has pacakges that implements
*IDistributedCache* for SQL and Redis.
In distributed/scalabe applications you need to protect the tokens in the same way in every instance, meening you need to provide a cryptographic key or certificate. 
see [https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.dataprotection.idataprotectionbuilder?view=aspnetcore-2.2](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.dataprotection.idataprotectionbuilder?view=aspnetcore-2.2) for details on how to use shared keys between dataprotector instances.

Add the Veracity infrastructure to the service collection
```CS
public void ConfigureServices(IServiceCollection services)
        {
            services.AddVeracity(Configuration)
                .AddSingleton(ConstructDataProtector)
                .AddSingleton(ConstructDistributedCache).Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            }).AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddVeracityAuthentication(Configuration)
                .AddCookie();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
```
1. add the Veracity Core Infrastructure (*AddVeracity(Configuration)*)
2. add the data protector (*AddSingleton(ConstructDataProtector)*)
3. add the distributed cache (*.AddSingleton(ConstructDistributedCache)*)
4. add the Veracity Identity Authentication schema (*AddVeracityAuthentication(Configuration)*)
5. add cookie authentication (*AddCookie()*)

Make sure you include UseAuthentication to enable the authentication middleware
```CS
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCookiePolicy();

    app.UseVeracity()
        .UseAuthentication()//add the auth middleware
        .UseMvc();
}
}
```

Configuration

```JSON
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "Veracity": {
    "RedirectUrl": "https://localhost:44326/signin-oidc",
    "TenantId": "a68572e3-63ce-4bc1-acdc-b64943502e9d",
    "ClientId": "{YourClientId}",
	//"ClientSecret":"{YourClientSecred(should be stored in keyvault)}"
    "Scope": "https://dnvglb2cprod.onmicrosoft.com/83054ebf-1d7b-43f5-82ad-b2bde84d7b75/user_impersonation",
    "Policy": "B2C_1A_SignInWithADFSIdp",
    "MyServicesApi": "https://api.veracity.com/Veracity/Services/V3",
    "Instance": "https://login.microsoftonline.com/tfp/",
    "CallbackPath": "/signin-oidc",
    "Domain": "dnvglb2ctest.onmicrosoft.com",
    "SignUpSignInPolicyId": "B2C_1A_SignInWithADFSIdp",
    "ResetPasswordPolicyId": "B2C_1A_SignInWithADFSIdp"
  }
}
```

##### Key Vault 

Naming convension: {SegmentName}--{PropertyName} => Veracity--ClientSecret

Include the KeyVault Configuration provider by replacing the constructor of startup.cs

```CS
public Startup(IHostingEnvironment env)
{
    var azureServiceTokenProvider = new AzureServiceTokenProvider();
    var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
        .AddAzureKeyVault({YourKeyVaultUrl}, new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback)), new DefaultKeyVaultSecretManager())
        .AddEnvironmentVariables();
    Configuration = builder.Build();
}
```


#### asp.net

Install the Veracity Identity Libraries (VIL)

```NUGET
PM> Install-Package Veracity.Common.Authentication.AspNet -version 2.0.0
```

For the .net framework it is a bit more fiddly to get the Authentication set up.
But we have tried to make the steps as similar to aspnetcore as possible.

First of all, this library expects the existance of a IOC container. Most of the mature IoC frameworks does support the IServiceCollection in some form so internally we use this.
In our sample we will use *Microsoft.Extensions.DependencyInjection* as the IoC container through the wrapper library *Stardust.Interstellar.Rest.DependencyInjection*

Add the folowing lines to global.asax.cs
```CS
protected void Application_Start()
        {
            ConfigurationManagerHelper.SetManager(new ConfigManager());//Veracity mandatory
            this.AddDependencyInjection<AppServiceConfig>();//Add the MS DI 
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
```

add a service binding setup class
```CS
 public class AppServiceConfig:ServicesConfiguration 
{
    protected override IServiceCollection Configure(IServiceCollection services)
    {
		//Add your own services to the serviceCollection
        return services;
    }
}
```

Add Owin and owin startup.cs 


Add cache constructor function
```CS
private static DistributedTokenCache CacheFactoryFunc()
        {
            return new DistributedTokenCache(HttpContext.Current.User as ClaimsPrincipal, DistributedCache, null, null);
        }

        private static MemoryDistributedCache DistributedCache { get; } =
            new MemoryDistributedCache(
                new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
```
You can inject a dataprotector into DistributedTokenCache as the last parameter, see aspnetcore for details

Add the configuration code
```CS
public void Configuration(IAppBuilder app)
{
	//Get secrets from Azure Key Vault using managed service identity (MSI)
    var azureServiceTokenProvider = new AzureServiceTokenProvider();
    var keyVaultClient = new KeyVaultClient(async (authority, resource, scope) =>
        await azureServiceTokenProvider.GetAccessTokenAsync(resource));
    var secret = keyVaultClient.GetSecretAsync("https://veracitydevdaydemo.vault.azure.net/",
        "Veracity1--ClientSecret").Result;
    var subscriptionKey = keyVaultClient
        .GetSecretAsync("https://veracitydevdaydemo.vault.azure.net/", "Veracity--SubscriptionKey").Result;
	//Add cookie authentication
    app.UseCookieAuthentication(new CookieAuthenticationOptions { CookieName = "a.c" }); //set auth cookie
    app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType); //set default auth type 
    //configure veracity auth
    app.UseVeracityAuthentication(new TokenProviderConfiguration
    {
        ClientSecret = secret.Value,
        SubscriptionKey = subscriptionKey.Value
    }) //Add Azure Ad B2C authentication 
        .UseTokenCache(CacheFactoryFunc); //add access token cache 
}
```

Configuration

```XML
<appSettings>
    <add key="webpages:Version" value="3.0.0.0" />
    <add key="webpages:Enabled" value="false" />
    <add key="ClientValidationEnabled" value="true" />
    <add key="UnobtrusiveJavaScriptEnabled" value="true" />
    <add key="apiGW:clientId" value="yourAppId" />
    <add key="apiGW:policy" value="B2C_1A_SignInWithADFSIdp" />
    <add key="apiGW:scope" value="	https://dnvglb2cprod.onmicrosoft.com/83054ebf-1d7b-43f5-82ad-b2bde84d7b75/user_impersonation" />
    <add key="apiGW:redirectUrl" value="yourAppUrl" />
    <add key="apiGW:idp" value="a68572e3-63ce-4bc1-acdc-b64943502e9d" />
    <add key="myApiV3Url" value="https://api.veracity.com/Veracity/Services/V3" />
    <add key="apiGW:clientSecret" value="yourSecret" />
    <add key ="subscriptionKey" value="yourSubscriptionKey"/>
  </appSettings>
```

#### Common 

To make a page/controller protected add the Authorize attribute to the controller or the mathod on the controller.

```CS
[Authorize]//Require autheniticated user
public class HomeController : Controller
{
    public ActionResult Index()
    {
        if (Request.IsAuthenticated)
        {
            ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
        }

        return View();
    }

    public ActionResult About()
    {
        if (Request.IsAuthenticated)
        {
            ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
        }
        ViewBag.Message = "Your application description page.";

        return View();
    }

    public ActionResult Contact()
    {
        if (Request.IsAuthenticated)
        {
            ViewBag.Email = (User.Identity as ClaimsIdentity)?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
        }
        ViewBag.Message = "Your contact page.";

        return View();
    }
}
```

### Veracity Identity with Services api



## Veracity.Services.Api

See [https://developer.veracity.com/doc/service-api](https://developer.veracity.com/doc/service-api) for full documentation

### Purpose

> provide a unified and simple api for interacting with Veracity Services. The services are all RESTfull with JSON as the default data format.

### Vocabulary

With the myDNVGL api V3 we are trying to make a unified and consistent vocabulary for interacting with myDNVGL. At the heart of the API lies a set of view-points that represents the data seen from an 
actors point of view. 

The api follows a normal usage of http verbs and nouns in the URI's. With the exception of some RPC type actions that uses '()' at the end of the action. example:
GET /my/policy/validate()

Collection responses will return a list of simplified representations with the url to the complete representation of that item. Example:

```JSON
[
   {
    "identity": "/directory/companies/1314941d-e574-46d3-9089-ef7639428d69",
    "name": "MyDNVGL Ltd",
    "id": "1314941d-e574-46d3-9089-ef7639428d69"
  },
  {
    "identity": "/directory/companies/407e9e2d-307f-43d8-909b-23fd005d50ed",
    "name": "Article 8 Demo Company",
    "id": "407e9e2d-307f-43d8-909b-23fd005d50ed"
  }
]
```

#### Model documentation

In[ Swagger Ui](/swagger/ui/index) you can find descriptions of the models and the properties on them by navigating to the 'Model' view under response class and under paramers -> parameter -> data type

![Model description](/content/ModelDetailsSample.PNG "Model description")

#### View points

##### My
This view-point represents the loged in user. 

> Note that you are required to validate that the user has accepted terms of use for the platform and your servcie (if applicable) each time a user accesses your service.
In order to do this, after receiving the authorization code using OIDC, you call the ValidatePolicy endpoint and redirect to the provided url if needed. (see the hello world for details)

##### This
the "This" view-point is the service/application's point ov view. The application has users (persons and or organizations), a set of capabillities and are able to send notifications etc.

##### Directory (formerly Discover)
This is a common viewpoint allowing your app to look up different masterdata and resources with in myDNVGL. The main categories are: Services, Users and companies.

##### Options

To find the CORS requirements and viewpoint rights requirements use the options verb for the different viewpoints

### Security

This api supports A OAuth2 bearer tokens. With *User* we understand authorization flows that involve a user. In most cases this will be Authorization Code flow. 


|View-point |Authorization type required|Comments                                   |Authorization rule                     |
|-----------|---------------------------|-------------------------------------------|---------------------------------------|
|My         |User                       |Only accessable when action on behalf of a user|User must exist in Mydnvgl|
|Our        |User                       |Only accessable when action on behalf of a user|User must exist in Mydnvgl|
|This       |User or ClientCredetial    |The client id must have basic access rights when used with a principal. Or 'deamon' rights and access to the feature for the service|User + clientId.read or clientId+clientId.read|
|Directory  |User or ClientCredetial    |The client id must have basic access rights when used with a principal. Or 'deamon' rights and access to the feature for the service|User + clientId.read or clientId+clientId.read|


### Common HTTP headers

|Header name|Description|
|:----------------------|:--------------------------------------------------------------------------------------------------|
|x-supportCode          |provides a unified way of correlating log entries accross all system components                    |
|x-serviceversion       |the api build number                                                                               |
|x-timer                |The time spent on the server producing the response                                                |
|x-region               |the azure region serving the request                                                               |
|x-principal            |the user the request was executen on behalf of                                                     |
|x-view-point           |the current view point                                                                             |
|x-actor                |the user id of the actor/service account                                                           |

### Error responses


|Http status|Status Name            |Description                                                                   |
|-----------|-----------------------|------------------------------------------------------------------------------|
|500        |Internal Server Error  |Something went wrong while processing the request.                            |
|404        |Not Found              |The requested resource was not found.                                         |
|400        |Bad Request            |Something was wrong with the request or the request parameters                |
|300        |Ambiguous              |More than one resource was found, use a more spesific version of the request. |
|403        |Forbidden              |Not sufficient rights or authorization is missing from request                |
|401        |Unauthorized           |Request is not authorized										               |
|501        |Not Implemented        |The action is not currently implemented, will be supported in future releases |


#### Response body

The error response may have one of the two formats
```JSON
{
  "message": "Error message",
  "information": "additional info",
  "subCode": 9 //error code
}
```
```JSON
{
  "Message": "Error message"
}
```
