using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dynamic.Project.Base
{
    //IOC容器，支持单例与多例
    public class IocContainer
    {
        public List<BeanInfo> _container;

        public List<BeanInfo> Container
        {
            get
            {
                if (_container == null)
                {
                    _container = new List<BeanInfo>();
                }
                return _container;
            }
            set => _container = value;
        }

        public static IocContainer _instance;
        public static IocContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IocContainer();
                }
                return _instance;
            }
            set => _instance = value;
        }

        /// <summary>
        /// 注册对象
        /// </summary>
        /// <param name="isSingleton"></param>
        /// <typeparam name="IInterfase"></typeparam>
        /// <typeparam name="IImplement"></typeparam>
        public void Register<IInterfase, IImplement>(bool isSingleton = true)
            where IImplement : IInterfase
            where IInterfase : class
        {
            if (Exsist<IInterfase>() || Exsist<IImplement>())
                throw new Exception($"注册失败，容器中已经存在类型为{typeof(IInterfase).FullName}的Bean");
            BeanInfo b = BeanInfo.Create<IInterfase, IImplement>(isSingleton);
            Container.Add(b);
        }
        public void Register<IImplement>(bool isSingleton = true)
            where IImplement : class
        {
            if (Exsist<IImplement>())
                throw new Exception($"注册失败，容器中已经存在类型为{typeof(IImplement).FullName}的Bean");
            BeanInfo b = BeanInfo.Create<IImplement>(isSingleton);
            Container.Add(b);
        }

        public void Register(Object obj,bool isSingleton = true)
        {
            if (Exsist(obj))
                throw new Exception($"注册失败，容器中已经存在类型为{obj.GetType().FullName}的Bean");
            BeanInfo b = BeanInfo.Create(obj,isSingleton);
            Container.Add(b);
        }


        public T GetOrDefault<T>()
            where T : class
        {
            Type t = typeof(T);
            return Container.FirstOrDefault(b => b.IsThis(t))?.Get<T>();
        }

        public T Get<T>()
            where T : class
        {
            var t = this.GetOrDefault<T>();
            if (t == null) throw new Exception($"{typeof(T).FullName}在容器中不存在");
            return t;
        }

        public object GetOrDefault(Type t)
        {
            return Container.FirstOrDefault(b => b.IsThis(t))?.Get();
        }

        public object Get(Type type)
        {
            var t = this.GetOrDefault(type);
            if (t == null) throw new Exception($"{type.FullName}在容器中不存在");
            return t;
        }


        public bool Exsist<T>()
        {
            Type t = typeof(T);
            return Container.Any(b => b.IsThis(t));
        }

        public bool Exsist(Object obj)
        {
            Type t = obj.GetType();
            return Container.Any(b => b.IsThis(t));
        }

        /// <summary>
        /// 从容器中注销类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SignOut<T>()
        {
            var info = Container.FirstOrDefault(c => c.IsThis(typeof(T)));
            if (info == null) throw new Exception($"容器中不存在{typeof(T)}的类型，无法注销");
            Container.Remove(info);
        }

    }

    /// <summary>
    /// Bean信息
    /// </summary>
    public class BeanInfo
    {
        public bool IsSingleton;
        public Type? _tinterfase;
        public Type _timplement;

        public object imp;

        private BeanInfo(Type tinterfase, Type timplement, bool isSingleton = true)
        {
            _tinterfase = tinterfase;
            _timplement = timplement;
            IsSingleton = isSingleton;
        }

        /// <summary>
        /// 创建Bean信息
        /// </summary>
        /// <param name="isSingleton">是否单例</param>
        /// <typeparam name="IInterfase">接口</typeparam>
        /// <typeparam name="IImplement">实现类</typeparam>
        /// <returns></returns>
        public static BeanInfo Create<IInterfase, IImplement>(bool isSingleton = true)
            where IImplement : IInterfase
            where IInterfase : class
        {
            return new BeanInfo(typeof(IInterfase), typeof(IImplement), isSingleton);
        }

        /// <summary>
        /// 创建Bean信息
        /// </summary>
        /// <param name="isSingleton"></param>
        /// <typeparam name="IImplement"></typeparam>
        /// <returns></returns>
        public static BeanInfo Create<IImplement>(bool isSingleton = true)
            where IImplement : class
        {
            return new BeanInfo(null, typeof(IImplement), isSingleton);
        }

        public static BeanInfo Create(Object obj,bool isSingleton = true)
        {
            Type t = obj.GetType();
            return new BeanInfo(null, t, isSingleton) { imp = obj};
        }

        public bool IsThis(Type t)
        {
            if (_tinterfase != null && _tinterfase == t)
            {
                return true;
            }
            if (_timplement != null && _timplement.IsAssignableFrom(t))
            {
                return true;
            }
            return false;
        }

        public T Get<T>()
        {
            if (!IsSingleton)
            {
                return (T)Activator.CreateInstance(_timplement);
            }
            if (imp == null)
            {
                imp = Activator.CreateInstance(_timplement);
            }
            return (T)imp;
        }
    
        public object Get()
        {
            if (!IsSingleton)
            {
                return Activator.CreateInstance(_timplement);
            }
            if (imp == null)
            {
                imp = Activator.CreateInstance(_timplement);
            }
            return imp;
        }
    }
}