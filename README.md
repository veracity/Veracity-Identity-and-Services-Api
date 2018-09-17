# Veracity.Services.Api

See [https://developer.veracity.com/doc/service-api](https://developer.veracity.com/doc/service-api) for full documentation

# Purpose

> provide a unified and simple api for interacting with Veracity Services. The services are all RESTfull with JSON as the default data format.

# Vocabulary

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

## Model documentation

In[ Swagger Ui](/swagger/ui/index) you can find descriptions of the models and the properties on them by navigating to the 'Model' view under response class and under paramers -> parameter -> data type

![Model description](/content/ModelDetailsSample.PNG "Model description")

## View points

### My
This view-point represents the loged in user. 

> Note that you are required to validate that the user has accepted terms of use for the platform and your servcie (if applicable) each time a user accesses your service.
In order to do this, after receiving the authorization code using OIDC, you call the ValidatePolicy endpoint and redirect to the provided url if needed. (see the hello world for details)

### This
the "This" view-point is the service/application's point ov view. The application has users (persons and or organizations), a set of capabillities and are able to send notifications etc.

### Directory (formerly Discover)
This is a common viewpoint allowing your app to look up different masterdata and resources with in myDNVGL. The main categories are: Services, Users and companies.

### Options

To find the CORS requirements and viewpoint rights requirements use the options verb for the different viewpoints

# Security

This api supports A OAuth2 bearer tokens. With *User* we understand authorization flows that involve a user. In most cases this will be Authorization Code flow. 


|View-point |Authorization type required|Comments                                   |Authorization rule                     |
|-----------|---------------------------|-------------------------------------------|---------------------------------------|
|My         |User                       |Only accessable when action on behalf of a user|User must exist in Mydnvgl|
|Our        |User                       |Only accessable when action on behalf of a user|User must exist in Mydnvgl|
|This       |User or ClientCredetial    |The client id must have basic access rights when used with a principal. Or 'deamon' rights and access to the feature for the service|User + clientId.read or clientId+clientId.read|
|Directory  |User or ClientCredetial    |The client id must have basic access rights when used with a principal. Or 'deamon' rights and access to the feature for the service|User + clientId.read or clientId+clientId.read|


## Common HTTP headers

|Header name|Description|
|:----------------------|:--------------------------------------------------------------------------------------------------|
|x-supportCode          |provides a unified way of correlating log entries accross all system components                    |
|x-serviceversion       |the api build number                                                                               |
|x-timer                |The time spent on the server producing the response                                                |
|x-region               |the azure region serving the request                                                               |
|x-principal            |the user the request was executen on behalf of                                                     |
|x-view-point           |the current view point                                                                             |
|x-actor                |the user id of the actor/service account                                                           |

## Error responses


|Http status|Status Name            |Description                                                                   |
|-----------|-----------------------|------------------------------------------------------------------------------|
|500        |Internal Server Error  |Something went wrong while processing the request.                            |
|404        |Not Found              |The requested resource was not found.                                         |
|400        |Bad Request            |Something was wrong with the request or the request parameters                |
|300        |Ambiguous              |More than one resource was found, use a more spesific version of the request. |
|403        |Forbidden              |Not sufficient rights or authorization is missing from request                |
|401        |Unauthorized           |Request is not authorized										               |
|501        |Not Implemented        |The action is not currently implemented, will be supported in future releases |


### Response body

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
