using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todo_api_v3._1.Models;

namespace todo_api_v3._1.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TodoController : ControllerBase
  {
    private readonly TodoContext _context;

    public TodoController(TodoContext context)
    {
      _context = context;
    }

    // GET: api/Todo
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems(bool showIncompleteTodos, bool showCompletedTodos)
    {
      try
      {
        //未完了タスクの取得
        if (showIncompleteTodos)
        {
          return await _context.TodoItem.Where(todoItem => todoItem.CompletionDate == null).ToListAsync();
        }
        //完了済タスクの取得
        else if (showCompletedTodos)
        {
          return await _context.TodoItem.Where(todoItem => todoItem.CompletionDate != null).ToListAsync();
        }
        //全件取得
        else
        {
          return await _context.TodoItem.ToListAsync();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error message: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return StatusCode(500, new { message = "何らかの問題が発生しました。再度実行してください。" });
      }
    }

    // POST: api/Todo
    [HttpPost]
    public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
    {
      if (todoItem.Description != null && todoItem.Description.Length > 100)
      {
        return BadRequest("タスクの内容は100文字以内にしてください");
      }

      //受け取ったISO8601形式の文字列(UTC)をDateTime型に変換
      DateTime.TryParse(todoItem.DueDate.ToString(), out DateTime dueDate);
      //このままではDateTimeKindがunspecified(不特定)となりデータベース登録時にエラーとなるためDateTimeKindにUtcを追加する
      todoItem.DueDate = DateTime.SpecifyKind(dueDate, DateTimeKind.Utc);

      //期日が過去日でないことをチェック
      if (todoItem.DueDate < DateTime.UtcNow.AddTicks(-DateTime.UtcNow.Ticks % TimeSpan.TicksPerMinute))
      {
        return BadRequest("期日に過去の日付が設定されています");
      }

      _context.TodoItem.Add(todoItem);

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error message: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return StatusCode(500, new { message = "何らかの問題が発生しました。再度実行してください。" });
      }
      return CreatedAtAction(nameof(GetTodoItems), new { id = todoItem.Id }, todoItem);
    }

    // PUT: api/Todo/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodoItem(int id, TodoItem todoItem)
    {
      if (id != todoItem.Id)
      {
        return BadRequest("タスクが存在しません");
      }
      if (todoItem.Description != null && todoItem.Description.Length > 100)
      {
        return BadRequest("タスクの内容は100文字以内にしてください");
      }

      //受け取ったISO8601形式の文字列(UTC)をDateTime型に変換
      DateTime.TryParse(todoItem.DueDate.ToString(), out DateTime dueDate);
      //このままではDateTimeKindがunspecified(不特定)となりデータベース登録時にエラーとなるためDateTimeKindにUtcを追加する
      todoItem.DueDate = DateTime.SpecifyKind(dueDate, DateTimeKind.Utc);

      //期日が過去日でないことをチェック
      //だたし、UtcNowはミリ秒単位の制度を持つため、リクエストとほぼ同時刻に設定された日付が過去日と判定されるため、
      //タイマー刻みの現在時刻をTimeSpan.TicksPerMinute(タイマー刻みの1分を表す定数)で割ったあまりを、マイナス値(つまり秒以下の切り捨て)としてAddticksの引数に渡して、分単位での比較を行う
      if (todoItem.DueDate < DateTime.UtcNow.AddTicks(-DateTime.UtcNow.Ticks % TimeSpan.TicksPerMinute))
      {
        return BadRequest("期日に過去の日付が設定されています");
      }

      _context.Entry(todoItem).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error message: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return StatusCode(500, new { message = "何らかの問題が発生しました。再度実行してください。" });
      }
      return NoContent();
    }

    // PUT: api/Todo/Status/5
    [HttpPut("Status/{id}")]
    public async Task<IActionResult> UpdateTodoStatus(int id, TodoItem todoItem)
    {
      if (id != todoItem.Id)
      {
        return BadRequest("タスクが存在しません");
      }

      // タスクをデータベースから取得
      var existingTodoItem = await _context.TodoItem.FindAsync(id);
      if (existingTodoItem == null)
      {
        return NotFound("タスクが存在しません");
      }

      // 状態のみを更新
      if (existingTodoItem.CompletionDate == null)
      {
        existingTodoItem.CompletionDate = DateTime.UtcNow;
      }
      else
      {
        existingTodoItem.CompletionDate = null;
      }

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error message: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return StatusCode(500, new { message = "何らかの問題が発生しました。再度実行してください。" });
      }
      return Ok(existingTodoItem);
    }

    // DELETE: api/Todo/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(int id)
    {
      var todoItem = await _context.TodoItem.FindAsync(id);

      if (todoItem == null)
      {
        return NotFound(new { message = "削除対象が見つかりませんでした。" });
      }

      _context.TodoItem.Remove(todoItem);

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error message: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return StatusCode(500, new { message = "何らかの問題が発生しました。再度実行してください。" });
      }
      return NoContent();
    }
  }
}