using Microsoft.EntityFrameworkCore;

namespace todo_api_v3._1.Models;

public class TodoContext : DbContext
{
  public TodoContext(DbContextOptions<TodoContext> options) : base(options)
  {
  }

  public DbSet<TodoItem> TodoItem { get; set; } = null!;
}