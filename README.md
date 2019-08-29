# GrpcNetProxy

A proxy to support simple GRPC communication between micro services in .NET core.


## Projects organization

There are three projects and all are included in a test solution `GrpcNetProxyTest` inside the test folder. The projects are:
* **GrpcNetProxy**. It is the main project, which implements the GRPC client and server proxy.
* **GrpcNetProxyTest**. It is a unit test project.
* **GrpcNetProxyTestBenchmark**. It is a command line application to run a benchmark test on demo service.
* **GrpcNetProxyTestApp**. Test application which illustrates the usage of configuration files.


## Services interfaces

To use this library to host and connect to GRPC services, interfaces have to be defined for each service. The interfaces requirements are:
* Methods return types has to be Task (with or without return argument).
* Methods have to be defined with two arguments.
** First argument is the method request argument. It is recommended to be an object with properties.
** Second argument is the CancellationToken. It is propagated from client to server and it is used to cancel the request from client.

The code below shows a service interface example with one method.

~~~cs
public interface ITestService
{
	Task<TestResponse> TestMethodSuccess(TestRequest request, CancellationToken token = default);	
}
~~~


## Server 

To run the GRPC server interfaces implementations are required. The code below shows a dummy example implementation on the `ITestService`.

~~~cs
public class ServerTestService : ITestService
{
	public Task<TestResponse> TestMethodSuccess(TestRequest request, CancellationToken token = default)
	{
		return Task.FromResult(new TestResponse { Id = request.Id });
	}
}
~~~

A GRPC server can be  initialized by calling the extension method `AddGrpcServer` on existing `IServiceCollection` as shown on the example code below. Do not forget to register services implementation. Not that the example code below shows only basic configuration options. Furthermore, GRPC server can be registered also as a hosted service. To do so the extension method `AddGrpcHostedService` can be used on the `IServiceCollection`.

~~~cs
collection.AddGrpcServer(cfg =>
{
	// add test service
	// todo: add other services
	cfg.AddService<ITestService>();

	// enable status probe service (used to check server connectivity from clients)
	cfg.AddStatusService();

	// set grpc host binding 
	cfg.SetConnection(new GrpcServerConnectionData { Port = 5000, Url = "127.0.0.1" });

	// set name (in case of multiple servers).
	cfg.SetName("TestGrpcServer");
});

// register services implementation
services.AddScoped<ITestService, ServerTestService>();
~~~


## Client

A client can be connected to one or more GRPC servers instances. Round-robin like algorithm is used to route requests. Furthermore, multiple GPRC servers with different services are also supported and have to be distinguished by different names. The code below shows and example of a GRPC client configuration over existing `IServiceCollection` object by calling the extension method `AddGrpcClient`.

~~~cs
services.AddGrpcClient(cfg => {

	// enable status service 
	cfg.EnableStatusService();

	// set name 
	cfg.SetName("TestGrpcServer");

	// add services (there is no need to register)
	cfg.AddService<ITestService>();

	// add channels to connect to
	// multiple channels (hosts) can be added (round-robin)
	cfg.AddHost(new GrpcChannelConnectionData
	{
		Port = 5000,
		Url = "127.0.0.1"
	});

	// set options 
	cfg.SetClientOptions(new GrpcClientOptions { TimeoutMs = 10000 });

});
~~~


### Clients management

For each client (connected to one or multiple servers), distinguished by name, a client manager is registered to services collection. Client manager is responsible to execute repeated probe request to all available service. From user prospective can be used to manage clients (get status, reset connections, etc.). The code below shows an example on how to reset all channels by using the client manager.

~~~cs
services.GetServices<GrpcClientManager>().First(m => m.Name == "TestGrpcServer").ResetChannels();
~~~


## Configuration files

Grpc server and client can be easily initialized from configuration file. A single configuration file can contain multiple clients and/or servers configurations. However, services registration needs to be configured manually in code. For a full example refer to the project `GrpcNetProxyTestApp`.

The following code snippets shows a JSON client  configuration and a JSON server configuration. Note that both client and server can be combined to a single file if used in the same application.

~~~json
{
  "Clients": [
    {
      "Name": "Default",
      "MonitorInterval": 60000,
      "Hosts": [
        {
          "Url": "127.0.0.1",
          "Port": 5001,
          "ErrorThreshold": 5
        }
      ],
      "EnableStatusService": false,
      "Options": {
        "LogRequests": false,
        "ContextKey": "X-ContextId",
        "TimeoutMs": 10000
      }
    }
  ]
}

~~~json
{
  "Servers": [
    {
      "Name": "Default",
      "EnableStatusService": false,
      "Options": {
        "LogRequests": false,
        "ContextKey": "X-ContextId"
      },
      "Host": {
        "Url": "127.0.0.1",
        "Port": 5001
      }
    }
  ]
}

The following code show a basic grpc server and client setup.

~~~cs
// create server
var host = new HostBuilder().ConfigureServices((hostContext, services) =>
{

	// register services
	services.AddScoped<ITestService, ServerTestService>();

	// configure grpc server
	var srvCfgFilePath = Path.Combine(Directory.GetCurrentDirectory(), "grpcServerOnly.json");
	services.ConfigureGrpc(srvCfgFilePath, (cfg) =>
	{
		cfg.Server().AddService<ITestService>();
	});

}).Build();

// create client
var clientCfgFilePath = Path.Combine(Directory.GetCurrentDirectory(), "grpcClientOnly.json");
var clientProvider = new ServiceCollection()
	.ConfigureGrpc(clientCfgFilePath, (cfg) => cfg.Client().AddService<ITestService>())
	.BuildServiceProvider();
~~~
