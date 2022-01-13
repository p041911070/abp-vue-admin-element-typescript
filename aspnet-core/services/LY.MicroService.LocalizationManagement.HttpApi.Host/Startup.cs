using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Volo.Abp.IO;
using Volo.Abp.Modularity.PlugIns;

namespace LY.MicroService.LocalizationManagement;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplication<LocalizationManagementHttpApiHostModule>(options =>
        {
            // ���� Modules Ŀ¼�������ļ���Ϊ���
            // ȡ����ʾ��������������Ŀ��ģ�飬��Ϊͨ���������ʽ����
            var pluginFolder = Path.Combine(
                    Directory.GetCurrentDirectory(), "Modules");
            DirectoryHelper.CreateIfNotExists(pluginFolder);
            options.PlugInSources.AddFolder(
                pluginFolder,
                SearchOption.AllDirectories);
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.InitializeApplication();
    }
}
