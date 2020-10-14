# Veracity Authentication and authorization on the wire (in progress)

Veracity seen with developer eyes are multiple components, each that has its own set of challenges.
1. Veracity Identity (logging in with a VeracityId)
    - Token cache 
    - Token protection
2. Services API (Api V3)

## Veracity Identity

In this part we will try to cover the most common scenarios for authenticating with Veracity. 

### Web apps 

#### Authorization Code Flow



Process:
1. redirect user to IdP (includes clientId, redirect url, nonce)
2. home realm discovery 
3. Authentication (present username/password)
4. send authorization code back to app
5. validate auth code
6. exchange auth code with access token and refresh token

Refresh:
1. Use refresh token to obtain new access token


*signing redirect request sample:*
```
GET https://login.veracity.com/dnvglb2cprod.onmicrosoft.com/oauth2/v2.0/authorize?
client_id={yourClientId}
&response_type=code+id_token
&redirect_uri={yourRedirectUrl}
&response_mode=form_post
&scope=open_id%20offline_access%20https%3A%2F%2Fdnvglb2cprod.onmicrosoft.com%2F83054ebf-1d7b-43f5-82ad-b2bde84d7b75%2Fuser_impersonation
&state=arbitrary_data_you_can_receive_in_the_response
&nonce={nounce}
&p=B2C_1A_SignInWithADFSIdp
```
*auth response (form data):*
```
state: OpenIdConnect.AuthenticationProperties=TVNgMEdFrCwfNNfycENWX4IgSYY36pODDz0...
code: eyJraWQiOiJhOXZ5SjJVX25SM3ZmakZubEpUQlFLbH....
id_token: eyJ0eXAiOiJKV1QiLCJhbGc......

```

*Exchange auth code with access token:*
```
POST https://login.veracity.com/dnvglb2cprod.onmicrosoft.com/oauth2/v2.0/token?p=b2c_1_sign_in HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code&client_id={yourClientId}&scope=https://dnvglb2cprod.onmicrosoft.com/83054ebf-1d7b-43f5-82ad-b2bde84d7b75/user_impersonation offline_access&code={receivedAuthCode}&redirect_uri={yourRedirectUrl}&client_secret={your-application-secret}
```

*Refresh access token request:*
```
POST https://login.veracity.com/dnvglb2cprod.onmicrosoft.com/oauth2/v2.0/token?p=b2c_1_sign_in HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token&client_id={yourClientId}&client_secret={your-application-secret}&scope=https://dnvglb2cprod.onmicrosoft.com/83054ebf-1d7b-43f5-82ad-b2bde84d7b75/user_impersonation offline_access&refresh_token={receivedRefreshToken}&redirect_uri={yourRedirectUrl}
```
*Token response (both exchange auth code and refresh)*
```
{
    "not_before": "1442340812",
    "token_type": "Bearer",
    "access_token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik5HVEZ2ZEstZnl0aEV1Q...",
    "scope": "https://dnvglb2cprod.onmicrosoft.com/83054ebf-1d7b-43f5-82ad-b2bde84d7b75/user_impersonation offline_access",
    "expires_in": "3600",
    "refresh_token": "AAQfQmvuDy8WtUv-sd0TBwWVQs1rC-Lfxa_NDkLqpg50Cxp5Dxj0VPF1mx2Z...",
}
```

