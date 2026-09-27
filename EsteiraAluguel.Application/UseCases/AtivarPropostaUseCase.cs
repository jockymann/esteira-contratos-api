using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EsteiraAluguel.Domain.Entities;
using EsteiraAluguel.Infrastructure.Data;

namespace EsteiraAluguel.Application.UseCases
{
    /* 
     * PADRÃO CQRS E ARQUITETURA ORIENTADA A EVENTOS
     * Agrupamos aqui o Comando (Ação a ser feita), o Handler (Quem executa a ação) 
     * e o Evento (Notificação de que a ação ocorreu). Isso mantém o código coeso.
     */

    /// <summary>
    /// Comando que solicita a ativação de uma proposta.
    /// </summary>
    public record AtivarPropostaCommand(Guid PropostaId) : IRequest<bool>;

    /// <summary>
    /// Evento de Domínio disparado após a proposta ser ativada com sucesso no banco.
    /// </summary>
    public record PropostaAtivadaEvent(Guid PropostaId, Guid ImovelId) : INotification;

    /// <summary>
    /// Handler responsável por orquestrar a ativação. Recebe o comando e executa a regra de negócio.
    /// </summary>
    public class AtivarPropostaCommandHandler : IRequestHandler<AtivarPropostaCommand, bool>
    {
        private readonly EsteiraDbContext _context;
        private readonly IMediator _mediator;

        public AtivarPropostaCommandHandler(EsteiraDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<bool> Handle(AtivarPropostaCommand request, CancellationToken cancellationToken)
        {
            var proposta = await _context.Propostas.FindAsync(new object[] { request.PropostaId }, cancellationToken);
            if (proposta == null) return false;

            // Simulando que o contrato já passou pelas etapas anteriores para fins de teste prático
            // Numa aplicação real, endpoints separados fariam cada passo.
            if (proposta.Status == StatusProposta.Nova) proposta.AvancarParaAnaliseCredito();
            if (proposta.Status == StatusProposta.AnaliseCredito) proposta.AvancarParaContratoEmitido();
            if (proposta.Status == StatusProposta.ContratoEmitido) proposta.AvancarParaAssinado();

            // Ativa o contrato e bloqueia o imóvel definitivamente
            proposta.AtivarContrato(); 
            
            // O controle de concorrência atua aqui. Se falhar, lançará DbUpdateConcurrencyException.
            await _context.SaveChangesAsync(cancellationToken);

            // DIFERENCIAL TÉCNICO: Arquitetura Orientada a Eventos
            // Publica o evento para notificar sistemas externos (ex: Sistema Financeiro) de forma desacoplada.
            await _mediator.Publish(new PropostaAtivadaEvent(proposta.Id, proposta.ImovelId), cancellationToken);

            return true;
        }
    }

    /// <summary>
    /// Listener (Ouvinte) do Evento. Simula o disparo para o sistema financeiro.
    /// </summary>
    public class IntegracaoFinanceiroEventHandler : INotificationHandler<PropostaAtivadaEvent>
    {
        public Task Handle(PropostaAtivadaEvent notification, CancellationToken cancellationToken)
        {
            // Como exigido no desafio, simulando o disparo de um evento.
            // Aqui poderíamos ter um produtor publicando numa fila RabbitMQ, Kafka ou AWS SQS.
            Console.WriteLine($"\n[EVENTO DOMÍNIO] => Sistema Financeiro Notificado com Sucesso!");
            Console.WriteLine($"Contrato Ativo para a Proposta: {notification.PropostaId}");
            Console.WriteLine($"Imóvel {notification.ImovelId} marcado permanentemente como Alugado.\n");
            
            return Task.CompletedTask;
        }
    }
}