using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerCraneGroup.Services;

namespace TowerCraneGroup
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //ע��swagger
            services.AddSwaggerGen();

            //TrueServe trueServe = new TrueServe(21);
            //trueServe.RunServe(
            //    @"G:\�ɹ�\Ⱥ��ʩ��\�����\ʩ��������Ϣ��(2).xlsx",
            //    @"G:\�ɹ�\Ⱥ��ʩ��\�����\������Ϣ���1018-ԭʼ.xlsx",
            //    @"G:\�ɹ�\Ⱥ��ʩ��\�����\����������Ϣ.xlsx");

            //CHJDemo cHJDemo = new CHJDemo();
            //cHJDemo.Run();
            //DemoService demoService = new DemoService(21);
            //demoService.Run();
            //demoService.DebugServe();
            //demoService.GreedyServe();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //ע��swagger
            app.UseSwagger();
            app.UseSwaggerUI(c=>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api V1");
            });
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
