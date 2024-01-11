using dynamic.Project.Entity;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace dynamic.Project.Base.Mate
{
    /// <summary>
    /// 元数据，负责存储一些全局的数据，以及读取配置文件中的数据，应当在Program.cs中初始化
    /// </summary>
    public class MateData
    {

        /// <summary>
        /// 已经注册的实体模型
        /// </summary>
        public static List<Type> RegistedEntityModels { get; set; } = new List<Type>();

        /// <summary>
        /// 数据库连接字符串，应当在Program.cs中初始化
        /// </summary>
        public static string ConnectDbString;

        /// <summary>
        /// 程序集
        /// </summary>
        private static Assembly[] _assems;

        /// <summary>
        /// 实体目录
        /// </summary>
        public static string _entityDir;
        public static string EntityDir
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_entityDir))
                {
                    _entityDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "entity");
                    if(!Directory.Exists(_entityDir))
                    {
                        Directory.CreateDirectory(_entityDir);
                    }
                }
                return _entityDir;
            }
            set => _entityDir = value;
        }
            


        /// <summary>
        /// 程序集
        /// </summary>
        public static Assembly[] Assems
        {
            get
            {
                if (_assems == default)
                    _assems = AppDomain.CurrentDomain.GetAssemblies();
                return _assems;
            }
            set => _assems = value;
        }

        /// <summary>
        /// 监控程序集目录，当程序目录下的dll发生变化时，自动重启程序
        /// </summary>
        public static void StartWatchEntity()
        {
            var watcher = new FileSystemWatcher(EntityDir);
            Console.WriteLine("实体监控启动");
            Console.WriteLine("实体监控目录：" + EntityDir);
            watcher.Filter = "*.dll";
            watcher.IncludeSubdirectories = false;
            watcher.Changed += OnBinChange;
            watcher.Created += OnBinChange;
            watcher.Deleted += OnBinChange;
            watcher.Renamed += OnBinChange;
            watcher.EnableRaisingEvents = true;
        }

        public static void OnBinChange(Object sender,FileSystemEventArgs args)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Process.GetCurrentProcess().ProcessName + ".exe");
            Process.Start(path);
            Environment.Exit(0);
        }

        /// <summary>
        /// 获取所有程序集，包括引用的程序集
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAllAssmbly()
        {
            var assm = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var refersNames = assm.SelectMany(a => a.GetReferencedAssemblies());
            var refers = refersNames.Select(r => Assembly.Load(r));
            refers.ToList().ForEach(refer => {
                if (!assm.Contains(refer)) assm.Add(refer);
            });

            //加载实体程序集目录下的所有dll
            var entityDirDlls = LoadAssmblyDirDll();
            entityDirDlls.ToList().ForEach(dll =>
            {
                if (!assm.Contains(dll)) assm.Add(dll);
            });

            return assm;
        }

        /// <summary>
        /// 加载实体程序集目录下的所有dll
        /// </summary>
        public static IEnumerable<Assembly> LoadAssmblyDirDll()
        {
            DirectoryInfo dir = new DirectoryInfo(EntityDir);
            var dllFiles = dir.GetFiles().Where(f => f.Name.EndsWith(".dll"));
            foreach (var dllFile in dllFiles)
            {
                yield return Assembly.LoadFile(dllFile.FullName);
            }
        }

        public static string ApiAddress { get; set; }

        public static void OnInited(WebApplication app)
        {
            new Thread(()=> {
                Thread.Sleep(500);
                ApiAddress = app.Urls.FirstOrDefault();
                if(ApiAddress == default(string))
                {
                    OnInited(app);
                    return;
                }
                Console.WriteLine(Specify);
                while(true)
                {
                    string key = Console.ReadLine();
                    if (key.ToLower() == "help")
                    {
                        Console.WriteLine(CmdSpecify);
                    }
                    else if (key.ToLower() == "apispe")
                    {
                        Console.WriteLine(ApiSpecify);
                    }
                    else if (key.ToLower() == "spe")
                    {
                        Console.WriteLine(UseSepcify);
                    }
                    else if (key.ToLower() == "api")
                    {
                        Console.WriteLine("所有api接口：");
                        RegistedEntityModels.ForEach(m =>
                        {
                            Console.WriteLine($"{m.Name}：{ApiAddress}/api/{m.Name}");
                        });
                    }
                    else if (key.ToLower().StartsWith("api ") && key.Split(" ").Length > 2)
                    {
                        var entityName = key.Split(" ")[1];
                        var entity = RegistedEntityModels.FirstOrDefault(
                            m => m.Name.ToUpper()
                            .Contains(entityName.ToUpper()));
                        if (entity == null)
                        {
                            Console.WriteLine($"未找到名称包含{entityName}的实体");
                        }
                        else
                        {
                            Console.WriteLine($"{entity.Name}：{ApiAddress}/api/{entity.Name}");
                        }
                    }
                    else if (key.ToLower() == "quit" || key.ToLower() == "exit")
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("未知命令，输入help查看帮助");
                    }   
                }
            }).Start();
        }
        /// <summary>
        /// 说明信息
        /// </summary>
        public static string Specify
        {
            get
            {
                return @$"
      _   _____  _____  _________    ___     _______     ________   ______   _________  
     / \ |_   _||_   _||  _   _  | .'   `.  |_   __ \   |_   __  |.' ____ \ |  _   _  | 
    / _ \  | |    | |  |_/ | | \_|/  .-.  \   | |__) |    | |_ \_|| (___ \_||_/ | | \_| 
   / ___ \ | '    ' |      | |    | |   | |   |  __ /     |  _| _  _.____`.     | |     
 _/ /   \ \_\ \__/ /      _| |_   \  `-'  /  _| |  \ \_  _| |__/ || \____) |   _| |_    
|____| |____|`.__.'      |_____|   `.___.'  |____| |___||________| \______.'  |_____|   
{DateTime.Now.ToLongTimeString()}
本程序现共注册： {RegistedEntityModels.Count}个实体模型
项目地址：https://gitee.com/gengens/auto-rest-ful.git
输入help获取帮助信息
";
            }

        }
    
        public static string CmdSpecify
        {
            get
            {
                return $@"
1. help              显示本帮助文档
2. spe               显示使用说明
3. apispe            显示api使用说明
4. api               显示所有实体接口及接口信息
5. api <entityName>  根据实体名称模糊查找某个实体的接口信息
6. quit/exit         退出本程序";
            }
        }

        public static string ApiSpecify
        {
            get
            {
                return $@"
1. Get请求 - 查询
（1） 基础查询
Get请求采取的是Url键值对的形式，例如：{ApiAddress}/api/userinfo?id=1
表示查出所有id为1的userinfo实体列表，
不过，实际EntityBase中的Id是Guid，这里只是举个例子
如果不传id参数，则表示查询所有userinfo实体列表，使用&可附加多个条件，

（2）时间区间查询
除了精准查询以为，也支持时间区间查询，例如：{ApiAddress}/api/userinfo?B_CreateAt=2022-01-01&L_CreateAt=2022-01-02
表示查询出所有创建时间在2022-01-01到2022-01-02之间的userinfo实体列表
当然也可以只传一个参数，例如：{ApiAddress}/api/userinfo?B_CreateAt=2022-01-01
表示查询出所有创建时间大于2022-01-01的userinfo实体列表
B_<属性名> 表示大于,E_<属性名>表示小于

（3）分页查询
分页查询需要三个参数，分别为排序字段（OrderBy），页面大小(PageSize)以及页面索引(PageIndex)，
例如：{ApiAddress}/api/userinfo?PageSize=10&PageIndex=2&OrderBy=Id
这三个字段为关键字，在编写模型时请勿使用占用
排序字段（OrderBy）可选，默认为Id
页面索引 (PageIndex)可选，默认为1
页面大小 (PageSize)必选，否则不分页

（4）模糊查询
模糊查询需要在属性名前加L_，例如：{ApiAddress}/api/userinfo?L_Name=张三
表示查询出所有Name中包含张三的userinfo实体列表


2. Post请求 - 新增
Post请求采取的是Json格式的数据，
Guid会自动生成，CreateAt和UpdateAt会自动赋值
这三个字段由程序自动维护

3. Put请求 - 修改
Put请求采取的是Json格式的数据，但必须包含Id字段

4. Delete请求 - 删除
Delete请求采取的键值对格式的数据，参数放在Url中，例如：{ApiAddress}/api/userinfo?id=1
表示删除Id为1的userinfo实体
";
            }
        }
    
        public static string UseSepcify
        {
            get
            {
                return $@"
1. 基本使用
新建一个类库项目，使用.net 6.0
引用本程序的dynamic.Project.Entity类库
编写你需要的实体，
实体必须继承EntityBase类
EntityBase类中包含Id,CreateAt,UpdateAt三个字段 其中Id为Key，皆自动维护
EntityBase类中包含5个生命周期方法，分别为
OnModelCreating,
OnInsert,OnUpdate,OnDelete,OnFind
可选择性重写这些方法，这些方法会在对应的操作时自动调用
编写完实体后，编译项目，将编译后的dll放入本程序的entity目录下
本程序将自动重启，并加载新的实体，同步至数据库，实现其Restful接口

2. 注意
本程序暂不支持实体的修改，如需修改实体，请删除实体，重新创建
注意保存表中的数据，以免丢失
";
            }
        }
    }
}
