using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DBMSWebApp.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace DBMSWebApp.Controllers
{
    public class ColumnsController : Controller
    {
        private readonly DatabaseContext _context;

        public ColumnsController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var databaseContext = _context.Columns.Include(c => c.Table);
            return View(await databaseContext.ToListAsync());
        }

        public IActionResult Create(int? tableId)
        {
            ViewBag.TableId = tableId;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,TypeFullName,TableId")] Column column)
        {
            if (ModelState.IsValid)
            {
                var rows = _context.Rows.Where(t => t.TableId == column.TableId);
                foreach(var row in rows)
                {
                    Cell cell = new Cell()
                    {
                        ColumnID = column.Id,
                        RowId = row.Id,
                        Value = null
                    };
                    _context.Add(cell);
                    column.Cells.Add(cell);
                }
                _context.Add(column);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Rows", new { tableId = column.TableId});
            }
            return View(column);
        }
      
        public async Task<IActionResult> Delete(int? tableId)
        {
            if (tableId == null)
            {
                return NotFound();
            }

            var column = await _context.Columns
                .Include(c => c.Table).Where(m => m.TableId == tableId).ToListAsync();
            if (column == null)
            {
                return NotFound();
            }
            ViewBag.TableId = tableId;
            ViewData["Id"] = new SelectList(column, "Id", "Name");
            return View();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([Bind("Id, TableId")]Column column)
        {
            var col = await _context.Columns.FindAsync(column.Id);
            var cells = _context.Cells.Where(t => t.ColumnID == column.Id);
            _context.Cells.RemoveRange(cells);
            _context.Columns.Remove(col);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Rows", new { tableId = column.TableId });
        }

        private bool ColumnExists(int id)
        {
            return _context.Columns.Any(e => e.Id == id);
        }
        public IActionResult TypeValid(string? TypeFullName)
        {
            Regex stringInvl = new Regex(@"StringInvl\({1,1}\w,\w\)");
            Regex charrgx = new Regex(@"CharInvl\({1,1}\w,\w\)");
            if (TypeFullName.Contains("Invl") && !charrgx.IsMatch(TypeFullName) && !stringInvl.IsMatch(TypeFullName))
            {
                return Json (data: "Invalid type");
            }
            else if (!TypeFullName.Contains("Invl"))
            {
                var type = Type.GetType(TypeFullName);
                if (type == null)
                return Json (data: "Invalid type");

            }
            return Json(true);
        }
        public IActionResult CellValid(string Value, int ColumnID)
        {
            var col = _context.Columns.Find(ColumnID);
            if (col == null) return Json(data: "wrong val");
            if (col.TypeFullName.Contains("Invl"))
            {
                bool type = col.TypeFullName.Contains("Char") ? true : false;
                char from = col.TypeFullName.Split('(')[1].Substring(0, 1).ToCharArray()[0];
                char to = col.TypeFullName.Split(',')[1].Substring(0, 1).ToCharArray()[0];
                if (CheckValue(Value.ToString(), type, from, to))
                    return Json(true);
            }
            else if (CheckCast(Value, col.TypeFullName))
                return Json(true);

            return Json(data: string.Format("Column {0} has {1} type", col.Name, col.TypeFullName));
        }
        private bool CheckCast(string value, string type)
        {
            if (value.ToString() == "") return true;
            try
            {
                var resultVal = Convert.ChangeType(value, Type.GetType(type));
                if (!resultVal.ToString().Equals(value.ToString()))
                    throw new InvalidCastException();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool CheckValue(string value, bool charType, char from, char to)
        {
            if (value == "") return true;

            if (charType)
            {
                if (value.Length > 1) return false;
            }
            var charList = new List<char>();
            charList.AddRange(value);
            var invalidChars = charList.FindAll(c => c < from || c > to);
            if (invalidChars.Count != 0) return false;
            return true;
        }
    }
}
