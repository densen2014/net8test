// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using FreeSql.DataAnnotations;
using System.ComponentModel;

namespace BlazorWebApp.Services;

/// <summary>
/// 
/// </summary>
public class WeatherForecastService
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startDate"></param>
    /// <returns></returns>
    public async Task<WeatherForecast[]> GetForecastAsync()
    {
        await Task.Delay(500);

        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToArray();

        return forecasts;
    }


}

[AutoGenerateClass(Searchable = true, Filterable = true, Sortable = true)] //BB自动建列特性,可搜索,可过滤,可排序
public class WeatherForecast
{
    [Column(IsIdentity = true)] //FreeSql ORM特性: 自增,第一个名称ID的自动为主键
    [DisplayName("序号")] //BB使用DisplayName渲染列名
    public int ID { get; set; }

    [DisplayName("日期")]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public static List<WeatherForecast> GenDatas()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToArray();

        return forecasts.ToList();

    }
}