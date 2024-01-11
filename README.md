# AutoRestFul
将您的实体类代码，自动映射到数据库，并实现Restful接口
支持动态查询，分页，排序，等等

本项目宗旨在于帮助开发者，
快速的搭建后端接口，减少后端开发时间，提高开发效率，
您只需要编写实体类，编译后放入项目的entitys文件下，
即可自动生成数据库表，以及对应的Restful接口
**即插即用**

本项目内涵盖命令行工具，命令行工具功能如下
- 查询使用说明
- 查询现存接口
- ETC

## 注意事项

- 请在appsettings.json中配置数据库连接字符串
- 暂不支持实体修改，如需修改请删除数据表，重新生成，**注意备份表中的数据集**

## 使用说明 - 可在命令行工具中查询

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

## Api说明

### Get请求 - 查询

##### **基础查询**

Get请求采取的是Url键值对的形式，例如：```{ApiAddress}/api/userinfo?id=1```  
表示查出所有id为1的userinfo实体列表，  
不过，实际EntityBase中的Id是Guid，这里只是举个例子  
如果不传id参数，则表示查询所有userinfo实体列表，使用&可附加多个条件，  

##### 时间区间查询  

除了精准查询以为，也支持时间区间查询，例如：
```{Address}/api/userinfo?B_CreateAt=2022-01-01&L_CreateAt=2022-01-02 ```  
表示查询出所有创建时间在2022-01-01到2022-01-02之间的userinfo实体列表  
当然也可以只传一个参数，

例如：```{Address}/api/userinfo?B_CreateAt=2022-01-01```
表示查询出所有创建时间大于2022-01-01的userinfo实体列表  
**B\_<属性名> 表示大于,E\_<属性名>表示小于**  

##### 分页查询  

分页查询需要三个参数，分别为排序字段`（OrderBy）`，页面大小`（PageSize）`以及页面索引`（PageIndex）`，  
例如：

```{Address}/api/userinfo?PageSize=10&PageIndex=2&OrderBy=Id```
    这三个字段为关键字，在编写模型时请勿使用占用  
    排序字段（OrderBy）可选，默认为Id  
    页面索引 (PageIndex)可选，默认为1  
    页面大小 (PageSize)必选，否则不分页  

##### 模糊查询  
模糊查询需要在属性名前加L_，
例如：```{Address}/api/userinfo?L_Name=张三 ```
表示查询出所有Name中包含张三的userinfo实体列表  

### Post请求 - 新增  

Post请求采取的是Json格式的数据
`Id`，`CreateAt`,`UpdateAt`
这三个字段由程序自动维护，无需传值

### Put请求 - 修改  
Put请求采取的是Json格式的数据，但必须包含Id字段

### Delete请求 - 删除  
Delete请求采取的键值对格式的数据，参数放在Url中，
例如：```{Address}/api/userinfo?id=1```
表示删除Id为1的userinfo实体