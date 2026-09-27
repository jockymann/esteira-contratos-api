using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EsteiraAluguel.Domain.Entities;
using EsteiraAluguel.Infrastructure.Data;

namespace EsteiraAluguel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImoveisController : ControllerBase
    {
        private readonly EsteiraDbContext _context;

        public ImoveisController(EsteiraDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Cadastra um novo imóvel. O status inicial será sempre 'Disponível'.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CadastrarImovel([FromBody] CadastrarImovelRequest request)
        {
            var imovel = new Imovel(request.Endereco);
            _context.Imoveis.Add(imovel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CadastrarImovel), new { id = imovel.Id }, imovel);
        }

        // DTO Interno para evitar exposição direta das Entidades
        public record CadastrarImovelRequest(string Endereco);
    }
}