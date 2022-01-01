using IPclient;
using IPclient.Extensions;

//using IPclient.Shared.IServices;

//set .json environment
//Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Live");

var host = HostBuilderExtensions.CreateHostBuilder(args).Build();
await host.Instance<IApp>().Run();

Console.WriteLine($"Done.");