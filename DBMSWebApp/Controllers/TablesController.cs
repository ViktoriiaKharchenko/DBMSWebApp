using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DBMSWebApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DBMSWebApp.Controllers
{
    public class TablesController : Controller
    {
        private readonly DatabaseContext _context;

        public TablesController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? databaseId)
        {
            ViewBag.DatabaseId = databaseId;
            var databaseContext = _context.Tables.Where(t=> t.DatabaseId == databaseId).Include(t=>t.Database);
            return View(await databaseContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables
                .Include(t => t.Database)
                .Include(t=>t.Columns)
                .Include(t=>t.Rows)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (table == null)
            {
                return NotFound();
            }
            return RedirectToAction("Index", "Rows", new { tableId = table.Id });
        }

        public IActionResult Create(int? databaseId)
        {
            ViewBag.DatabaseId = databaseId;
            return View() ;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,DatabaseId")] Table table)
        {
            if (ModelState.IsValid)
            {
                _context.Add(table);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Tables", new { databaseId = table.DatabaseId });
            }
            return View(table);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }
            ViewData["DatabaseId"] = new SelectList(_context.Databases, "Id", "Name", table.DatabaseId);
            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DatabaseId")] Table table)
        {
            if (id != table.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(table);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TableExists(table.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DatabaseId"] = new SelectList(_context.Databases, "Id", "Name", table.DatabaseId);
            return View(table);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
 
            var table = await _context.Tables
                .Include(t => t.Database)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var columns = _context.Columns.Where(t => t.TableId == id);
            var rows = _context.Rows.Where(t => t.TableId == id);
            foreach (var row in rows)
            {
                var cells = _context.Cells.Where(t => t.RowId == row.Id);
                _context.RemoveRange(cells);
            }
            _context.RemoveRange(rows);
            _context.RemoveRange(columns);
            var table = await _context.Tables.FindAsync(id);
            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Tables", new { databaseId = table.DatabaseId });
        }

        private bool TableExists(int id)
        {
            return _context.Tables.Any(e => e.Id == id);
        }
        public IActionResult Join(int? databaseId)
        {
            var db = _context.Databases.Include(t=>t.Tables).First(t=>t.Id == databaseId);
            if(db.Tables.Count < 2)
            {
                ModelState.AddModelError("Database", string.Format("Database {0} contains only {1} tables",db.Name, db.Tables.Count));
                return View();
            }
            ViewBag.DatabaseId = databaseId;
            ViewBag.Tables = new SelectList(_context.Tables.Where(t=>t.DatabaseId == databaseId), "Id", "Name");
            var firstTable = _context.Tables.Where(t => t.DatabaseId == databaseId).First();
            ViewBag.Columns = new SelectList(_context.Columns.Where(t => t.TableId == firstTable.Id), "Id", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> JoinedTables([Bind("FirstTable, SecondTable, FirstColumn, SecondColumn")] JoinViewModel joinModel)
        {
            var firsttbl = _context.Tables.Include(t => t.Columns).Include(t => t.Rows).FirstOrDefault(t => t.Id == joinModel.FirstTable);
            var secondtbl = _context.Tables.Include(t => t.Columns).Include(t => t.Rows).FirstOrDefault(t => t.Id == joinModel.SecondTable);

            var validRows = _context.Cells.Where(t => t.ColumnID == joinModel.FirstColumn)
                    .Join(_context.Cells.Where(t => t.ColumnID == joinModel.SecondColumn), fc => fc.Value, sc => sc.Value, (fc, sc) => new { fc, sc })
                    .Join(_context.Rows.Include(t => t.Cells), r => r.fc.RowId, c => c.Id, (rfc, c) => new { rfc, c })
                    .Join(_context.Rows.Include(t => t.Cells), r2 => r2.rfc.sc.RowId, k => k.Id, (r2rfc, k) => new { r2rfc, k })
                    .Select(m => new { Row1 = m.r2rfc.c, Row2 = m.k }).ToList();

            var table = new Table
            {
                Columns = firsttbl.Columns.Where(t => t.Id != joinModel.FirstColumn).ToList()
            };
            foreach (var col in secondtbl.Columns)
            {
                if (col.Id != joinModel.SecondColumn)
                    table.Columns.Add(col);
            }
            foreach (var row in validRows)
            {
                var Row = new Row
                {
                    Cells = row.Row1.Cells.Where(t => t.ColumnID != joinModel.FirstColumn).ToList()
                };
                foreach (var cell in row.Row2.Cells)
                {
                    if (cell.ColumnID != joinModel.SecondColumn)
                        Row.Cells.Add(cell);
                }
                table.Rows.Add(Row);
            }
            table.Name = firsttbl.Name + "/" + secondtbl.Name;
            table.DatabaseId = firsttbl.DatabaseId;

            return View(table);
        }
        
        [HttpGet]
        public async Task<JsonResult> GetColumns(int tableId)
        {
            List<Column> columns = await _context.Columns.Where(x => x.TableId == tableId).ToListAsync();
            return Json(columns);

        }
    }
}
