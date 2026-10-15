using MathApp.Api.Data.Daos;

namespace MathApp.Api.Data;

public static class DataAccessServiceCollectionExtensions
{
    /// <summary>
    /// Registra la fábrica de conexiones y todos los DAO en el contenedor de
    /// dependencias para poder inyectarlos en controladores y servicios.
    /// </summary>
    public static IServiceCollection AddAccesoDatos(this IServiceCollection services, string cadenaConexion)
    {
        services.AddSingleton<IDbConnectionFactory>(new MySqlConnectionFactory(cadenaConexion));

        services.AddScoped<IPlanEstudioDao, PlanEstudioDao>();
        services.AddScoped<IUsuarioDao, UsuarioDao>();
        services.AddScoped<ITemaDao, TemaDao>();
        services.AddScoped<ILeccionDao, LeccionDao>();
        services.AddScoped<IEjercicioDao, EjercicioDao>();
        services.AddScoped<IUsuarioPlanDao, UsuarioPlanDao>();
        services.AddScoped<IProgresoLeccionDao, ProgresoLeccionDao>();
        services.AddScoped<IResultadoEjercicioDao, ResultadoEjercicioDao>();
        services.AddScoped<IPruebaDao, PruebaDao>();
        services.AddScoped<IResultadoPruebaDao, ResultadoPruebaDao>();

        return services;
    }
}
