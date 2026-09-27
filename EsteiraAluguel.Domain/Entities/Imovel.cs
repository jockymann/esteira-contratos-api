using System;

namespace EsteiraAluguel.Domain.Entities
{
    public enum StatusImovel { Disponivel, EmNegociacao, Alugado }

    /// <summary>
    /// Entidade que representa um Imóvel.
    /// Mantém seu próprio estado para evitar que seja alterado indevidamente fora do domínio.
    /// </summary>
    public class Imovel
    {
        public Guid Id { get; private set; }
        public string Endereco { get; private set; }
        public StatusImovel Status { get; private set; }
        
        /// <summary>
        /// Token de concorrência (Race Condition). 
        /// Essencial para garantir que duas propostas simultâneas não aluguem o mesmo imóvel.
        /// </summary>
        public uint Versao { get; private set; } 

        // Construtor sem parâmetros exigido pelo Entity Framework
        protected Imovel() { }

        public Imovel(string endereco)
        {
            Id = Guid.NewGuid();
            Endereco = endereco ?? throw new ArgumentNullException(nameof(endereco));
            Status = StatusImovel.Disponivel; // Regra de negócio: inicia sempre disponível
        }

        // Métodos de alteração de estado (Encapsulamento rico, evitando "setters" públicos)
        public void IniciarNegociacao()
        {
            if (Status != StatusImovel.Disponivel)
                throw new InvalidOperationException("Imóvel não está disponível para locação. Status atual: " + Status);
            
            Status = StatusImovel.EmNegociacao;
        }

        public void FinalizarLocacao() => Status = StatusImovel.Alugado;
        
        public void LiberarImovel() => Status = StatusImovel.Disponivel;
    }
}