#### Implicit Grant Flow
[https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-reference-spa](https://docs.microsoft.com/en-us/azure/active-directory-b2c/active-directory-b2c-reference-spa)

Process:
1. redirect user to IdP (includes clientId, redirect url, nonce)
2. home realm discovery 
3. Authentication (present username/password)
4. Return access token to app
5. Use in Authorization header

Refresh:
1. Send hidden signin request to IdP
2. Authenticate with browser session (IdP)
3. Return access token to app

*Signin request:*
```
GET https://login.veracity.com/dnvglb2cprod.onmicrosoft.com/oauth2/v2.0/authorize?
client_id={yourClientId}
&response_type=id_token+token
&redirect_uri={yourRedirectUrl}
&response_mode=fragment
&scope=open_id%20offline_access%20https%3A%2F%2Fdnvglb2cprod.onmicrosoft.com%2F83054ebf-1d7b-43f5-82ad-b2bde84d7b75%2Fuser_impersonation
&state=arbitrary_data_you_can_receive_in_the_response
&nonce={nounce}
&p=B2C_1A_SignInWithADFSIdp
```
*Signin response:*
```
GET {yourRedirectUrl}/#
access_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik5HVEZ2ZEstZnl0aEV1Q...
&token_type=Bearer
&expires_in=3599
&scope="https://dnvglb2cprod.onmicrosoft.com/83054ebf-1d7b-43f5-82ad-b2bde84d7b75/user_impersonation offline_access",
&id_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik5HVEZ2ZEstZnl0aEV1Q...
&state=arbitrary_data_you_sent_earlier
```


### Native client (Dektop and mobile apps)

This flow is similar to authorization code flow but requires you to pop up an in-proc web browser to do the redirect. The redirect url also have some limitations.

*Native app login request:*
```
GET https://login.veracity.com/dnvglb2cprod.onmicrosoft.com/oauth2/v2.0/authorize?
client_id={yourClientId}
&response_type=code
&redirect_uri=urn%3Aietf%3Awg%3Aoauth%3A2.0%3Aoob
&response_mode=query
&scope=offline_access%20https%3A%2F%2Fdnvglb2cprod.onmicrosoft.com%2F83054ebf-1d7b-43f5-82ad-b2bde84d7b75%2Fuser_impersonation
&state=arbitrary_data_you_can_receive_in_the_response
&p=B2C_1A_SignInWithADFSIdp
```
*Login response*

```
GET urn:ietf:wg:oauth:2.0:oob?
code=AwABAAAAvPM1KaPlrEqdFSBzjqfTGBCmLdgfSTLEMPGYuNHSUYBrq...        
&state=arbitrary_data_you_can_receive_in_the_response                
```

### Server to server (Deamon)

Client credential flow is only supported through the Azure AD V1 endpoint, which means that you will receive a separate set of clientId, client secret, scope and resource url's. The token endpoint will also have a different url. We do not recommend this in normal scenarios, and is something we will discontinue when B2C supports an alternative using the same variables as the user initiated flows.


## Services Api (Api V3)

Veracity Services Api, lovingly called 'APIV3', gives access to the logged in users profile, subscription management and other user profile related features.
The api is divided into "view points", a view point is veracity seen from the actors point of view.

There is currently 3 view points:
1. My: the user's perspective
2. This: the service's perspective
3. Directory: this is reserves spescial cases and should not be used by most services. This view point will not be discussed here.

### Versioning policy

At the current time we only have 1 version that we will support after August 2019. In Api Management you will find the Services Api under the 'Veracity MyServices' product. There you will find 3 api's:

- https://api.veracity.com/Veracity/Services/V3: This is the most common endpoint to use. This will remain operational without breaking changes 1) after a V4 is released



*1 note that there might be breaking changes if there are major security reasons for it*


### View Points

#### My
This is veracity seen from the logged in user's perspective. It provides all the information needed to make the Veracity header section if you choose to make something that is following our design language.

*Key features (all GET)*
- /my/profile: contains the basic information about the logged in user
- /my/messages/count: the number of unread notifications
- /my/policies/{yourServiceId}/validate(): Validates that the platform and service terms are accepted

### This
Provides the features that the service needs to manage subscriptions, invite new users, send notifications and translate email into VeracityId

*Key Features*
- GET /this/services/{yourServiceId}/subscribers: get the list of subscribers to your service
- GET /this/services/{yourServiceId}/subscribers/{userId}: Check if a user has a subscription to your service
- PUT /this/services/{yourServiceId}/subscribers/{userId}: add a new subscription
- DELETE /this/services/{yourServiceId}/subscribers/{userId}: remove a user's subscription
- POST /this/user: Invite a new user to Veracity, and optionally your service
- POST /this/services/{serviceId}/notification: send a notification to a channel or specific list of users
- GET /this/user/resolve({email}): Check if the user is a Veracity user and get the user id

#### Scenario

We have a service for allowing Veracity users share pictures of horrible guitar repairs. The service manages it's own user, but like to put a tile on the users 'My Services' page. Also, we want to enable our users to invite others to share their horror stories, even people that doesn't have a Veracity account yet.

We apply service specific terms to have our users to agree to not share copyrighted, off-topic or any other illegal pictures

To make sharing easier and more engaging we tag the pictures and send notifications to the users that follows one or more of the tags.

**Api calls during signup**
1. create subscription: PUT /this/services/{yourServiceId}/subscribers/{userId}
2. Get redirect url to accept terms:  GET /my/policies/{yourServiceId}/validate()

**Api calls during signin**
1. Validate that all terms are accepted:  GET /my/policies/{yourServiceId}/validate() 

**Api calls from the header section**
1. Get the unread messages count: GET /my/messages/count
2. Get the full user profile: GET /my/profile

**Api calls when inviting users to the service**
1. Get the VeracityId for the user if he/she is already a veracity user
If not a veracity user
2. Invite and add subscription: POST /this/user
else
2. PUT /this/services/{yourServiceId}/subscribers/{userId}

**Api calls when a image is tagged**
1. send a notification to the list of user id's following one the tags: POST /this/services/{serviceId}/notification
