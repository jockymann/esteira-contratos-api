using System;

namespace EsteiraAluguel.Domain.Entities
{
    public enum StatusProposta { Nova, AnaliseCredito, ContratoEmitido, Assinado, Ativo, Reprovada, Cancelada }

    /// <summary>
    /// Entidade que representa a Proposta e atua como a Máquina de Estados (State Machine)
    /// </summary>
    public class Proposta
    {
        public Guid Id { get; private set; }
        public Guid ImovelId { get; private set; }
        public Guid ClienteId { get; private set; }
        public StatusProposta Status { get; private set; }
        public Imovel? Imovel { get; private set; }

        protected Proposta() { } // Para o Entity Framework

        public Proposta(Guid imovelId, Guid clienteId, Imovel imovel)
        {
            Id = Guid.NewGuid();
            ImovelId = imovelId;
            ClienteId = clienteId;
            Imovel = imovel ?? throw new ArgumentNullException(nameof(imovel));
            Status = StatusProposta.Nova;
            
            // Ao criar a proposta, já altera o status do imóvel associado
            Imovel.IniciarNegociacao();
        }

        /* 
         * MÁQUINA DE ESTADOS:
         * Estes métodos garantem a regra de ouro do negócio: Não é permitido pular etapas.
         * Se um desenvolvedor tentar ir de 'Nova' direto para 'Assinado', o sistema bloqueia.
         */
         
        public void AvancarParaAnaliseCredito()
        {
            ValidarTransicao(StatusProposta.Nova);
            Status = StatusProposta.AnaliseCredito;
        }

        public void AvancarParaContratoEmitido()
        {
            ValidarTransicao(StatusProposta.AnaliseCredito);
            Status = StatusProposta.ContratoEmitido;
        }

        public void AvancarParaAssinado()
        {
            ValidarTransicao(StatusProposta.ContratoEmitido);
            Status = StatusProposta.Assinado;
        }

        public void AtivarContrato()
        {
            ValidarTransicao(StatusProposta.Assinado);
            Status = StatusProposta.Ativo;
            
            // Quando a proposta finaliza, o imóvel é permanentemente alugado
            if (Imovel != null) Imovel.FinalizarLocacao();
        }

        public void Reprovar()
        {
            if (Status == StatusProposta.Ativo)
                throw new InvalidOperationException("Contrato ativo não pode ser reprovado.");
                
            Status = StatusProposta.Reprovada;
            if (Imovel != null) Imovel.LiberarImovel();
        }

        public void Cancelar()
        {
            if (Status == StatusProposta.Ativo)
                throw new InvalidOperationException("Contrato ativo não pode ser cancelado via este fluxo.");
                
            Status = StatusProposta.Cancelada;
            if (Imovel != null) Imovel.LiberarImovel();
        }

        private void ValidarTransicao(StatusProposta statusEsperado)
        {
            if (Status != statusEsperado)
                throw new InvalidOperationException($"Transição inválida. O status atual é {Status}, mas era esperado {statusEsperado}. O fluxo obrigatório deve ser respeitado.");
        }
    }
}