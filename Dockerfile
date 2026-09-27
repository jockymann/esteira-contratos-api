# Usa a imagem base do .NET 8 para rodar a aplicação
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Usa a imagem do SDK para compilar o código
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto para restaurar as dependências (otimização de cache do Docker)
COPY ["EsteiraAluguel.Api/EsteiraAluguel.Api.csproj", "EsteiraAluguel.Api/"]
COPY ["EsteiraAluguel.Application/EsteiraAluguel.Application.csproj", "EsteiraAluguel.Application/"]
COPY ["EsteiraAluguel.Domain/EsteiraAluguel.Domain.csproj", "EsteiraAluguel.Domain/"]
COPY ["EsteiraAluguel.Infrastructure/EsteiraAluguel.Infrastructure.csproj", "EsteiraAluguel.Infrastructure/"]
RUN dotnet restore "EsteiraAluguel.Api/EsteiraAluguel.Api.csproj"

# Copia o restante do código e compila
COPY . .
WORKDIR "/src/EsteiraAluguel.Api"
RUN dotnet build "EsteiraAluguel.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EsteiraAluguel.Api.csproj" -c Release -o /app/publish

# Gera a imagem final apenas com o runtime, deixando o container leve
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EsteiraAluguel.Api.dll"]