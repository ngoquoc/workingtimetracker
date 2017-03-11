using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Owin;
using WorkingTimeTracker.Core.Authorization;
using WorkingTimeTracker.Core.Authorization.Requirements;
using WorkingTimeTracker.Core.Services;
using WorkingTimeTracker.Core.Services.Implementations;
using WorkingTimeTracker.Core.Validators;
using WorkingTimeTracker.Implementations.Database;
using WorkingTimeTracker.Implementations.Repositories;
using WorkingTimeTracker.Implementations.Services;
using WorkingTimeTracker.Web.Auth;
using WorkingTimeTracker.Web.Middlewares;
using WorkingTimeTracker.Web.Templates;

[assembly: OwinStartup(typeof(WorkingTimeTracker.Web.REST.Startup))]
namespace WorkingTimeTracker.Web.REST
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            var container = ConfigureDependencies();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            app.UseAutofacLifetimeScopeInjector(container);
            ConfigureAuth(app);

            app.UseAutofacMiddleware(container);

            WebApiConfig.Register(config);

            app.UseCors(CorsOptions.AllowAll);
            app.UseWebApi(config);
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions oAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new ApplicationOAuthProvider()
            };

            app.UseOAuthBearerTokens(oAuthServerOptions);
        }

        private IContainer ConfigureDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Database contexts
            builder.RegisterType<AuthDbContext>().AsSelf().InstancePerRequest();
            builder.RegisterType<WorkingTimeTrackerDbContext>().AsSelf().InstancePerRequest();

            // Authentication infrastructures
            builder.Register(c => 
                    new UserStore<IdentityUser>(c.Resolve<AuthDbContext>())
                )
                .AsImplementedInterfaces().InstancePerRequest();
            builder.Register(c =>
                new IdentityFactoryOptions<UserManager<IdentityUser>>()
                {
                    DataProtectionProvider = new DpapiDataProtectionPr‌​ovider("WorkingTimeTracker")
                });

            //builder.RegisterType<UserManager<IdentityUser, string>>().AsSelf();
            builder.RegisterType<UserManager<IdentityUser>>().As<UserManager<IdentityUser, string>>().AsSelf();
            builder.RegisterType<RoleStore<IdentityRole>>().AsImplementedInterfaces();
            builder.RegisterType<RoleManager<IdentityRole>>().AsSelf();

            builder.Register(c => HttpContext.Current.GetOwinContext().Authentication);
            builder.RegisterType<SignInManager<IdentityUser, string>>().AsSelf();

            builder.RegisterType<AspNetIdentityUserManager>().AsImplementedInterfaces();
            builder.RegisterType<SignInManager>().AsImplementedInterfaces();

            builder.RegisterType<ThreadPrincipalCurrentUserResolver>().AsImplementedInterfaces();

            // Service implementations
            builder.RegisterType<AspNetIdentityAuthenticationService>().AsImplementedInterfaces();

            builder.RegisterType<QueryParser>().AsImplementedInterfaces();
            builder.RegisterType<AuthorizationService>().AsImplementedInterfaces();
            builder.RegisterType<UserService>().AsImplementedInterfaces();
            builder.RegisterType<TimeEntryService>().AsImplementedInterfaces();
            builder.RegisterType<TimeEntrySummaryReportTemplating>().AsSelf().SingleInstance();

            // Validators
            builder.RegisterType<ChangePasswordCommandValidator>().AsSelf();
            builder.RegisterType<DeleteTimeEntryCommandValidator>().AsSelf();
            builder.RegisterType<RegisterCommandValidator>().AsSelf();
            builder.RegisterType<UpsertTimeEntryValidator>().AsSelf();
            builder.RegisterType<UpsertUserCommandValidator>().AsSelf();
            builder.RegisterType<UpdateCurrentUserSettingsCommandValidator>().AsSelf();
            builder.RegisterType<DeleteUserCommandValidator>().AsSelf();

            builder.Register(context =>
            {
                return new ValidationService(
                    context.Resolve<ChangePasswordCommandValidator>(),
                    context.Resolve<DeleteTimeEntryCommandValidator>(),
                    context.Resolve<RegisterCommandValidator>(),
                    context.Resolve<UpsertTimeEntryValidator>(),
                    context.Resolve<UpsertUserCommandValidator>(),
                    context.Resolve<UpdateCurrentUserSettingsCommandValidator>(),
                    context.Resolve<DeleteUserCommandValidator>()
                );
            })
            .AsImplementedInterfaces();

            // Authorization requirements
            builder.RegisterType<TimeEntryRequirement>().AsSelf();
            builder.RegisterType<TimeEntryTypeRequirement>().AsSelf();
            builder.RegisterType<UserRequirement>().AsSelf();
            builder.RegisterType<UserTypeRequirement>().AsSelf();
            builder.Register(context =>
            {
                var authorizationService = new AuthorizationService();
                authorizationService.SetResourceRequirements(
                    context.Resolve<TimeEntryRequirement>(),
                    context.Resolve<UserRequirement>()
                );
                authorizationService.SetResourceTypeRequirements(
                    context.Resolve<TimeEntryTypeRequirement>(),
                    context.Resolve<UserTypeRequirement>()
                );

                return authorizationService;
            })
            .AsImplementedInterfaces();

            // Repository implementations
            builder.RegisterType<UserRepository>().AsImplementedInterfaces();
            builder.RegisterType<TimeEntryRepository>().AsImplementedInterfaces();

            // Middlewares
            builder.RegisterType<IdentityRefreshMiddleware>();

            return builder.Build();
        }
    }
}
