using Microsoft.EntityFrameworkCore;

namespace dynamic.Project.Entity
{
    public class EntityBase : IEntityBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime UpdateAt { get; set; } = DateTime.Now;
        public EntityBase()
        {
            
        }
        public virtual void OnDelete()
        {
        }

        public virtual void OnFind()
        {
        }

        public virtual void OnInsert()
        {
        }

        public virtual void OnUpdate()
        {
            UpdateAt = DateTime.Now;
        }

        /// <summary>
        /// 基类方法会进行注册实体操作，请在重写后调用base.OnModelCreating(modelBuilder)
        /// </summary>
        /// <param name="modelBuilder"></param>
        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity(this.GetType()).HasKey(nameof(Id));
        }

    }
}
