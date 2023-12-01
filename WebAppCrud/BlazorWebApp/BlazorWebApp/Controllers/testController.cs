using BlazorWebApp.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace BlazorWebApp.Controllers
{
    /// <summary>
    /// freesql API
    /// </summary>
    [ApiController]
    [Route("[controller]")] 
    public class testController : ControllerBase
    {

        IFreeSql _fsql { get; set; }
        private readonly ILogger<testController> _logger;

        public testController(ILogger<testController> logger, IFreeSql fsql)
        {
            _logger = logger;
            _fsql = fsql;
        }

        /// <summary>
        /// 测试api工作是否正常
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        public string Test() => "OK";


        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var ItemList = _fsql.Select<WeatherForecast>().OrderByDescending(a=>a.ID).ToList();

            return ItemList;
        } 

        /// <summary>
        /// put或者GET的put方法插入一条数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("Put")]
        [HttpPut]
        public IEnumerable<WeatherForecast> Put(string name)
        {
            var id = _fsql.Insert<WeatherForecast>().AppendData(new WeatherForecast() { Summary = name}).ExecuteIdentity();
            var ItemList = _fsql.Select<WeatherForecast>().Where (a=>a.ID == id).ToList();
            return ItemList;
        }

        /// <summary>
        /// delete或者GET的Del方法删除一条数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("Del")]
        [HttpDelete]
        public string Del(int Id)
        {
            var rows = _fsql.Delete<WeatherForecast>().Where(a=>a.ID  == Id).ExecuteAffrows();
            return $"删除{rows}行";
        }

        [HttpGet("Reset")]
        [HttpDelete("Reset")]
        public string Reset(bool flag)
        {
            if (flag)
            {
                var rows = _fsql.Delete<WeatherForecast>().Where(a => a.ID > 0).ExecuteAffrows(); 
                return $"删除{rows}行";
            }
            else
            {
                return "删除失败";
            } 
        }


        /// <summary>
        /// Patch或者GET的Modify方法编辑一条数据
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("Modify")]
        [HttpPatch]
        public string Modify(int Id,string name)
        {
            var rows = _fsql.Update<WeatherForecast>().Set(a=>a.Summary == name).Where(a=>a.ID == Id).ExecuteAffrows();
            return $"编辑{rows}行";
        }

    }
}
