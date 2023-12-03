// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorWebApp.Services;
using Densen.DataAcces.FreeSql;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

//SQLitePCL.Batteries.Init();
//SQLitePCL.Batteries_V2.Init();
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 增加 BootstrapBlazor 服务
builder.Services.AddBootstrapBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
//builder.Services.AddFreeSql(option =>
//{
//    //demo演示的是Sqlite驱动,FreeSql支持多种数据库，MySql/SqlServer/PostgreSQL/Oracle/Sqlite/Firebird/达梦/神通/人大金仓/翰高/华为GaussDB/MsAccess
//    option.UseConnectionString(FreeSql.DataType.Sqlite, "Data Source=wasm.db;")  //也可以写到配置文件中
//#if DEBUG
//         //开发环境:自动同步实体
//         .UseAutoSyncStructure(true)
//         .UseNoneCommandParameter(true)
//         //调试sql语句输出
//         .UseMonitorCommand(cmd => System.Console.WriteLine(cmd.CommandText))
//#endif
//    ;
//});
//builder.Services.AddScoped(typeof(FreeSqlDataService<>));
//Table附加内存数据操作服务
builder.Services.AddTransient(typeof(ApiDataService<>));

builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    });

await builder.Build().RunAsync();
