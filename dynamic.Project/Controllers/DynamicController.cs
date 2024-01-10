using dynamic.Project.Base;
using dynamic.Project.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace dynamic.Project.Controllers
{
    [Route("api/{entity}")]
    [ApiController]
    public class DynamicController : ControllerBase
    {
        public Type _dbAccessType;
        public Type DbAccessType
        {
            get
            {
                if(_dbAccessType == null)
                {
                    _dbAccessType = typeof(DbAccess<>);
                }
                return _dbAccessType;
            }
        }

        //通过Entity寻找实体
        public static Type FindEntity(string entity)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies();
            var types = assembly.SelectMany(a => a.GetTypes());
            foreach (var type in types)
            {
                if (type.Name.ToLower() == entity.ToLower())
                {
                    return type;
                }
            }
            return null;
        }

        public static object GetAccess(Type accType)
        {
            
            var acc = IocContainer.Instance.GetOrDefault(accType);
            if(acc == null)
            {
                var accobj = Activator.CreateInstance(accType);
                IocContainer.Instance.Register(accobj);
                acc = IocContainer.Instance.GetOrDefault(accType);
            }
           if (acc == null) throw new Exception($"数据访问器创建失败{accType.FullName}");
            return acc;
        }

        public static ApiResult Handle(
            Type accessType,
            string entity,
            Dictionary<string, string>? execParams,
            [CallerMemberName]string methodName = "")
        {
            if(string.IsNullOrEmpty(methodName)) 
                throw new Exception("调用方法名为空，请检查");

            //获取实体类型
            Type type = FindEntity(entity);
            if (type == null) throw new Exception($"未找到实体<{entity}>");

            //获取数据访问器
            var accType = accessType.MakeGenericType(type);
            var acc = GetAccess(accType);

            //处理参数
            JObject selParams = new JObject();
            if (execParams != null && execParams.Count > 0)
            {
                foreach (var key in execParams.Keys)
                {
                    selParams.Add(key, execParams[key]);
                }
            }
            //执行方法
            var method = accType.GetMethod(methodName);
            dynamic res = method.Invoke(acc, new object[] { selParams });
            try
            {
                res.Wait();
                return ApiResult.Success(data: res.Result);
            }catch(Exception ex)
            {
                return ApiResult.Success(res);
            }
        }

        [HttpGet]
        public ActionResult<ApiResult> Sel(
            [FromRoute] string entity
            ,[FromQuery] Dictionary<string, string>? queryParams
            ) => Handle(DbAccessType, entity,queryParams);
            
        [HttpPost]
        public ActionResult<ApiResult> Add(
            [FromRoute] string entity
            , [FromBody] Dictionary<string, string>? queryParams
            ) => Handle(DbAccessType, entity, queryParams);

        [HttpPut]
        public ActionResult<ApiResult> Update(
            [FromRoute] string entity
            , [FromBody] Dictionary<string, string>? queryParams
            ) => Handle(DbAccessType, entity, queryParams);

        [HttpDelete]
        public ActionResult<ApiResult> Delete(
            [FromRoute] string entity
            , [FromQuery] Dictionary<string, string>? queryParams
            ) => Handle(DbAccessType, entity, queryParams);
    }
}
