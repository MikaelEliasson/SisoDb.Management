SisoDb.Management
=================

A SisoDb management tool for your asp.net websites. The tool is hosted on the website and will accessible from everywhere if the user is given permission.

SisoDb can be found at: http://www.sisodb.com or https://github.com/danielwertheim/sisodb-provider

## Features overview
- Querying for entities
- Loading entity by id
- Updating entity by modifying json
- Deleting entity by id
- Deleting enity by query
- Inserting new entity(ies) by json
- Regenerate indexes

## Where and how does it run?
SisoDb.Management in it's current form only runs in a asp.net website. It does this as a HttpHandler that is accessible att yoursite.com/siso-db-management/page.

Right now it's only tested in Firefox and Chrome. IE9 will probably work but the layout might be messed up. IE10 should be supported in a comming release.

If you want to see a sample of how it works there is a project called "TestApplication" in the solution. Set that as startup project and run it. It automatically creates some test data on app start. Navigate to localhost:someport/siso-db-management/page and start testing

## Installing

### The easy way : Use Nuget
TBD

### The harder way : Download the code, build it and add the reference manually

1) Download the code
2) Build SisoDb.Management in Release mode
3) Copy the .dll files found in bin/Release to your project
4) Reference what you need (You might already use a few of these). 

You will almost certainly have to add the following to web.config
```xml
<system.webServer>
    <handlers>
      <add name="SisoDbManagement" path="siso-db-management/*" verb="*" type="System.Web.Routing.UrlRoutingModule" resourceType="Unspecified" preCondition="integratedMode"/>
    </handlers>
</system.webServer>
```
##Configuration

You need to add some configuration to your applications startup. For example to Application_Start() in global.asax.cs.

You must set/call the following things:
- Configuration.DB: Your Siso database. Remember this should be long lived
- Configuration.Authorize: Func<string, bool> that gives you the name of the current action and should return if the user is allowed to do it. Typically you check if the user is Admin and then return true
- Configuration.Init() Initiates the routes for the HttpHandler. Might do more in the future so should be called

You should also add all the types you want to be accessible from the administration. Use
Configuration.AddTypeMapping<TContract, TImplementation>(); if you store the entity as an interface
or 
Configuration.AddTypeMapping<TImplementation>(); if you store the implementation.

Example of app start code:

```c#
Configuration.DB = YourSisoDatabase:ISisoDatabase;

Configuration.AddTypeMapping<IFeedbackItem, FeedbackItem>();
Configuration.AddTypeMapping<IComment, Comment>();

Configuration.Authorize = actionname => CheckIfUserHasAccess(actionname);

Configuration.Init();
```

##Start using it
Got to yoursite.com/siso-db-management/page

A new tab is created by clicking an entity to the left. The interface supports multiple tabs don't worry about opening new tabs while you are working in one tab. 

##Tips about querying
Mono is used to compile your Predicate and order by into c#. A query like q.Where(Your-Predicate).OrderBy(Your-Sort).Take(Your-PageSize) is created and executed. If you didn't define one of the options that part is skipped. Note: For Paging to be enabled you must set a Sort expression otherwise the paging is skipped.

A nice feature is that you can see which properties you can query on for the entity below the other fields.

###Setup code
Sometimes you want to compare against variables that has a complex setup in the query. You can define these variables in the Setup code. 

** Put example here **

###Sort
With the current version of SisoDb sorting on Guid and bool doesn't work well. Next version(v16) will have fixed this



