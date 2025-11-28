using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CondoHub.Models.Entities;
using CondoHub.Data;
using Microsoft.AspNetCore.Identity;
using CondoHub.Models.ViewModels;

namespace CondoHub.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Mural()
        {
            var avisos = _context.Set<Messages>().OrderByDescending(a => a.DataPublicacao).ToList();
            var usuarios = _context.Users.ToList();
            var mural = avisos.Select(a => {
                var usuario = usuarios.FirstOrDefault(u => u.Id == a.UsuarioId);
                return new AvisoMuralViewModel {
                    Id = a.Id,
                    Titulo = a.Titulo,
                    Mensagem = a.Mensagem,
                    Tipo = a.Tipo.ToString(),
                    ImagemPath = a.ImagemPath,
                    DataPublicacao = a.DataPublicacao,
                    UsuarioNome = usuario?.Nome ?? "Usuário",
                    UsuarioTipo = usuario?.Role.ToString() ?? "Morador"
                };
            }).ToList();
            return View(mural);
        }


        public IActionResult CriarAviso()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarAviso(Messages aviso)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                    return View(aviso);
                }
                aviso.UsuarioId = user.Id;
                aviso.DataPublicacao = DateTime.Now;
                _context.Add(aviso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Mural));
            }
            return View(aviso);
        }
    }
}