// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace BlazorWebApp.Services;

/// <summary>
/// API 数据 的 IDataService 数据注入服务接口实现
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class ApiDataService<TModel> : DataServiceBase<TModel>
        where TModel : class, new()
{
    public List<TModel> Items { get; set; } = [];
    public IEnumerable<int> PageItemsSource => new int[] { 10, 20, 100, 500 };
    public IEnumerable<int> PageItemsSource50 => new int[] { 50, 100, 200, 500 };

    [NotNull]
    private HttpClient? Http { get; set; }

    /// <summary>
    /// 设置主表主键,用于保存临时数据
    /// </summary>
    [NotNull]
    public string? Field { get; set; }

    public ApiDataService(HttpClient? Http)
    {
        this.Http = Http;
        if (this.Http.BaseAddress == null)
            this.Http.BaseAddress = new Uri("https://localhost:7241/");

    }

    /// <summary>
    /// 保存方法
    /// </summary>
    /// <param name="model"></param>
    /// <param name="changedType"></param>
    /// <returns></returns>
    public override async Task<bool> SaveAsync(TModel model, ItemChangedType changedType)
    {
        // 增加数据演示代码

        if (Items.Where(a => a.GetValue("ID") == model.GetValue("ID")).Any())
        {
            var res = await Http.PatchAsync($"test?Id={model.GetValue("ID")}&name={model.GetValue("Summary")}", null);
        }
        else
        {
            //Items.Add(model);
            var res = await Http.PutAsync($"test?name={model.GetValue("Summary")}", null);

        }
        return true;
    }

    public override async Task<bool> DeleteAsync(IEnumerable<TModel> items)
    {
        items.ToList().ForEach(async i =>
        {
            //Items.Remove(i);
            await Http.DeleteAsync($"test?Id={i.GetValue("ID")}");
        });
        return true;
    }

    private static readonly ConcurrentDictionary<Type, Func<IEnumerable<TModel>, string, SortOrder, IEnumerable<TModel>>> SortLambdaCache = new();
    private int 计数 = 0;
    public override async Task<QueryData<TModel>> QueryAsync(QueryPageOptions options)
    {
        System.Console.WriteLine($"LazyHero QueryAsync {计数} 数据:{Items?.Count}"); 计数++;
        Items = (await Http.GetFromJsonAsync<TModel[]>("test"))?.ToList();
        IEnumerable<TModel> items = Items;

        // 处理 Searchable=true 列与 SeachText 模糊搜索
        if (options.Searches.Any())
        {
            //类加入 [AutoGenerateClass(Filterable = true, Sortable = true, TextWrap = true, Searchable = true)] 就能搜索了

            items = items.Where(options.Searches.GetFilterFunc<TModel>(FilterLogic.Or));
        }
        else
        {
            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrEmpty(options.SearchText))
            {
                //TODO : LazyHero SearchText 模糊搜索 未实现
                // items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false)|| (item.Address?.Contains(options.SearchText) ?? false));
            }
        }


        // 过滤
        var isFiltered = false;
        if (options.Filters.Any())
        {
            items = items.Where(options.Filters.GetFilterFunc<TModel>());

            // 通知内部已经过滤数据了
            isFiltered = true;
        }

        // 排序
        var isSorted = false;
        if (!string.IsNullOrEmpty(options.SortName))
        {
            var invoker = SortLambdaCache.GetOrAdd(typeof(TModel), key => LambdaExtensions.GetSortLambda<TModel>().Compile());
            items = invoker(items, options.SortName, options.SortOrder);
            isSorted = true;
        }

        // 设置记录总数
        var total = items.Count();

        // 内存分页
        if (options.IsPage)
        {
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems);
        }
        else if (options.IsVirtualScroll)
        {
            items = items.Skip(options.StartIndex).Take(options.PageItems);
        }

        return new QueryData<TModel>()
        {
            Items = items,
            TotalCount = total,
            IsSorted = isSorted,
            IsFiltered = isFiltered,
            IsSearch = true
        };

    }

}

public class LazyTools
{

    /// <summary>
    /// 泛型 Copy 方法
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public static void Copy<TModel>(TModel source, TModel destination) where TModel : class
    {
        if (source != null && destination != null)
        {
            var type = source.GetType();
            //if (type.IsClass)
            //{
            var valType = destination.GetType();
            if (valType != null)
            {
                type.GetFields().ToList().ForEach(f =>
                {
                    var v = f.GetValue(source);
                    valType.GetField(f.Name)?.SetValue(destination, v);
                });
                type.GetProperties().ToList().ForEach(p =>
                {
                    if (p.CanWrite)
                    {
                        var v = p.GetValue(source);
                        valType.GetProperty(p.Name)?.SetValue(destination, v);
                    }
                });
            }
            //}
        }
    }

}

/// <summary>
/// Object 扩展方法
/// </summary>
internal static partial class ObjectsExtensions
{
    /// <summary>
    /// 获取属性
    /// </summary>
    /// <param name="instance">object</param>
    /// <param name="propertyName">需要判断的属性</param>
    /// <returns>是否包含</returns>
    public static object GetField<TItem>(this TItem instance, string propertyName)
    {

        if (instance != null && !string.IsNullOrEmpty(propertyName))
        {
            var propertyInfo = instance.GetType().GetProperty(propertyName);
            return propertyInfo;
        }
        return null;
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="instance">object</param>
    /// <param name="propertyName">需要判断的属性</param>
    /// <returns>是否包含</returns>
    public static object GetValue<TItem>(this TItem instance, string propertyName)
    {

        if (instance != null && !string.IsNullOrEmpty(propertyName))
        {
            var propertyInfo = instance.GetType().GetProperty(propertyName);
            return propertyInfo.GetValue(instance);
        }
        return null;
    }


}
