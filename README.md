# Esteira de Contratos de Aluguel (Rental Pipeline API)

API REST desenhada para gerir a jornada de propostas de locação de imóveis. O projeto foi arquitetado com padrões de nível *Enterprise*, focando-se na integridade do estado, prevenção de falhas de concorrência (*Race Conditions*) e facilidade de manutenção a longo prazo.

## Padrões e Decisões de Arquitetura

O código foi estruturado utilizando princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**. As motivações para estas escolhas incluem:

1. **Encapsulamento da Máquina de Estados (State Machine)**
   A regra de negócio que dita o fluxo da proposta (`NOVA -> ANALISE_CREDITO -> CONTRATO_EMITIDO...`) não está espalhada em *Controllers* ou *Services*. Ela foi desenhada dentro da própria entidade `Proposta`. Isto impede alterações de estado anómalas (Anemic Domain Model) e garante que nenhum desenvolvedor da equipa consiga burlar o funil de transição.

2. **Resolução de Concorrência (Race Conditions)**
   Para o requisito crítico de impedir que o mesmo imóvel seja alugado por duas pessoas no mesmo milissegundo, optei por um mecanismo de **Controlo de Concorrência Otimista**.
   Em vez de sobrecarregar a base de dados com *locks* pessimistas pesados, a API utiliza o recurso de `IsRowVersion` do Entity Framework mapeado para a coluna oculta `xmin` do PostgreSQL. Se existirem duas submissões simultâneas, o motor da base de dados rejeita a segunda de forma nativa e extremamente rápida, lançando uma exceção tratada pela aplicação.

3. **Arquitetura Orientada a Eventos (CQRS com MediatR)**
   A transição final para o status `ATIVO` exige comunicação com o sistema financeiro. Para garantir que a API não fique fortemente acoplada a serviços externos, foi implementado o padrão CQRS. O comando de ativação desencadeia a emissão de um *Domain Event* (`PropostaAtivadaEvent`), permitindo que ouvintes (*Handlers*) notifiquem serviços externos (simulado via console) de forma totalmente assíncrona e desacoplada.

## Stack Tecnológica

- **C# / .NET 8**
- **Entity Framework Core 8**
- **PostgreSQL 15**
- **MediatR** (Orquestração de Comandos e Eventos)
- **Docker & Docker Compose** (Containerização)

## Como Executar o Projeto

A aplicação e a base de dados foram totalmente containerizadas para garantir que qualquer engenheiro consiga testar a solução com apenas um comando, sem necessidade de instalar SDKs ou bases de dados locais.

1. Certifique-se de que o [Docker](https://docs.docker.com/get-docker/) está a ser executado na sua máquina.
2. Na raiz do projeto, execute:
   ```bash
   docker-compose up --build -d