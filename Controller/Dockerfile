FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Controller/Controller.csproj", "Controller/"]
COPY ["Model/Model.csproj", "Model/"]
COPY ["Service/Service.csproj", "Service/"]

RUN dotnet restore "Controller/Controller.csproj"
COPY . .
WORKDIR "/src/Controller"
RUN dotnet build "Controller.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Controller.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Controller.dll"]