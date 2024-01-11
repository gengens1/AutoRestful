using Microsoft.EntityFrameworkCore;

namespace dynamic.Project.Entity
{
    public interface IEntityBase
    {
        void OnInsert();
        void OnUpdate();
        void OnFind();
        void OnDelete();
        void OnModelCreating(ModelBuilder modelBuilder);
    }
}
