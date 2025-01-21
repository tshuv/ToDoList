using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// הוספת שירות ה-CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin() // מאפשר גישה מכל ה-Origins (Domains).
              .AllowAnyMethod() // מאפשר את כל שיטות ה-HTTP (GET, POST, PUT, DELETE).
              .AllowAnyHeader(); // מאפשר את כל ה-Headers.
    });
});

// הוספת שירות ה-DbContext עם החיבור למסד נתונים MySQL
builder.Services.AddDbContext<sysContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))));

// הוספת שירות Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// הפעלת CORS על כל ה-API
app.UseCors();

// הפעלת Swagger אם האפליקציה פועלת במצב פיתוח
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// מיפוי של כל ה-routes

// שליפת כל המשימות
app.MapGet("/tasks", async (sysContext dbContext) =>
{
    return await dbContext.Items.ToListAsync();
});

// הוספת משימה חדשה
app.MapPost("/tasks", async (sysContext dbContext, Item newItem) =>
{
    dbContext.Items.Add(newItem);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/tasks/{newItem.Id}", newItem);
});

// עדכון משימה קיימת
app.MapPut("/tasks/{id}", async (sysContext dbContext, int id, Item updatedItem) =>
{
    var item = await dbContext.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.IsComplete = updatedItem.IsComplete;

    await dbContext.SaveChangesAsync();
    return Results.Ok(item);
});

// מחיקת משימה
app.MapDelete("/tasks/{id}", async (sysContext dbContext, int id) =>
{
    var item = await dbContext.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    dbContext.Items.Remove(item);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
