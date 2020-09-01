using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using MimicAPI.DataBases;
using MimicAPI.Helpers;
using MimicAPI.Helpers.Swagger;
using MimicAPI.Repositories;
using MimicAPI.Repositories.Contracts;
using MimicAPI.V1.Repositories;

namespace MimicAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new MapperConfiguration( cfg => {
                cfg.AddProfile(new DTOMapperProfile());
            });

            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            services.AddDbContext<MimicContext>(opt => {
                opt.UseSqlite("Data Source = DataBases\\Mimic.db");
            }
                );
            services.AddMvc();
            services.AddScoped<IPalavraRepository, PalavraRpository>();
            services.AddApiVersioning
                (
                cfg =>
                {
                    cfg.ReportApiVersions = true;
                    //cfg.ApiVersionReader = new  HeaderApiVersionReader("api-version");
                    cfg.AssumeDefaultVersionWhenUnspecified = true;
                    cfg.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1,0);
                }
                );

            services.AddSwaggerGen(cfg => {
                cfg.ResolveConflictingActions(ApiDescription => ApiDescription.First()); ;
                cfg.SwaggerDoc("v2.0", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "MimicAPI - V2.0",
                    Version = "v2.0"
                });

                cfg.SwaggerDoc("v1.1", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "MimicAPI - V1.1",
                    Version = "v1.1"
                });

                cfg.SwaggerDoc("v1.0", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "MimicAPI - V1.0",
                    Version = "v1.0"
                }) ;

                var CaminhoProjeto = PlatformServices.Default.Application.ApplicationBasePath;
                var NomeProjeto = $"{PlatformServices.Default.Application.ApplicationName}.xml";
                var CaminhoProjetoXmlComentario = Path.Combine(CaminhoProjeto, NomeProjeto);

                cfg.IncludeXmlComments(CaminhoProjetoXmlComentario);

                cfg.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
                   
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }
                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
                    }
                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });
                cfg.OperationFilter<ApiVersionOperationFilter>();
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                       }

            app.UseStatusCodePages();

            app.UseMvc();

            app.UseSwagger();

            app.UseSwaggerUI(
                cfg => {
                    cfg.SwaggerEndpoint("/swagger/v2.0/swagger.json", "MimicAPI - V2.0");
                    cfg.SwaggerEndpoint("/swagger/v1.1/swagger.json", "MimicAPI - V1.1");
                    cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json","MimicAPI - V1.0");
                    cfg.RoutePrefix = String.Empty;
                } 
                );
        }
    }
}
