using AppointmentsApi.Data;
using AppointmentsApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Sql")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

// ---- Endpoints Customers ----
app.MapGet("/api/customers", async (AppDbContext db) =>
    await db.Customers.AsNoTracking().ToListAsync());

app.MapGet("/api/customers/{id:guid}", async (Guid id, AppDbContext db) =>
    await db.Customers.FindAsync(id) is { } c ? Results.Ok(c) : Results.NotFound());

app.MapPost("/api/customers", async (Customer c, AppDbContext db) => {
    db.Customers.Add(c); await db.SaveChangesAsync(); return Results.Created($"/api/customers/{c.Id}", c);
});

app.MapPut("/api/customers/{id:guid}", async (Guid id, Customer input, AppDbContext db) => {
    var c = await db.Customers.FindAsync(id); if (c is null) return Results.NotFound();
    c.Name = input.Name; c.Email = input.Email; await db.SaveChangesAsync(); return Results.NoContent();
});

app.MapDelete("/api/customers/{id:guid}", async (Guid id, AppDbContext db) => {
    var c = await db.Customers.FindAsync(id); if (c is null) return Results.NotFound();
    db.Customers.Remove(c); await db.SaveChangesAsync(); return Results.NoContent();
});

// ---- Endpoints Appointments (+ filtros por fecha/estado) ----
app.MapGet("/api/appointments", async (DateTime? from, DateTime? to, string? status, AppDbContext db) => {
    var q = db.Appointments.Include(a => a.Customer).AsQueryable();
    if (from.HasValue) q = q.Where(a => a.DateTime >= from.Value);
    if (to.HasValue) q = q.Where(a => a.DateTime <= to.Value);
    if (!string.IsNullOrWhiteSpace(status)) q = q.Where(a => a.Status == status);
    return Results.Ok(await q.AsNoTracking().ToListAsync());
});

app.MapGet("/api/appointments/{id:guid}", async (Guid id, AppDbContext db) =>
    await db.Appointments.Include(a => a.Customer).FirstOrDefaultAsync(a => a.Id == id)
      is { } a ? Results.Ok(a) : Results.NotFound());

app.MapPost("/api/appointments", async (Appointment a, AppDbContext db) => {
    // Validación rápida de estado
    var ok = new[] { "scheduled", "done", "cancelled" }.Contains(a.Status);
    if (!ok) return Results.BadRequest("Invalid status");
    db.Appointments.Add(a);
    try { await db.SaveChangesAsync(); }
    catch (DbUpdateException) { return Results.Conflict("Double booking not allowed."); }
    return Results.Created($"/api/appointments/{a.Id}", a);
});

app.MapPut("/api/appointments/{id:guid}", async (Guid id, Appointment input, AppDbContext db) => {
    var a = await db.Appointments.FindAsync(id); if (a is null) return Results.NotFound();
    a.CustomerId = input.CustomerId; a.DateTime = input.DateTime; a.Status = input.Status;
    try { await db.SaveChangesAsync(); }
    catch (DbUpdateException) { return Results.Conflict("Double booking not allowed."); }
    return Results.NoContent();
});

app.MapDelete("/api/appointments/{id:guid}", async (Guid id, AppDbContext db) => {
    var a = await db.Appointments.FindAsync(id); if (a is null) return Results.NotFound();
    db.Appointments.Remove(a); await db.SaveChangesAsync(); return Results.NoContent();
});

app.Run();
