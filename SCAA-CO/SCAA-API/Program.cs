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

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDbAutoUpdate();
            if (app.Environment.IsDevelopment())
                app.UseHttpsRedirection();
            app.MapHealthChecks("/health");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
