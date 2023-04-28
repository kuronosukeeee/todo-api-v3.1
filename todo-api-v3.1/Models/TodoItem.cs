namespace todo_api_v3._1.Models;

public class TodoItem
{
  public int Id
  { get; set; }

  public string Title { get; set; } = string.Empty;

  public string? Description
  { get; set; }

  public DateTime DueDate
  { get; set; }

  public DateTime? CompletionDate
  { get; set; }
}