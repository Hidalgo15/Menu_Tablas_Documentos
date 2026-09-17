using MenuDeDocumentos.Service;
using MenuDeDocumentos.Service.Interface;

namespace MenuDeDocumentos
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<IDocumentoService, DocumentoService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                //app.UseExceptionHandler("/");
                app.UseExceptionHandler("/Documento/Index");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            // Mapea los controladores basados en atributos ([Route("Documento")])
            app.MapControllers();

            // Ruta por defecto MVC
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Documento}/{action=Index}/{id?}");

            app.Run();
        }
    }
}