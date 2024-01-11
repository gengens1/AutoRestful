using dynamic.Project.Base;
using dynamic.Project.Base.Mate;
using dynamic.Project.Db;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//��ʼ��MateData
MateData.ConnectDbString = builder.Configuration.GetConnectionString("Default");
//���س���Ŀ¼�µ����г���
MateData.LoadAssmblyDirDll();
//�������򼯼��
MateData.StartWatchEntity();
//��ʼ�����ݿ������Ĳ�Ǩ��
ZengDb db = new ZengDb();
var sql = db.Database.GenerateCreateScript();
db.Database.EnsureCreated();
sql.Split("GO").ToList().ForEach(s =>
{
    try
    {
        db.Database.ExecuteSqlRaw(s);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
});
////db.Database.Migrate();

IocContainer.Instance.Register(db);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

MateData.OnInited(app);
app.Run();



