# Use the official .NET image as a build stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET SDK image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EventManagementServer.csproj", "./"]
RUN dotnet restore "./EventManagementServer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "EventManagementServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventManagementServer.csproj" -c Release -o /app/publish

# Final stage: running the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventManagementServer.dll"]
