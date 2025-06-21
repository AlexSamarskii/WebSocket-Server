using System;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<IHostedService>();

builder.Services.AddSingleton<IHostedService>();

var host =  builder.Build();
host.Run();

