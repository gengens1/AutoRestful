using dynamic.Project.Base;
using dynamic.Project.Base.Mate;
using dynamic.Project.Entity.Plugins;
using dynamic.Project.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace dynamic.Project.Db
{
    /// <summary>
    /// 数据访问器
    /// 通过该访问器去访问实体
    /// 可以执行实体和插件的生命周期方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbAccess<T> where T : EntityBase
    {
        private static ZengDb _db;
        public static ZengDb Db
        {
            get
            {
                if (_db == null)
                {
                    _db = IocContainer.Instance.GetOrDefault<ZengDb>();
                    if(_db == null)
                    {
                        IocContainer.Instance.Register<ZengDb>();
                        _db = IocContainer.Instance.Get<ZengDb>();
                    }
                }
                return _db;
            }
        }

        public List<EntityBasePlugin<T>> _plugins;
        public List<EntityBasePlugin<T>> Plugins
        {
            get
            {
                if (_plugins == default)
                {
                    var ts = MateData.Assems.SelectMany(a => a.GetTypes()).Where(t => t.IsSubclassOf(typeof(EntityBasePlugin<T>)));
                    _plugins = ts.Select(t => (EntityBasePlugin<T>)Activator.CreateInstance(t)).ToList();
                }
                return _plugins;
            }
        }

        /// <summary>
        /// 直接操作Set 无法启用实体插件
        /// </summary>
        public DbSet<T> _models;
        public DbSet<T> Models
        {
            get
            {
                if(_models == null)
                {
                    _models = Db.Set<T>();
                }
                return _models;
            }
            set => _models = value;
        }

        public Type type = typeof(T);

        public async Task<string> Add(JObject data)
        {
            T t = JsonConvert.DeserializeObject<T>(data.ToString());
            await Models.AddAsync(t);
            Db.SaveChanges();
            return type.Name + "添加成功";
        }

        public string Update(JObject data)
        {
            var Id = data.Value<string>("Id");
            if (string.IsNullOrWhiteSpace(Id)) throw new Exception("更新实体时，Id不可为空");
            T t = JsonConvert.DeserializeObject<T>(data.ToString());
            Models.Update(t);
            Db.SaveChanges();
            return type.Name + "更新成功";
        }

        public string Delete(JObject data)
        {
            string Id = data.Value<string>("Id");
            if (string.IsNullOrWhiteSpace(Id)) throw new Exception("删除实体时，Id不可为空");
            var mode = Models.FirstOrDefault(m => m.Id == Guid.Parse(Id));
            if (mode == null) return $"ID:{Id}的{type.Name}不存在";
            Models.Remove(mode);
            Db.SaveChanges();
            return type.Name + "删除成功";
        }

        [Obsolete("仅供测试")]
        public List<T> FindAll()
        {
            var res = Models.Where(t => true);
            return res.ToList();
        }

        public async Task<List<T>> Sel(JObject? seaObj) 
        {
            if (seaObj == null) return await Models.ToListAsync();
            //获取泛型类中的所有属性
            var sql = $"select * from {type.Name} where 1 = 1";
            var props = type.GetProperties();
            var sProps = seaObj.Properties();
            //筛选出该模型中含有的属性
            var seaProps = sProps.Where(
                p => props.Any(pp => pp.Name == p.Name) 
            );

            foreach(var prop in seaProps) {
                var val = prop.Value;
                if(val != null)
                {
                    int num;
                    if (val.GetType().IsEnum)
                        sql += $"and {prop.Name} = {(int)val}";
                    else if (int.TryParse(val.ToString(), out num) && val.GetType() != "".GetType())
                        sql += $"and {prop.Name} = {val}";
                    else
                        sql += $"and {prop.Name} = '{val}'";
                }
            }



            //时间区间筛选
            //筛选规则，检查到属性类型为时间则传入 B_属性名 表示限制其最早时间 E_属性名为最晚时间
            var timeProps = props.Where(p => p.PropertyType == typeof(DateTime));
            foreach(var tp in timeProps)
            {
                var bt = sProps.FirstOrDefault(p => p.Name == "B_" + tp.Name);
                if (bt != null) sql += $"and {tp.Name} > '{bt.Value}'";

                var et = sProps.FirstOrDefault(p => p.Name == "E_" + tp.Name);
                if (et != null) sql += $"and {tp.Name} > '{et.Value}'";
            }



            //分页查询（目前只支持Sqlserver）
            //需要三个参数，排序字段，页面大小以及页面索引
            if(
                sProps.Any(p => p.Name == "PageSize")
                )
            {
                //获取页面大小
                int pagesize = int.Parse(sProps.FirstOrDefault(p => p.Name == "PageSize").Value.ToString());
                string? pageiIndexStr = sProps.FirstOrDefault(p => p.Name == "PageIndex")?.Value?.ToString();

                //获取页面索引
                int pageindex = 1;
                if(pageiIndexStr != null) int.TryParse(pageiIndexStr, out pageindex);

                //获取排序字段
                string OrderBy = "Id";
                string? ord = sProps.FirstOrDefault(p => p.Name == "OrderBy")?.Value?.ToString();
                if (ord != null) OrderBy = ord.ToString();

                //添加分页sql
                sql += $@"order by {OrderBy} offset {(pageindex - 1)*pagesize} row fetch next {pagesize} row only";
            }


            var result = await ExcuteMd(sql);
            return result;
        }

        public async Task<List<T>> ExcuteMd(string sql, params Object[] pms)
        {
            var mds = await Models.FromSqlRaw(sql, pms).ToListAsync();
            mds?.ForEach(md => md.OnFind());
            return mds;
        }
    }
}
