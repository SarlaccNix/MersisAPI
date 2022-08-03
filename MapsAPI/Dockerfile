FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MapsAPI/MapsAPI.csproj", "MapsAPI/"]
RUN dotnet restore "MapsAPI/MapsAPI.csproj"
COPY . .
WORKDIR "/src/MapsAPI"
RUN dotnet build "MapsAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapsAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MapsAPI.dll"]
