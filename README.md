[![CI Pipeline](https://github.com/teklitgereziher/RESTful-APIs-with-Databases/actions/workflows/ci.yml/badge.svg)](https://github.com/teklitgereziher/RESTful-APIs-with-Databases/actions/workflows/ci.yml)

# RESTful-APIs-with-Databases
This repo contains practices on how to use different databases with ASP.NET Core Web API and Docker.

## Azure Cosmos DB NoSql with .Net 9
Use Swashbuckle SwaggerUI to show the API documentation in a .Net 9 Web API project.
https://localhost:7144/swagger/index.html

### Connectivity modes
The .NET and Java SDKs can connect to the service in:
- `Gateway mode`: it uses the HTTP protocol
- `Direct mode`: it uses the TCP protocol

### Client instances and connections
Regardless of the connectivity mode, maintain a Singleton instance of the SDK client per account per application.
Connections, both HTTP, and TCP, are scoped to the client instance. Most compute environments have limitations in terms of the number of connections that can be open at the same time.
When these limits are reached, connectivity is affected.
Transient errors are errors that might soon resolve themselves. A `429` status code indicates the backend can't handle your request at that moment the request is totally valid.

### Set Up the Query
- FeedIterator<T>: This iterator retrieves the query results in batches, allowing efficient data retrieval.
- FeedResponse<T>: Represents the results from a query, and HasMoreResults checks if there are more results to fetch.
- The SQL query API is case-sensitive, so it matches the exact casing of field names when querying.


## EF Core Notes
### Inheritance Mapping
EF Core does not automatically scan for base or derived types by convention.
If you want a CLR type in your hierarchy to be mapped, you must explicitly specify that type on your model. 
For example, specifying only the base type of a hierarchy will not cause EF Core to implicitly include all of its sub-types.

In EF Core, there are three inheritance mapping strategies:
- Table-per-Hierarchy (TPH): All types in the hierarchy are stored in a single table,  and a discriminator column is used to identify the type of each row. This is the default strategy.
- Table-per-Type (TPT): Each type in the hierarchy is stored in its own table. The base type's table contains the common properties, and each derived type's table contains the properties specific to that type.
- Table-per-Concrete Type (TPC): Each concrete type in the hierarchy is stored in its own table and there is not database inheritance relationship. The base type's table is not used, and each derived type's table contains all properties, including those from the base type. 

TPC and TPH inheritance patterns generally deliver better performance than TPT inheritance patterns, because TPT patterns can result in complex join queries.
