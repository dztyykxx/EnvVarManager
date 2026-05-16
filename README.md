# 环境变量管理器

一个给本机 skills、脚本和开发工具使用的小型 Windows WPF 客户端，用来查看和更改当前用户级 API key 环境变量。

## 功能

- 查看常见 API key 变量是否已设置。
- 自动显示当前 Windows 用户级环境变量里已经存在的变量名。
- 默认按 `sk-****ef` 这类格式脱敏展示已设置变量。
- 在右侧详情区按需查看或复制完整当前值。
- 写入、覆盖、删除当前 Windows 用户级环境变量。
- 添加自定义变量名，并保存在 `%APPDATA%\EnvVarManager\custom-variables.json`。
- 保存或删除后广播 Windows 环境变量变更通知。
- 支持搜索变量名、服务名、说明和类别。

## 已预置的常见变量

- `OPENAI_API_KEY`
- `OPENAI_BASE_URL`
- `ANTHROPIC_API_KEY`
- `GEMINI_API_KEY`
- `GOOGLE_API_KEY`
- `DASHSCOPE_API_KEY`
- `DEEPSEEK_API_KEY`
- `MOONSHOT_API_KEY`
- `ZAI_API_KEY`
- `ZHIPUAI_API_KEY`
- `QIANFAN_OPENAI_API_KEY`
- `QIANFAN_ACCESS_KEY`
- `QIANFAN_SECRET_KEY`
- `ARK_API_KEY`
- `HUNYUAN_API_KEY`
- `TENCENTCLOUD_SECRET_ID`
- `TENCENTCLOUD_SECRET_KEY`
- `MINIMAX_API_KEY`
- `BAICHUAN_API_KEY`
- `YI_API_KEY`
- `STEP_API_KEY`
- `SILICONFLOW_API_KEY`
- `MODELSCOPE_API_TOKEN`
- `OPENROUTER_API_KEY`
- `GITHUB_TOKEN`
- `HF_TOKEN`
- `TAVILY_API_KEY`
- `SERPAPI_API_KEY`
- `FIRECRAWL_API_KEY`

更多变量可以在界面左侧添加自定义名称。

## 开发运行

```powershell
dotnet restore
dotnet run --project src\EnvVarManager.App\EnvVarManager.App.csproj
```

## 测试与构建

```powershell
dotnet test
dotnet build EnvVarManager.sln
```

## 发布本机可执行文件

```powershell
dotnet publish src\EnvVarManager.App\EnvVarManager.App.csproj -c Release -r win-x64 --self-contained false -o publish
```

发布后运行：

```powershell
.\publish\EnvVarManager.App.exe
```

## 使用说明

1. 在左侧选择一个变量。
2. 在右侧输入新 API key 或 token。
3. 点击“保存新值”。
4. 重新打开终端、IDE 或 Codex，让新进程读取最新用户级环境变量。

删除变量只会删除当前用户级环境变量，不会修改系统级环境变量。
保存和删除不会广播通知其他程序刷新环境变量；这是为了避免 Windows 消息广播拖慢操作。

## 安全边界

- 本工具不会把真实 API key 写入项目文件。
- 自定义变量配置只保存变量名，不保存变量值。
- 已设置的变量只显示脱敏结果。
- 当前已打开的进程通常不会自动获得新值，建议保存后重启相关终端或工具。
