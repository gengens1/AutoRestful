using System.Diagnostics;
using System.Reflection;

namespace dynamic.Project.Base.Mate
{
    /// <summary>
    /// 元数据，负责存储一些全局的数据，以及读取配置文件中的数据，应当在Program.cs中初始化
    /// </summary>
    public class MateData
    {
        /// <summary>
        /// 数据库连接字符串，应当在Program.cs中初始化
        /// </summary>
        public static string ConnectDbString;

        /// <summary>
        /// 程序集
        /// </summary>
        private static Assembly[] _assems;

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
        /// 加载程序集目录下的所有dll
        /// </summary>
        public static IEnumerable<Assembly> LoadAssmblyDirDll()
        {
            DirectoryInfo dir = new DirectoryInfo(EntityDir);
            if (!dir.Exists) dir.Create();
            var dllFiles = dir.GetFiles().Where(f => f.Name.EndsWith(".dll"));
            foreach (var dllFile in dllFiles)
            {
                yield return Assembly.LoadFile(dllFile.FullName);
            }
        }
    }
}
