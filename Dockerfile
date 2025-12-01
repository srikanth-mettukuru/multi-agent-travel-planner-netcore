FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/MultiAgentTravelPlanner.Web/MultiAgentTravelPlanner.Web.csproj", "MultiAgentTravelPlanner.Web/"]
RUN dotnet restore "MultiAgentTravelPlanner.Web/MultiAgentTravelPlanner.Web.csproj"
COPY src/MultiAgentTravelPlanner.Web/ MultiAgentTravelPlanner.Web/
WORKDIR "/src/MultiAgentTravelPlanner.Web"
RUN dotnet build "MultiAgentTravelPlanner.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MultiAgentTravelPlanner.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MultiAgentTravelPlanner.Web.dll"]