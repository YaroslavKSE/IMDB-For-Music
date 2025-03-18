FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MusicInteraction.Presentation/MusicInteraction.Presentation.csproj", "MusicInteraction.Presentation/"]
COPY ["MusicInteraction.Application/MusicInteraction.Application.csproj", "MusicInteraction.Application/"]
COPY ["MusicInteraction.Domain/MusicInteraction.Domain.csproj", "MusicInteraction.Domain/"]
COPY ["MusicInteraction.Infrastructure/MusicInteraction.Infrastructure.csproj", "MusicInteraction.Infrastructure/"]
RUN dotnet restore "MusicInteraction.Presentation/MusicInteraction.Presentation.csproj"
COPY . .
WORKDIR "/src/MusicInteraction.Presentation"
RUN dotnet build "MusicInteraction.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MusicInteraction.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicInteraction.Presentation.dll"]