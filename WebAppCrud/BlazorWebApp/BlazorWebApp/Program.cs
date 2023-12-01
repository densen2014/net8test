using BlazorWebApp.Client.Pages;
using BlazorWebApp.Components;
using BlazorWebApp.Services;
using Densen.DataAcces.FreeSql;
using Microsoft.OpenApi.Models;

IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.Sqlite, "Data Source=test.db")
    .UseAutoSyncStructure(true) //自动同步实体结构【开发环境必备】
    .UseMonitorCommand(cmd => System.Console.Write(cmd.CommandText))
    .Build();


#region 初始化demo数据

var ItemList = WeatherForecast.GenDatas();

if (fsql.Select<WeatherForecast>().Count() == 0)
{
    fsql.Insert<WeatherForecast>().AppendData(ItemList).ExecuteAffrows();
}
ItemList = fsql.Select<WeatherForecast>().ToList();

System.Console.WriteLine("ItemList: " + ItemList.Count());

#endregion 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// 增加 BootstrapBlazor 组件
builder.Services.AddBootstrapBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddFreeSql(option =>
{
    //demo演示的是Sqlite驱动,FreeSql支持多种数据库，MySql/SqlServer/PostgreSQL/Oracle/Sqlite/Firebird/达梦/神通/人大金仓/翰高/华为GaussDB/MsAccess
    option.UseConnectionString(FreeSql.DataType.Sqlite, "Data Source=test.db;")  //也可以写到配置文件中
#if DEBUG
         //开发环境:自动同步实体
         .UseAutoSyncStructure(true)
         .UseNoneCommandParameter(true)
         //调试sql语句输出
         .UseMonitorCommand(cmd => System.Console.WriteLine(cmd.CommandText))
#endif
    ;
});
builder.Services.AddScoped(typeof(FreeSqlDataService<>));

builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
       .AddInteractiveServerComponents()
       .AddInteractiveWebAssemblyComponents();

builder.Services.AddSwaggerGen(c =>
{
    c.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, "BlazorWebApp.xml"), true);
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi", Version = "v1", Description = "工程源码 https://github.com/densen2014/FreeSqlDemos" });
});


builder.Services.AddHttpClient();
builder.Services.AddSingleton<IHostedService, HttpClientSetupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreesSQL Demo WebApi v1");
    c.RoutePrefix = "api";
});

app.MapDefaultControllerRoute();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebApp.Client._Imports).Assembly);


app.Run();
