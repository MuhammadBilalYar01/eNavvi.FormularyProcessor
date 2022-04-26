#region Usings
using eNavvi.FormularyProcessor.Data;
using eNavvi.FormularyProcessor.Interfaces;
using eNavvi.FormularyProcessor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
#endregion

#region Setup
IConfiguration Configuration = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json")
                       .Build();

IServiceProvider Startup()
{
    string path = $"{System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "")}\\Logs\\log.json";

    string SeqServerUrl = Configuration["SeqServerUrl"];
    if (null == SeqServerUrl)
        SeqServerUrl = "http://localhost:5341/";

    string SeqServerKey = Configuration["SeqServerKey"];
    var logger = new LoggerConfiguration()
        .MinimumLevel.Override("Host.Startup", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Azure.WebJobs.Hosting.OptionsLoggingService", LogEventLevel.Warning)
        .MinimumLevel.Override("Host.Results", LogEventLevel.Warning)
        .MinimumLevel.Override("Host.LanguageWorkerConfig", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.StatusCodeResult", LogEventLevel.Warning)
        .MinimumLevel.Override("DurableTask.AzureStorage", LogEventLevel.Warning)
        .MinimumLevel.Override("DurableTask.Core", LogEventLevel.Warning)
        .MinimumLevel.Override("Host.Triggers.DurableTask", LogEventLevel.Warning)
        .MinimumLevel.Override("Host.Aggregator", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Model.Validation", LogEventLevel.Error)
        .WriteTo.Console()
        .WriteTo.File(path)
        .WriteTo.Seq(SeqServerUrl, apiKey: SeqServerKey)
        .CreateLogger();

    Log.Logger = logger;

    //setup our DI
    var Services = new ServiceCollection()
    .AddLogging(lb => lb.AddSerilog(logger))
    .AddTransient<IBlobStorage, BlobStorageService>()
    .AddTransient<ITableStorage, TableStorageService>()
    .AddTransient<IRxNormUtility, RxNormUtility>()
    .AddSingleton(_ => Configuration)
    .AddDbContext<eNavviContext>(options => options.UseSqlServer(Configuration["SQLConnection"]))
    .BuildServiceProvider();


    return Services;
}
#endregion


IServiceProvider serviceProvider = Startup();

/*
              1: Process new or updated plan
              2: Insert or update Medicare Plan
              3: Process Medicare Plans
             */
if (Configuration["ProcessingWorkflow"] == "1")
{
    FormularyProcessing formulary = new FormularyProcessing(
        serviceProvider.GetService<IBlobStorage>(),
        serviceProvider.GetService<ITableStorage>(),
         Configuration,
         serviceProvider.GetService<IRxNormUtility>());

    await formulary.Run();
}
else
{
    Console.WriteLine("Un Match workflow.");
}