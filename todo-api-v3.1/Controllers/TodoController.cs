using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        if (showIncompleteTodos)
        {
          return await _context.TodoItem.Where(todoItem => todoItem.CompletionDate == null).ToListAsync();
        }
        else if (showCompletedTodos)
        {
          return await _context.TodoItem.Where(todoItem => todoItem.CompletionDate != null).ToListAsync();
        }
        else
        {
          return await _context.TodoItem.ToListAsync();
        }
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error:{ex.Message}");
      }
    }

    // PUT: api/Todo/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTodoItem(int id, TodoItem todoItem)
    {
      if (id != todoItem.Id)
      {
        return BadRequest("タスクが存在しません");
      }
      if (todoItem.Description != null && todoItem.Description.Length > 100)
      {
        return BadRequest("タスクの内容は100文字以内にしてください");
      }

      //受け取った日付情報をUTCに変換
      todoItem.DueDate = todoItem.DueDate.ToUniversalTime();

      //UtcNowはミリ秒単位の制度を持つため、リクエストとほぼ同時刻に設定された日付が過去日と判定される
      //タイマー刻みの現在時刻をTimeSpan.TicksPerMinute(タイマー刻みの1分を表す定数)で割ったあまりを、マイナス値(つまり秒以下の切り捨て)としてAddticksの引数に渡して、分単位での比較を行う
      if (todoItem.DueDate < DateTimeOffset.UtcNow.AddTicks(-DateTimeOffset.UtcNow.Ticks % TimeSpan.TicksPerMinute))
      {
        return BadRequest("期日に過去の日付が設定されています");
      }

      _context.Entry(todoItem).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!_context.TodoItem.Any(e => e.Id == id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return NoContent();
    }

    // POST: api/Todo
    [HttpPost]
    public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
    {
      if (todoItem.Description != null && todoItem.Description.Length > 100)
      {
        return BadRequest("タスクの内容は100文字以内にしてください");
      }

      //受け取った日付情報をUTCに変換
      todoItem.DueDate = todoItem.DueDate.ToUniversalTime();

      //UtcNowはミリ秒単位の制度を持つため、リクエストとほぼ同時刻に設定された日付が過去日と判定される
      //タイマー刻みの現在時刻をTimeSpan.TicksPerMinute(タイマー刻みの1分を表す定数)で割ったあまりを、マイナス値(つまり秒以下の切り捨て)としてAddticksの引数に渡して、分単位での比較を行う
      if (todoItem.DueDate < DateTimeOffset.UtcNow.AddTicks(-DateTimeOffset.UtcNow.Ticks % TimeSpan.TicksPerMinute))
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
        return StatusCode(500, $"Internal server error:{ex.Message}");
      }

      return CreatedAtAction(nameof(GetTodoItems), new { id = todoItem.Id }, todoItem);
    }

    // DELETE: api/Todo/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(int id)
    {
      var todoItem = await _context.TodoItem.FindAsync(id);

      if (todoItem == null)
      {
        return NotFound();
      }

      _context.TodoItem.Remove(todoItem);

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error:{ex.Message}");
      }

      return NoContent();
    }
  }
}