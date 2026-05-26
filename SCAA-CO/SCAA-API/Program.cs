namespace SCAA_API
{
    public class Program
    {
        protected Program() { }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddControllers();
            builder.AddSwagger();
            builder.AddDatabase();
            builder.AddHealthChecks();
            builder.AddRepository();
            builder.AddMapster();
            builder.AddServices();
            builder.AddIdentity();
            builder.AddAuthentication();
            builder.AddCors();


            var app = builder.Build();

            app.UseErrorHandling();
            app.UseDbAutoUpdate();
            app.UseCors("AllowAll");
            app.UseSwagger();
            app.UseSwaggerUI();
            if (app.Environment.IsDevelopment())
                app.UseHttpsRedirection();
            app.MapHealthChecks("/health");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
