using Amazon.S3;
using net9PoCR2.Abstract;
using net9PoCR2.Concrete;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IAmazonS3>(provider =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = builder.Configuration["R2:ServiceUrl"],
        ForcePathStyle = true,
        SignatureVersion = "4"
    };

    return new AmazonS3Client(
        builder.Configuration["R2:AccessKey"],
        builder.Configuration["R2:SecretKey"],
        config
    );
});
builder.Services.AddScoped<IR2Service, R2Service>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
