using System;
using WebSocketServer;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IHostedService>();

var host =  builder.Build();
host.Run();

