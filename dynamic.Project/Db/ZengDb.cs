using dynamic.Project.Entity.Plugins;
using dynamic.Project.Entity;
using dynamic.Project.Base.Mate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;

namespace dynamic.Project.Db
{
    public class ZengDb : DbContext
    {

        public ZengDb() : base(new DbContextOptions<ZengDb>()) 
        {
            
        }


        /// <summary>
        /// 数据库连接信息
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(MateData.ConnectDbString);            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            var Assmblys = MateData.GetAllAssmbly();
            var models = Assmblys.SelectMany(a => a.GetTypes())
                    .Where(t => t.IsSubclassOf(typeof(EntityBase)))
                    .ToList();
            if (models.Count > 0)
            {
                models.ForEach(m =>
                {
                    Console.WriteLine("加载实体"+ m.FullName);
                    if (!MateData.RegistedEntityModels.Contains(m)) 
                        MateData.RegistedEntityModels.Add(m);
                    ((EntityBase)Activator.CreateInstance(m)).OnModelCreating(modelBuilder);
                });
            }
            base.OnModelCreating(modelBuilder);
        }
    }
   
}
