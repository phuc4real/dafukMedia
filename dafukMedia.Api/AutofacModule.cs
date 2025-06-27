using Autofac;
using System.Reflection;
namespace dafukMedia.Api;

public class AutofacModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly.Load("dafukMedia.Service"))
               .Where(t => t.Name.EndsWith("Service"))
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope();

        base.Load(builder);
    }
}
