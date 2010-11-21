萤火虫框架漩涡文本本地化工具项目(Firefly.Eddy)

地狱门神(F.R.C.)


1 概论

本项目为萤火虫框架中原有的文本本地化工具及附属插件分离而来，用于进行显示和保存对照的几组本地化文本。
本项目的文本本地化工具有一些附属的插件，用于辅助翻译工作。


2 各程序集功能介绍

2.1 主程序(Eddy.exe)
文本本地化工具主程序。

2.2 项目库(Firefly.Project.dll)
本库定义了插件的接口。

2.3 差异比较高亮插件(Eddy.DifferenceHighlighter.dll)
实现差异比较高亮。

2.4 控制符高亮插件(Eddy.EscapeSequenceHighlighter.dll)
实现控制符高亮。

2.5 朗读插件(Eddy.Voice.dll)
实现使用TTS进行语音朗读。

2.6 查找替换插件(Eddy.FindReplace.dll)
实现查找替换。

2.7 WQSG文本插件(Eddy.WQSG.dll)
实现对WQSG格式的支持。

2.8 日汉转换插件(Eddy.J2G.dll)
实现日本汉字到简体汉字的转换。

2.9 模板翻译插件(Eddy.TemplateTranslate.dll)
实现按模板翻译重复文本。


3 环境要求

本项目使用 Visual Basic 10.0 编写，开发时需要 Microsoft .Net Framework 4.0 编译器 或 Visual Studio 2010 支持。
本项目运行时需要 Microsoft .Net Framework 4 或 Microsoft .Net Framework 4 Client Profile 运行库支持。
Microsoft .Net Framework 4 (x86/x64，48.1MB)
http://download.microsoft.com/download/9/5/A/95A9616B-7A37-4AF6-BC36-D6EA96C8DAAE/dotNetFx40_Full_x86_x64.exe
Microsoft .NET Framework 4 Client Profile (x86，28.8MB)
http://download.microsoft.com/download/3/1/8/318161B8-9874-48E4-BB38-9EB82C5D6358/dotNetFx40_Client_x86.exe


4 用户使用协议

以下协议不针对示例(Examples文件夹)：
本项目是免费自由软件，所有源代码和可执行程序按照BSD许可证授权，详见License.zh.txt。
本项目的所有文档不按照BSD许可证授权，你可以不经修改的复制、传播这些文档，你还可以引用、翻译这些文档，其他一切权利保留。

以下协议针对示例(Examples文件夹)：
本项目的示例进入公有领域，可以随意修改使用。


5 备注

如果发现了BUG，或者有什么意见或建议，请到以下网址与我联系。
http://www.cnblogs.com/Rex/Contact.aspx?id=1
常见的问题将在今后编制成Q&A。
