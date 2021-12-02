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
            //注册swagger
            services.AddSwaggerGen();

            //TrueServe trueServe = new TrueServe(21);
            //trueServe.RunServe(
            //    @"G:\成果\群塔施工\漕河泾\施工进度信息表(2).xlsx",
            //    @"G:\成果\群塔施工\漕河泾\塔吊信息表格1018-原始.xlsx",
            //    @"G:\成果\群塔施工\漕河泾\塔吊附着信息.xlsx");

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
            //注册swagger
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
