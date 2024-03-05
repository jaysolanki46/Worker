# Worker Project

## Table of Contents

- [Prerequisites](#prerequisites)
- [Clone the Repository](#clone-the-repository)
- [Set Up the Development Environment](#set-up-the-development-environment)
- [MongoDB Setup](#mongodb-setup)
- [Build and Run the Project](#build-and-run-the-project)
- [Running Tests](#running-tests)
- [Assumptions](#assumptions)
- [Output](#output)
- [Improvements](#improvements)

## Prerequisites

Before you begin, make sure you have the following installed on your machine:

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Visual Studio](https://visualstudio.microsoft.com/downloads/)
- [MongoDB](https://www.mongodb.com/try/download/community)

## Clone the Repository

```bash
git clone https://github.com/<your-username>/Worker.git
```

## Set Up the Development Environment

1. Open Visual Studio.
2. Navigate to File > Open > Project/Solution and select the .sln file in the cloned repository.

## MongoDB Setup

1. Open MongoDB compass.
2. Create a MongoDB database (`Worker`) and configure the connection string (`mongodb://localhost:27017`) in your project's appsettings file. Your sample appsettings.json would look something like this:

```
{
  "DatabaseConnectionSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "Worker"
  },
}
```

3. Under database (`Worker`), create two collections:
   - ParcelEvents : `ParcelEvents` collection records the last event made against a parcel and any pickup or delivery times.
   - ParcelLastEvent : `ParcelLastEvent` collection records the last event Id of API response. So that it can be used to continue where it left off if the service is stopped and started.

## Build and Run the Project

1. Once you open .sln file in Visual Studio, Right-click on the solution in the Solution Explorer.
2. Choose Restore NuGet Packages to restore the necessary packages.
3. If necessary, update your system environment in "launchSettings.json".
4. Build the solution by right-clicking the solution and selecting Build Solution.
5. You should now be able to run the project.

## Running Tests

The project includes a separate test project. To run tests:

1. Open the Test Explorer in Visual Studio.
2. Click Run All Tests

## Assumptions

- **Event Ordering**: I assume that the corresponding events returned by the scan event API are sorted by their EventId in ascending order. This assumption is necessary to accurately track the last event processed and pick up where the application left off.
- **Parcel Events**: The task specifies to save the last transaction of the parcel. I assume that each parcel can have multiple scan events and the application is designed to track the last event of each parcel.
- **Pickup/Delivery Recording**: The application is expected to record the pickup and delivery times according to the transaction types 'PICKUP' and 'Delivery'. This assumption is based on the understanding that these events directly represent the start of pickup and the completion of delivery.
- **Continuous Operation**: A worker application is assumed to be continuous because it is assumed to periodically fetch audit events, process them, and update the state of the application. The assumption is that it is not intended for one-time execution, but rather for continuous and repetitive processes.
- **Storage**: The database configuration is not specified; Thus, based on the assumptions made about the response data type and requirements, the NoSQL database MongoDB is expected to be well suited to handle JSON-type documents and adapt perfectly to the data structure of scan events.
- **Parcel Identifier**: The parcel is identified by the ParcelId field. For the purpose of this application, it is assumed that this field uniquely identifies each parcel, and the application utilizes it to associate events with specific parcels.
- **API Security**: I assume that the scan event API is publicly accessible without requiring any authorization or authentication mechanisms, such as M2M tokens.
- **API Response**: I assume that the `scanevents` endpoint is publicly available without authorization or authentication mechanisms such as M2M tokens.
- **API Functional**: Assuming the Scan Event API is always running.
- **Event Types**: As mentioned in the JSON example, event types should be limited to PICKUP, DELIVERED and STATUS. However, STATUS is not a valid event type. Therefore, it is expected that there may be multiple transaction types, but currently the implementation explicitly supports only 'PICKUP' and 'DELIVERY'. It may not handle other events separately, if any.
- **Logging**:I assumed that an application should have sections that serve not only informational purpose but also for easy debugging and troubleshooting. Additionally, I find it acceptable to use Serilog to manage both console and file logging.
- **CreatedDateTimeUtc**: The API response `CreatedDateTimeUtc` interprets the time when the event was created. This interpretation applies regardless of whether the transaction type is 'PICKUP' or not.
- **Required fields**: `EventId, ParcelId, Type, CreatedDateTimeUtc, StatusCode, RunId` are the only required fields that need to be persisted.
- **DateTimes**: Assuming that the request (2.b DateTimes indicator) is in the context of the `CreatedDateTimeUtc` JSON response, I added two new fields to the collection. CreatedDateTimeUtc is stored in the PickedUpDateTimeUtc field if the transaction type is 'PICKUP', and `CreatedDateTimeUtc` is stored in the `DeliveredDateTimeUtc` field if the transaction type is 'DELIVERY'. This layout makes it easy to search and view a single line of a parcel's event route.
- **Default Limit**: The default limit for scan events returns 100. However, I figured we could change this limit by specifying a URL parameter.

## Output

Assumed the following JSON response would be received from Scan Event API (eg. GET http://localhost/v1/scans/scanevents):

```json
{
  "ScanEvents": [
    {
      "EventId": 83269,
      "ParcelId": 5002,
      "Type": "PICKUP",
      "CreatedDateTimeUtc": "2021-05-11T21:11:34.1506147Z",
      "StatusCode": "",
      "Device": {
        "DeviceTransactionId": 83269,
        "DeviceId": 103
      },
      "User": {
        "UserId": "NC1001",
        "CarrierId": "NC",
        "RunId": "100"
      }
    },
    {
      "EventId": 83270,
      "ParcelId": 5003,
      "Type": "PICKUP",
      "CreatedDateTimeUtc": "2022-06-11T21:11:34.1506147Z",
      "StatusCode": "",
      "Device": {
        "DeviceTransactionId": 83270,
        "DeviceId": 104
      },
      "User": {
        "UserId": "PH1001",
        "CarrierId": "PH",
        "RunId": "101"
      }
    },
    {
      "EventId": 83271,
      "ParcelId": 5002,
      "Type": "DELIVERY",
      "CreatedDateTimeUtc": "2021-06-11T21:11:34.1506147Z",
      "StatusCode": "",
      "Device": {
        "DeviceTransactionId": 83271,
        "DeviceId": 103
      },
      "User": {
        "UserId": "CP1001",
        "CarrierId": "CP",
        "RunId": "103"
      }
    },
    {
      "EventId": 83272,
      "ParcelId": 5003,
      "Type": "IN_CUSTOM",
      "CreatedDateTimeUtc": "2022-07-11T21:11:34.1506147Z",
      "StatusCode": "",
      "Device": {
        "DeviceTransactionId": 83272,
        "DeviceId": 104
      },
      "User": {
        "UserId": "PH1001",
        "CarrierId": "PH",
        "RunId": "101"
      }
    },
    {
      "EventId": 83273,
      "ParcelId": 5004,
      "Type": "DELIVERY",
      "CreatedDateTimeUtc": "2023-07-11T21:11:34.1506147Z",
      "StatusCode": "",
      "Device": {
        "DeviceTransactionId": 83273,
        "DeviceId": 106
      },
      "User": {
        "UserId": "PH1001",
        "CarrierId": "PH",
        "RunId": "108"
      }
    }
  ]
}
```

**MongoDB colletion samples**:

| ![ParcelEvents](/ReadMeImages/ParcelEvents.png) | ![ParcelLastEvent](/ReadMeImages/ParcelLastEvent.png) |

**Logs sample**:

![Worker logs](/ReadMeImages/Logs.png)

## Improvements

- **Application Performance Optimization**: To reduce the load on external services and improve response times, enable the caching mechanism. Additionally, identify bottlenecks where heavy loads occur. Caching can be done both in the database and in the application itself.
- **API Documentation**: In the document, define API endpoints, request and response structures, and authentication mechanisms for external consumers.
- **Unit Testing and Integration Testing**: A sample unit test was written for `ScanEventWorker` function. Additional unit tests would cover individual components and functions to ensure they work as expected. In addition, integration testing is required to verify the interaction of different components.
- **Secure communication**: Develop secure communication methods, such as M2M, to manage external information and resources.
- **CI/CD Pipeline**: When you set up a CI/CD pipeline in Azure DevOps, the job is automatically built, tested, and published by tools like Jenkins when merging your PRs. Additionally, we will add a docker to build images for the application.
- **AWS - EKS**: Deploy on Amazon EKS for optimal container deployment.
- **AWS - Lamda**: Run the `Worker` program using Lambda. This serverless computing service eliminates the need to manage infrastructure and resolves and requests automatically.
- **AWS - DynamoDB**: Since we are using MongoDB for local development, AWS DynamoDB may be better for running our highly managed NoSQL database. It can be easily integrated with AWS services like Lambda, because it can help with scalability, performance, flexibility, security and seamless installation.
- **AWS - Parameter Store**: The AWS Systems Manager parameter store is useful for storing and managing configuration settings, such as the appettings.json file for `Worker` application. It is mainly used for sensitive information such as API keys and database connection strings. It is encrypted, providing another layer of security.
- **Elasticsearch for Logging**: Elasticsearch is most often used for logging. Additionally, configure monitoring and alerts so that Elasticsearch detects issues such as low disk space, high error rates, or performance issues.

### High-level architecture of this overall system to enable another worker application downstream

To enabled downstream processing of parcel scan events by another worker application, introduce a message broker service like RabbitMQ.

**1. Processing Application**:
Process scan events and store them in the database.
Publish processed events to RabbitMQ.

**2. RabbitMQ**:
A message broker manages communication.
Directs processed messages to downstream workers.

**3. Downstream Worker Application**:
Subscribe to RabbitMQ to get processed events.
Perform additional operations on received events.

**4. Database**:
Shared storage location for processed events.

**5. Monitoring and Logging**:
Use Elastic search to monitor and log events.

**6. Deployment and Orchestration**:
Docker and Kubernetes for scalability.
Define scaling policies to manage number of resources based on incoming traffic.

This RabbitMQ-based architecture provides decoupled processing, allowing downstream workers to act on the same scan events without direct integration.
