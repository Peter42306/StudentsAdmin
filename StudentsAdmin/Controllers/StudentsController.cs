using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using StudentsAdmin.Data;
using StudentsAdmin.Models;

namespace StudentsAdmin.Controllers
{    
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment=webHostEnvironment;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            return View(await _context.Students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        [Authorize(Roles ="Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Student student, IFormFile? uploadedFile)
        {
            if (ModelState.IsValid)
            {
                if(uploadedFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image");
                    Directory.CreateDirectory(uploadsFolder);

                    string newFileNameGenerated = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
                    string newFilePath = Path.Combine(uploadsFolder, newFileNameGenerated);

                    using (var image = await Image.LoadAsync(uploadedFile.OpenReadStream()))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(600, 600),
                            Mode = ResizeMode.Max
                        }));

                        await image.SaveAsJpegAsync(newFilePath);
                    }

                    student.PhotoPath = "/image/" + newFileNameGenerated;
                }

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Student updatedStudent, IFormFile? uploadedFile)
        {
            if (id != updatedStudent.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedStudent);
            }

            var studentToUpdate = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
            if(studentToUpdate == null)
            {
                return NotFound();
            }
            
            // Обновляем поля
            studentToUpdate.Name = updatedStudent.Name;
            studentToUpdate.Email = updatedStudent.Email;

            // Если загружен новый файл — удаляем старый
            if (uploadedFile != null)
            {
                if (!string.IsNullOrEmpty(studentToUpdate.PhotoPath))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, studentToUpdate.PhotoPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }


                // Сохраняем новое изображение
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image");
                Directory.CreateDirectory(uploadsFolder);

                string newFileNameGenerated = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
                string newFilePath = Path.Combine(uploadsFolder, newFileNameGenerated);

                using (var image = await Image.LoadAsync(uploadedFile.OpenReadStream()))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(600, 600),
                        Mode = ResizeMode.Max
                    }));

                    await image.SaveAsJpegAsync(newFilePath);
                }

                studentToUpdate.PhotoPath = "/image/" + newFileNameGenerated;
            }


            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!StudentExists(updatedStudent.Id))
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

        // GET: Students/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                // Удаляем изображение, если есть
                if (!string.IsNullOrEmpty(student.PhotoPath))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, student.PhotoPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }            
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
