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

## Installing
TBD

##Configuration
TBD

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



