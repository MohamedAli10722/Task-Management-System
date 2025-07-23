using Microsoft.EntityFrameworkCore;
using Area.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.ComponentModel;
using Area.Notification;
using Microsoft.SqlServer.Dac.Model;
using Area.Jwt;
using Area.ReportService;


namespace TaskArea
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //builder.WebHost.ConfigureKestrel(serverOptions =>
            //{
            //    serverOptions.ListenAnyIP(5000); 
            //});


            builder.Services.AddDbContext<AreaContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            builder.Services.AddScoped<JwtTokenService>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,
                         ValidIssuer = builder.Configuration["Jwt:Issuer"],
                         ValidAudience = builder.Configuration["Jwt:Audience"],
                         IssuerSigningKey = new SymmetricSecurityKey(
                         Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
                         )
                     };
                 });


            builder.Services.Configure<FirebaseSettings>(builder.Configuration.GetSection("Firebase"));
            builder.Services.AddSingleton<FirebaseService>();

            builder.Services.AddScoped<ReportService>();
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            builder.Services.AddControllers()
                 .AddNewtonsoftJson(options =>
                 {
                       options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                 });


            builder.Services.AddControllers()
                 .AddJsonOptions(options =>
                 {
                     //options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                 });
            builder.Services.AddSignalR();
            builder.Services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type => type.FullName);
            });

            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseCors("AllowAll");
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllers();

            });
            app.MapControllers();
            //app.Run("http://0.0.0.0:5000");
            app.Run();
        }
    }

}