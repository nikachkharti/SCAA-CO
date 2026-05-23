namespace SCAA_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddControllers();
            builder.AddSwagger();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();
            if (app.Environment.IsDevelopment())
                app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
