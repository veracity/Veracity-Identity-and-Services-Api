# API design.

## Defining the url's and actions 

An api is a collection of paths pointing at resources. In our api the reaources are grouped as My 
(the logged in user), This (the external services), [Our (the company, not implemented at the moment)] and Directory (lookup services, currently users, companies and services)

In code the api is grouped in controllers that are automatically implemented at runtime based on an interface.
This interface is decorated to instruct the tool how to generate the controller. The tool uses standard WebApi attributes whenever possible.

Make all actions task-async to optimize resource utilization on the server side (and in the client sdk)

```CSharp
    [IRoutePrefix("directory"), Oauth, ServiceInformation,  CircuitBreaker(100, 5), SupportCode, ErrorHandler(typeof(ExceptionWrapper)), AuthorizeWrapper]
    public interface ICompaniesDirectory
    {
        [Get, Route("companies/{id}")]
        Task<CompanyInfo> CompanyById([In(InclutionTypes.Path)]string id);

        [Get, Route("companies/{id}/users")]
        Task<IEnumerable<UserReference>> GetUsersByCompany([In(InclutionTypes.Path)] string id, [In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Get, Route("companies")]
        Task<IEnumerable<CompanyReference>> GetCompanies([In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize);

        [Get, Route("companies/search")]
        Task<IEnumerable<CompanyReference>> FindCompanies([In(InclutionTypes.Path)]string searchTerm,[In(InclutionTypes.Path)] int page, [In(InclutionTypes.Path)] int pageSize, [In(InclutionTypes.Path)] bool includeDeleted);
    }
```

### Verbs and nouns

There are no verbs in the paths (with some exceptions, like validate for the user policies).

* Plural 'gets' always return collections (GET /this/users)
* Singular 'gets' always returns a single object (GET /my/profile)
* the path /discovery/users/8B27B3C9-9E72-44F8-8A1B-0DBB289853DB is a singular statement
* For RPC type operations postfix the url with () 
    * /my/policy/validate()
        * this does not get messed up by url encoding.

The usage of the http verbs:

* Get
    * Read
* Post
    * Create
* Put
    * Replace/Update (Full update)
* Patch
    * Delta update (or for example: Fix issue with policy) 
* Delete
    * Delete 
        * Hard delete, the item is totally removed from storage/DB
        * Soft delete, the item is marked as deleted in storage/DB


## Request payload

Place all parameters that identify a resource (like user or company) in the path of the url, while parameters that identify selection criteria, selections, paging etc. is placed in the query part (behind the ?)

If a create, update or similar operations need extra parameters to describe behavior these should be placed in an options parameter of the message body.

```JSON
{
  "firstName": "Test",
  "lastName": "user",
  "email": "dest.user@veracity.com",
  "options": {
    "SendMail": true
  }
}
```

or for relationship description where the two resource id's are placed in the path use the an options object to send behavior information.

```CURL
curl -X PUT --header 'Content-Type: application/json' --header 'Accept: application/json'  -d '{ \    "accessLevel": "read" \   }'  https://localhost:44337/this/channel/someServiceId/subscribers/myId'
```


## Responses

### Succeess

For operations that does not have a logical response type (like delete and update) return void/NoContent (204) with optional extra info in the http headers.

For plurals (list) return a collection of reference objects that has as a minimum a name, id and a url to the detailed object.
```JSON
[
  {
    "url": "/discover/companies/ac87efaf-5462-4319-b485-b343bd886972",
    "name": "TestCompany1",
    "id": "ac87efaf-5462-4319-b485-b343bd886972"
  },
  {
    "url": "/discover/companies/26bd7dfc-2184-4394-8324-796448dcfaf4",
    "name": "Test compay 2,
    "id": "26bd7dfc-2184-4394-8324-796448dcfaf4"
  }
]
```

For Creates return a reference object with the http code 200 or 201 and the url in the http header x-location
```JSON
{
    "url": "/discover/companies/ac87efaf-5462-4319-b485-b343bd886972",
    "name": "TestCompany1",
    "id": "ac87efaf-5462-4319-b485-b343bd886972"
}
```

### Failure

