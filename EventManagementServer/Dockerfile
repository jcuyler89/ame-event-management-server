# Use the official .NET 8.0 SDK as the build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application files
COPY . ./

# Build the application
RUN dotnet publish -c Release -o /app/out

# Use the official .NET runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Set environment variables
ENV JWT_ISSUER=""
ENV JWT_AUDIENCE=""
ENV JWT_AUTHORITY=""
ENV ALLOWED_ORIGINS=""
ENV DEFAULT_CONNECTION_STRING=""
ENV KeyVaultName=""


# Copy the published output from the build step
COPY --from=build-env /app/out .

# Expose the port your application runs on
EXPOSE 5050

# Set the entry point for the container
ENTRYPOINT ["dotnet", "EventManagementServer.dll"]
