using Hotel.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IGuestService, GuestService>();
        services.AddScoped<IReservationService, ReservationService>();

        return services;
    }
}
