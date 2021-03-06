using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CloudEd.DAL.Repositories;
using CloudEd.DAL.Persistence;
using CloudEd.BLL.Core.Quiz.Services;
using CloudEd.BLL.Core.Question.Services;
using CloudEd.BLL.Core.QuizWorkflow.Services;

namespace CES
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("mongo");

            var mongoQuizRepository = new MongoRepository<Quiz, Guid>(connectionString);
            services.AddSingleton<IRepository<Quiz, Guid>>(mongoQuizRepository);

            var mongoQuestonRepository = new MongoRepository<Question, Guid>(connectionString);
            services.AddSingleton<IRepository<Question, Guid>>(mongoQuestonRepository);

            services.AddSingleton<IQuizBackofficeService, QuizBackofficeService>();
            services.AddSingleton<IQuestionBackofficeService, QuestionBackofficeService>();

            services.AddSingleton<IQuestionService, QuestionService>();
            services.AddSingleton<IQuizService, QuizService>();
            services.AddSingleton<IQuizWorkflowService, QuizWorkflowService>();


            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
