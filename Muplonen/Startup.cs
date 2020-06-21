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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Register database
            services.AddDbContextPool<MuplonenDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("muplonen")));

            services.AddSingleton<MessageHandlerTypes>();
            services.AddSingleton<ClientManager>();

            // Pool message objects
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

            app.UseMiddleware<ClientSessionMiddleware>();
        }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="services">Configured services.</param>
        private void InitializeDatabase(IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<MuplonenDbContext>();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
        }
    }
}
