using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoApp.Shared
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
