using Kreddit.Api.Data;
using Kreddit.api3.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=kreddit.db"));

var app = builder.Build();

app.UseHttpsRedirection();

// GET /posts/
app.MapGet("/posts/", async (AppDbContext db) =>
    await db.Posts
        .Include(p => p.User)
        .Include(p => p.Comments)
        .ThenInclude(c => c.User)
        .ToListAsync());

// GET /posts/{id}/
app.MapGet("/posts/{id}/", async (int id, AppDbContext db) =>
{
    var post = await db.Posts
        .Include(p => p.User)
        .Include(p => p.Comments)
        .ThenInclude(c => c.User)
        .FirstOrDefaultAsync(p => p.Id == id);

    return post is null
        ? Results.NotFound()
        : Results.Ok(post);
});

// POST /posts
app.MapPost("/posts", async (Post post, AppDbContext db) =>
{
    db.Posts.Add(post);
    await db.SaveChangesAsync();

    return Results.Created($"/posts/{post.Id}", post);
});

// POST /posts/{id}/comments
app.MapPost("/posts/{id}/comments",
    async (int id, Comment comment, AppDbContext db) =>
{
    var post = await db.Posts
        .FirstOrDefaultAsync(p => p.Id == id);

    if (post is null)
        return Results.NotFound();

    db.Comments.Add(comment);

    await db.SaveChangesAsync();

    return Results.Created(
        $"/posts/{id}/comments",
        comment);
});

// PUT /posts/{id}/upvote/
app.MapPut("/posts/{id}/upvote/",
    async (int id, AppDbContext db) =>
{
    var post = await db.Posts.FindAsync(id);

    if (post is null)
        return Results.NotFound();

    post.Upvotes++;

    await db.SaveChangesAsync();

    return Results.Ok(post);
});

// PUT /posts/{id}/downvote/
app.MapPut("/posts/{id}/downvote/",
    async (int id, AppDbContext db) =>
{
    var post = await db.Posts.FindAsync(id);

    if (post is null)
        return Results.NotFound();

    post.Downvotes++;

    await db.SaveChangesAsync();

    return Results.Ok(post);
});

// PUT /posts/{postid}/comments/{commentid}/upvote/
app.MapPut("/posts/{postid}/comments/{commentid}/upvote/",
    async (int postid, int commentid, AppDbContext db) =>
{
    var comment = await db.Comments
        .FirstOrDefaultAsync(c => c.Id == commentid);

    if (comment is null)
        return Results.NotFound();

    comment.Upvotes++;

    await db.SaveChangesAsync();

    return Results.Ok(comment);
});

// PUT /posts/{postid}/comments/{commentid}/downvote/
app.MapPut("/posts/{postid}/comments/{commentid}/downvote/",
    async (int postid, int commentid, AppDbContext db) =>
{
    var comment = await db.Comments
        .FirstOrDefaultAsync(c => c.Id == commentid);

    if (comment is null)
        return Results.NotFound();

    comment.Downvotes++;

    await db.SaveChangesAsync();

    return Results.Ok(comment);
});

app.Run();