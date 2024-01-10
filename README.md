# AutoRestFull

自动实现RestFull接口，支持动态查询

## 使用步骤

将你编写的实体类，继承自本项目中的EntityBase,

然后将你的项目打包成dll文件，把文件塞到，entity目录下，

本程序将自动重启，将你的实体映射到数据库中，并实现其RestFull风格接口



## 默认地址

```
http://localhost:5167/api/{Model}
```



# 注意事项

- 注意修改数据库配置



