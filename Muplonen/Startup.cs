using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Muplonen.Clients;
using Muplonen.Clients.Messages;
using Muplonen.DataAccess;
using Muplonen.Security;
using Muplonen.Services;
using System;

namespace Muplonen
{
    /// <summary>
    /// Handles the ASP.NET Core setup.
    /// </summary>
    public sealed class Startup
    {
        /// <summary>
        /// The application's configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Creates a new <see cref="Startup"/> instance.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime and adds services to the container. 
        /// </summary>
        /// <param name="services">Service collection to be populated with Muplonen services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Register database
            services.AddDbContextPool<MuplonenDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("muplonen")));

            // Register singletons
            services.AddSingleton<MessageHandlerTypes>();
            services.AddSingleton<PlayerSessionManager>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            // Register poool for message objects
            services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.TryAddSingleton<ObjectPool<GodotMessage>>(serviceProvider =>
            {
                var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
                var policy = new GodotMessagePooledObjectPolicy();
                return provider.Create<GodotMessage>(policy);
            });

            // Register message handlers
            foreach (Type messageHandler in MessageHandlerTypes.FindMessageHandlersInAssembly())
                services.AddScoped(messageHandler);

            InitializeDatabase(services);

            // Start services
            services.AddHostedService<PlayerTimeoutDetectionService>();
        }

        /// <summary>
        /// This method gets called by the runtime and configures the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application builder to configure the request pipeline.</param>
        /// <param name="env">Information regarding the hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseWebSockets();

            app.UseMiddleware<MuplonenSessionMiddleware>();
        }

        /// <summary>
        /// Initializes the database and applies migrations.
        /// </summary>
        /// <param name="services">Configured services.</param>
        private void InitializeDatabase(IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<MuplonenDbContext>();
            dbContext.Database.EnsureCreated();
        }
    }
}
