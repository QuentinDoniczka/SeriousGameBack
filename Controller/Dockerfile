FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MobileSeriousGame.API/MobileSeriousGame.API.csproj", "MobileSeriousGame.API/"]
COPY ["Model/Model.csproj", "Model/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["Controller/Controller.csproj", "Controller/"]

RUN dotnet restore "MobileSeriousGame.API/MobileSeriousGame.API.csproj"
COPY . .
WORKDIR "/src/MobileSeriousGame.API"
RUN dotnet build "MobileSeriousGame.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MobileSeriousGame.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MobileSeriousGame.API.dll"]