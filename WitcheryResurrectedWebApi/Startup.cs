using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WitcheryResurrectedWebApi.Download;

namespace WitcheryResurrectedWebApi;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options => options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        services.AddControllersWithViews();

        services.AddSingleton<IAccessTokenManager>(
            _ => new AccessTokenManager("access_tokens.bin")
        );

        services.AddHostedService(provider => provider.GetRequiredService<IAccessTokenManager>());

        services.AddSingleton<IDownloadManager>(_ => new DownloadManager("Downloads"));
        services.AddHostedService(provider => provider.GetRequiredService<IDownloadManager>());
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors("CorsPolicy");

        app.UseEndpoints(endpoints => endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}"));
    }
}
