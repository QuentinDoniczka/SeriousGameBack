FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MobileSeriousGame/MobileSeriousGame.csproj", "MobileSeriousGame/"]
RUN dotnet restore "MobileSeriousGame/MobileSeriousGame.csproj"
COPY . .
WORKDIR "/src/MobileSeriousGame"
RUN dotnet build "MobileSeriousGame.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MobileSeriousGame.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MobileSeriousGame.dll"]
