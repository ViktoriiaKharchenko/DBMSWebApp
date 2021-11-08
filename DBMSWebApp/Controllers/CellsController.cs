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
    public class CellsController : Controller
    {
        private readonly DatabaseContext _context;

        public CellsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Cells
        public async Task<IActionResult> Index()
        {
            var databaseContext = _context.Cells.Include(c => c.Column).Include(c => c.Row);
            return View(await databaseContext.ToListAsync());
        }

        // GET: Cells/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cell = await _context.Cells
                .Include(c => c.Column)
                .Include(c => c.Row)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cell == null)
            {
                return NotFound();
            }

            return View(cell);
        }

        // GET: Cells/Create
        public IActionResult Create()
        {
            ViewData["ColumnID"] = new SelectList(_context.Columns, "Id", "Name");
            ViewData["RowId"] = new SelectList(_context.Rows, "Id", "Id");
            return View();
        }

        // POST: Cells/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Value,RowId,ColumnID")] Cell cell)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cell);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ColumnID"] = new SelectList(_context.Columns, "Id", "Name", cell.ColumnID);
            ViewData["RowId"] = new SelectList(_context.Rows, "Id", "Id", cell.RowId);
            return View(cell);
        }

        // GET: Cells/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cell = await _context.Cells.FindAsync(id);
            if (cell == null)
            {
                return NotFound();
            }
            ViewData["ColumnID"] = new SelectList(_context.Columns, "Id", "Name", cell.ColumnID);
            ViewData["RowId"] = new SelectList(_context.Rows, "Id", "Id", cell.RowId);
            return View(cell);
        }

        // POST: Cells/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Value,RowId,ColumnID")] Cell cell)
        {
            if (id != cell.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cell);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CellExists(cell.Id))
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
            ViewData["ColumnID"] = new SelectList(_context.Columns, "Id", "Name", cell.ColumnID);
            ViewData["RowId"] = new SelectList(_context.Rows, "Id", "Id", cell.RowId);
            return View(cell);
        }

        // GET: Cells/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cell = await _context.Cells
                .Include(c => c.Column)
                .Include(c => c.Row)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cell == null)
            {
                return NotFound();
            }

            return View(cell);
        }

        // POST: Cells/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cell = await _context.Cells.FindAsync(id);
            _context.Cells.Remove(cell);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CellExists(int id)
        {
            return _context.Cells.Any(e => e.Id == id);
        }
    }
}