Return an appropriate httpStatusCode with a datailed and helpful error message in the body of the response.

```JSON
{
    "message" : "Something bad happended",
    "information" : "Some additional information when appropriate",
    "subCode" : "42"
}
```


## Stardust.Interstellar.Rest, a quick guide

### Reasons

> It separates concerns. In this case, api design and implementation, but also non functional aspects 
>> A separation layer suppresses the urge to take shortcuts that transforms well designed software into a big pile of mud. 

> It makes it easy to show-case consepts  
>> Like security on the client side
> The .net client library is already built
>> No magic strings that slip detection of unit and integration tests

> Any software that consists heavily on boilerplate code are more likely to have inconsistencies and errors than generated software. Cos, boilerplate code is boring and developers are ~~lazy~~ only human. Generated boilerplate code gives consistency and a single point of maintenance at the cost of fine grained control and visibility.

### Built in attributes

|Attribute name     |Usage                          |WebApi equivalent                      |
|:-----------------:|:---------------------------------------------:|:---------------------:|
|IRoutePrefix       |the prefix of the url template                 |RoutePrefix            |
|Route              |the action url template                        |Route                  |
|Http*              |The http verb to use                           |Http*                  |
|In                 |Allocates the parameter to url, header, or body|FromUri or FromBody    |
|AuthorizeWrapper   |Make the controller or action require auth     |Authorize              |
|ServiceDescription |Add a description in the swagger (action       |                       |
|CircuitBreaker     |Makes the client use a circuit breaker when calling the service|       |

### Custom attributes and handlers

|Attribute name     |Usage                                              |
|:-----------------:|:-------------------------------------------------:|
|Oauth              |Instruct the client how to get the access token    |
|ServiceInformation |Add http headers with additional serivce info      |
|SupportCode        |gets/sets the support code from the http headers   |
|ExceptionWrapper   |converts exceptions to and from http status codes  |


### Making custom aspects

The tool supports the notion of custom aspects that allow the developer to add custom http headers, do interception of processing.
To make a header aspect:

```CSharp

    public class CustomHeaderAttribute : HeaderInspectorAttributeBase
    {
        public override IHeaderHandler[] GetHandlers()
        {
            return new[] { new SupportCodeHandler() };
        }
    }

    public class SupportCodeHandler : StatefullHeaderHandlerBase
    {
        protected override void DoSetHeader(StateDictionary state, HttpWebRequest req)
        {
            //Set http request header values in the client 
        }

        protected override void DoGetHeader(StateDictionary state, HttpWebResponse response)
        {
            //Get header values from the response in the client
            var code = response.Headers.Get("x-supportCode");
            
        }

        protected override void DoSetServiceHeaders(StateDictionary state, HttpResponseHeaders headers)
        {
            //Write http header values to the response message in the service
            headers.Add("x-supportCode", HttpContext.Current.Items["x-supportCode"].ToString());
           
        }

        protected override void DoGetServiceHeader(StateDictionary state, HttpRequestHeaders headers)
        {
            //Read header values from the http request in the service
            var supportCode = headers?.SingleOrDefault(h => h.Key == "x-supportCode");
        }

        public override int ProcessingOrder => 2;
    }


```

### Adding headers from functional code

By letting the service implement IServiceExtensions the controller context and a dictionary for getting/setting header values is exposed.

```CSharp

    public abstract class ServiceImplementationBase : IServiceExtensions
    {
        internal Dictionary<string, string> _headers;

        public void SetControllerContext(HttpControllerContext currentContext)
        {
            //Do not use the context as it creates a strong dependency with the webapi infrastructure
        }

        public void SetResponseHeaderCollection(Dictionary<string, string> headers)
        {
            _headers = headers;
        }

        public Dictionary<string, string> GetHeaders()
        {
            //Add the http header x-view-point to the response
            //Also a natural place to add page size, total pages etc. as well.
            //when using the _headers field make sure it uses a conditional access, so the service can be reused in non http contexts as well
            _headers?.Add("x-view-point", ServiceViewPointName);
            return _headers;
        }

        public abstract string ServiceViewPointName { get; }
    }

``` 