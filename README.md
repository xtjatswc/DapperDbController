# 基于 Dapper 的轻量级数据库查询 API 工具

## 一、简介

实现了一个轻量级的数据库查询 API 工具，支持 SQL Server、MySQL、PostgreSQL 和 Oracle 四种主流数据库，还可扩展其它数据库。通过简洁的设计和灵活的架构，具备**开箱即用**、**易于集成**的特点。即使不使用 Dapper，只要是支持`IDbConnection`接口的 ORM 框架，也能轻松替换核心数据库操作部分，实现快速集成与扩展。同时，该工具内置 Basic Auth 认证机制，保障 API 访问安全。

## 二、核心功能

**多数据库支持**：支持 SQL Server、MySQL、PostgreSQL 和 Oracle 数据库，通过前端下拉菜单或请求参数指定数据库类型，后端动态创建对应连接，可通过修改`CreateConnection`方法轻松扩展更多数据库类型。

**灵活的查询操作**：提供`ExecuteQuery`和`ExecuteNonQuery`两个 API 端点，分别用于执行查询语句（如`SELECT`）和非查询语句（如`INSERT`、`UPDATE`、`DELETE`），满足不同业务场景需求。

**Basic Auth 认证**：通过用户名和密码进行 Basic Auth 认证，确保只有授权用户可访问 API。用户信息存储在`_users`字典中（生产环境建议存储于安全位置），认证逻辑清晰，便于定制扩展。

**Web 界面交互**：内置 HTML 页面（通过`GetUi`接口返回），提供可视化操作界面，用户可直接输入数据库连接字符串、SQL 语句并执行操作，查询结果以 HTML 表格形式展示，直观易用。

**框架无关性**：基于`IDbConnection`接口设计，核心数据库操作使用 Dapper，但可无缝替换为其他支持`IDbConnection`的 ORM 框架，如 SqlSugar、FreeSql 等，或者脱离ORM框架，支持`IDbConnection`的数据库驱动都可以集成进来。

**编码与解码处理**：对数据库连接字符串和 SQL 语句进行 Base64 编码传输，避免特殊字符在网络传输中出现问题，同时在后端进行解码处理，保障数据准确性与安全性。

## 三、技术实现

**后端技术**

**编程语言**：C#

**框架**：.NET Core WebApi

**数据库访问**：Dapper（可替换）

**认证机制**：Basic Auth

**前端技术**

**HTML/CSS**：构建操作界面

**JavaScript**：实现页面交互逻辑、请求发送及结果展示

## 四、NuGet 包依赖

项目依赖以下 NuGet 包，可通过 NuGet 包管理器或`.csproj`文件添加：

**Dapper**：轻量级数据库访问工具，用于执行 SQL 语句。

**MySql.Data**：MySQL 数据库驱动。

**Npgsql**：PostgreSQL 数据库驱动。

**Oracle.ManagedDataAccess**：Oracle 数据库驱动。

**System.Data.SqlClient**：SQL Server 数据库驱动。

## 五、使用方法

**部署项目**：将代码集成到.NET Core WebApi 项目中，确保 NuGet 包引用完整，编译并运行项目。

**访问界面**：通过浏览器访问`/api/db/ui`，弹出 Basic Auth 认证对话框，输入正确用户名和密码（默认：`QtAdmin`/`MGn6Cf8XA55owmya`）进入操作界面。

**执行操作**

在界面中选择数据库类型，输入对应连接字符串和 SQL 语句。

点击`Execute Query`执行查询语句，结果展示在页面下方；点击`Execute Non-Query`执行非查询语句，返回受影响行数。

## 六、适用场景

**极端受限环境**：没有数据库权限且需要对数据库进行操作的场景，当然请合规使用。

**快速集成**：无需复杂配置，快速搭建数据库查询 API。

**多数据库查询**：统一支持多种类型数据库，简化sql查询操作流程。

**小型项目**：轻量级设计，减少依赖，适合资源有限的小型项目。