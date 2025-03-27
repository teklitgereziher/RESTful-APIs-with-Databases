# Azure Cosmos DB NoSql with .Net 9
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
