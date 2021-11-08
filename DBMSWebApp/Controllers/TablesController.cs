using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DBMSWebApp.Models;

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
            return View();
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
            ViewBag.DatabaseId = databaseId;
            ViewBag.Tables = new SelectList(_context.Tables.Where(t=>t.DatabaseId == databaseId), "Id", "Name");
            var firstTable = _context.Tables.Where(t => t.DatabaseId == databaseId).First();
            ViewBag.Columns = new SelectList(_context.Columns.Where(t => t.TableId == firstTable.Id), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Join(int firstTable, int secondTable, int firstCol, int secondCol)
        {
            var firsttbl = _context.Tables.Include(t=>t.Columns).Include(t=>t.Rows).First(t=>t.Id==firstTable);
            var secondtbl = _context.Tables.Include(t => t.Columns).Include(t => t.Rows).First(t=>t.Id == secondTable);
            var table = new Table();
            //foreach (var col in firsttbl.Columns)
            //{
            //    if (!col.Name.Equals(column1, StringComparison.OrdinalIgnoreCase))
            //     table.AddColumn(col.Name, col.TypeFullName, false);
            //}
            //    foreach (var col in secondTable.Columns)
            //    {
            //        if (!col.Name.Equals(column2, StringComparison.OrdinalIgnoreCase))
            //            table.AddColumn(col.Name, col.TypeFullName, false);
            //    }
            //    int colIndex1 = firstTable.Columns.IndexOf(firstTable.GetColumn(column1));
            //    int colIndex2 = secondTable.Columns.IndexOf(secondTable.GetColumn(column2));
            //    foreach (var row in secondTable.Rows)
            //    {
            //        var rows = firstTable.Rows.FindAll(r => r[colIndex1] == row[colIndex2]);
            //        foreach (var r in rows)
            //        {
            //            var tableRow = new List<string>();
            //            tableRow.AddRange(r);
            //            tableRow.AddRange(row);
            //            tableRow.RemoveAt(r.Count + colIndex2);
            //            tableRow.RemoveAt(colIndex1);
            //            table.AddRows(tableRow, false);
            //        }
               // }
                //var firstTable =  Request.Form["firstTable"].ToString();
                //return RedirectToAction("Index", "Tables", new { databaseId = table.DatabaseId });
         
            return View();
        }
        [HttpGet]
        public JsonResult GetColumns(int tableId)
        {
            List<Column> columns = _context.Columns.Where(x => x.TableId == tableId).ToList();
            return Json(columns);

        }
    }
}
