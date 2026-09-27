using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using EsteiraAluguel.Application.UseCases;
using System;
using System.Threading.Tasks;
using EsteiraAluguel.Domain.Entities;
using EsteiraAluguel.Infrastructure.Data;

namespace EsteiraAluguel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropostasController : ControllerBase
    {
        private readonly EsteiraDbContext _context;
        private readonly IMediator _mediator;

        public PropostasController(EsteiraDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        /// <summary>
        /// Cria uma nova proposta, vinculando um cliente a um imóvel. 
        /// O imóvel passa automaticamente para 'Em Negociação'.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CriarProposta([FromBody] CriarPropostaRequest request)
        {
            var imovel = await _context.Imoveis.FindAsync(request.ImovelId);
            
            if (imovel == null) 
                return NotFound(new { erro = "Imóvel não encontrado." });

            try
            {
                // A própria criação da entidade cuida das regras de negócio e validação de estado
                var proposta = new Proposta(request.ImovelId, request.ClienteId, imovel);
                
                _context.Propostas.Add(proposta);
                await _context.SaveChangesAsync();
                
                return CreatedAtAction(nameof(CriarProposta), new { id = proposta.Id }, proposta);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Tratamento explícito da Race Condition para a API.
                return Conflict(new { erro = "Conflito: Este imóvel acabou de ser reservado por outra requisição simultânea." });
            }
            catch (InvalidOperationException ex)
            {
                // Captura exceções de domínio (ex: imóvel não estava disponível)
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Ativa a proposta utilizando MediatR para orquestrar o processo e os eventos consequentes.
        /// </summary>
        [HttpPost("{id}/ativar")]
        public async Task<IActionResult> AtivarProposta(Guid id)
        {
            try
            {
                // O Mediator envia o comando e lida com a complexidade, mantendo o controller limpo.
                var sucesso = await _mediator.Send(new AtivarPropostaCommand(id));
                
                if (!sucesso) 
                    return NotFound(new { erro = "Proposta não encontrada." });

                return Ok(new { mensagem = "Contrato ativado e imóvel alugado com sucesso. Eventos do sistema financeiro disparados." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
        
        public record CriarPropostaRequest(Guid ImovelId, Guid ClienteId);
    }
}