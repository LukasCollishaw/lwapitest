# lwapi

## Preface
This test was written with the intention of design and scalability being primary concern, and because of this, the code isn't written to a standard I'd generally allow in production, i.e. database I/O in controller code, no unit tests, etc. 

The services also have not had any security considerations, most notably, rate limiting. This is something that would definitley need to be implemented if this were to be moved to production.

The spec mentions that authentication is required for the internal parts of the application, although not required to implement. Given a middleware authentication service, Ambassador can easily be configured to check against this before routing traffic to the internal services.

## API Requirements and Assumptions
1. Generate ticket.
2. Check whether a ticket is valid.
3. Get ticket count (internal).
4. Invalidate a ticket (internal).

The spec calls for an api that is tolerant to huge amounts of traffic; working under this, we can make some assumptions on where most of the traffic will be going. There wasn't any context attached to this test, so these assumptions could be entirely wrong and are open to change. In addition to this, I'll also be focusing on high availabilty.

1. Generate ticket - Most used, millions/second @ peak.
2. Validate a ticket - Also heavily used, but not to the degree of the first (potentially spikey? depends what these tickets are being used for).
3. Invalidate a ticket - Significantly less usage, would probably not need to be scaled, again, depends on context.
4. Get ticket count - Presumably negligable usage in comparison to rest.

## Design Diagram / Scaling Considerations
![](https://github.com/LukasCollishaw/lwapitest/blob/master/design.PNG)

I wrote this solution as 3 individual microservices to be orchestrated with Kubernetes; this allows us to individually scale up/down each service depending on their needs.

Ultimately, the biggest bottleneck as far as I can see is the ticket generation/validation services and the write heavy database. To alleviate these, both the generation and validation services sit behind a load balancer and have Horizontal Pod Autoscaling, which will spin up or kill pods depending on CPU usage. Azure is also configured to autoscale the number of nodes depending on load.

From a database perspective, I've decided to go with Couchbase (which I have never used before, but looked fitting for the role). It is a distributed database that remedies the heavy I/O by spreading it across N servers. Adding or removing nodes in this cluster is relatively easy and inexpensive, so if demands require it, it can be scaled up and shouldn't face any issues.

I played around with some ideas of caching, but ultimately it seemed pretty superfluous since the context of what is being done wouldn't particularly benefit from it or would be very minimal. I.E. If many users share a ticket (for whatever reason), we could use a LFU cache to store the top 20?% most commonly validated ticket ids and values, to reduce load on the database.

The code used to gather the number of tickets issued is incredibly barebones, and not performant at all. This could be fixed, but due to my understanding of the context and usage, this shouldn't be an issue.
## API Documentation and Demo
This API is hosted on a Azure Kubernetes Service, which can be accessed at 'https://20.50.147.218'.

***

**PUT https://20.50.147.218/api/ticket/generate**

Response in format:

```
{
  id: "030600c0-0520-4124-aaa3-b37d4ed1c1bb"
}
```
***
**GET https://20.50.147.218/api/ticket/validate/{id}**

Response in format:

404 if ticket not found, otherwise
```
{
  valid: [true|false]
}
```
***
**POST https://20.50.147.218/api/ticket/invalidate/{id}**

Returns 200 OK on success.

***
**GET https://20.50.147.218/api/ticket/count**

Response in format: 
```
{
  count: 0
}
```
