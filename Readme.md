# Serverless with Azure Functions

## Demo

### Add a new function

~~~C#
[FunctionName("ProcessFromQueue")]
public static void ProcessFromQueue(
    [ServiceBusTrigger("ProcessFile01", Connection = "ServiceBusConnection")]Message incomingMessage,
    ILogger log)
{
    log.LogInformation($"{incomingMessage.MessageId} received");
}
~~~

* Send a message with ServiceBus Explorer

~~~json
{
  "FileName": "xxx.txt"
}
~~~

The ServiceBus message is injected to the function.

### Deserialize the message

~~~C#
[FunctionName("ProcessFromQueue")]
public static void ProcessFromQueue(
    [ServiceBusTrigger("ProcessFile01", Connection = "ServiceBusConnection")]Message incomingMessage,
    ILogger log)
{
    var message = Newtonsoft.Json.JsonConvert.DeserializeObject<ProcessFileMessage>(Encoding.UTF8.GetString(incomingMessage.Body));
    log.LogInformation($"{message.FileName}");
}
~~~

### Read the file

* Inject the ExecutionContext to get access to the settings
* Read the file from blob container

~~~C#
[FunctionName("ProcessFromQueue")]
public static void ProcessFromQueue(
    [ServiceBusTrigger("ProcessFile01", Connection = "ServiceBusConnection")]Message incomingMessage,
    ExecutionContext context,
    ILogger log)
{
    var message = Newtonsoft.Json.JsonConvert.DeserializeObject<ProcessFileMessage>(Encoding.UTF8.GetString(incomingMessage.Body));
    var storageAccountConnectionString = context.GetConfig("StorageConnection");
    var fileContent = GetFileContent(storageAccountConnectionString, message.FileName);
    log.LogInformation($"Content of {message.FileName} is {fileContent}");
}
~~~

### Insert the content to a table

* Inject the Table Binding to get access to the table storage
* Insert the content

~~~C#
[FunctionName("ProcessFromQueue")]
public static void ProcessFromQueue(
    [ServiceBusTrigger("ProcessFile01", Connection = "ServiceBusConnection")]Message incomingMessage,
    [Table("State", Connection = "StorageConnection")] CloudTable stateTable,
    ExecutionContext context,
    ILogger log)
{
    var message = Newtonsoft.Json.JsonConvert.DeserializeObject<ProcessFileMessage>(Encoding.UTF8.GetString(incomingMessage.Body));
    var storageAccountConnectionString = context.GetConfig("StorageConnection");
    var fileContent = GetFileContent(storageAccountConnectionString, message.FileName);
    var result = stateTable.ExecuteAsync(TableOperation.Insert(new FileContentEntity(message.FileName, fileContent))).Result;
    log.LogInformation($"Content of {message.FileName} is {fileContent} and is added to state with result {result.HttpStatusCode}");
}
~~~

### Trigger from blob storage

* Add a new function with a BlobTrigger

~~~C#
[FunctionName("ProcessFromStorage")]
public static void ProcessFromStorage(
    [BlobTrigger("files/{fileName}", Connection = "StorageConnection")] Stream file,
    string fileName,
    ILogger log)
{
    var fileContent = GetFileContent(file);
    log.LogInformation($"Content of {fileName} is {fileContent}.");
}
~~~

The blob is injected as a stream

### Insert the content to a table

* Inject the Table Binding to get access to the table storage
* Insert the content

~~~C#
[FunctionName("ProcessFromStorage")]
public static void ProcessFromStorage(
    [BlobTrigger("files/{fileName}", Connection = "StorageConnection")] Stream file,
    [Table("State", Connection = "StorageConnection")] CloudTable stateTable,
    string fileName,
    ILogger log)
{
    var fileContent = GetFileContent(file);
    var result = stateTable.ExecuteAsync(TableOperation.Insert(new FileContentEntity(fileName, Encoding.UTF8.GetString(fileContent)))).Result;
    log.LogInformation($"Content of {fileName} is {fileContent} and is added to state with result {result.HttpStatusCode}");
}
~~~
