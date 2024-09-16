# Use the official .NET 8.0 SDK as the build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore dependencies
COPY . ./
RUN dotnet restore

# Build the application
RUN dotnet publish -c Release -o /out --no-restore

# Step 2: Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Define build-time arguments
ARG JWT_ISSUER
ARG JWT_AUDIENCE
ARG JWT_AUTHORITY
ARG ALLOWED_ORIGINS
ARG DB_CONNECTION_STRING
ARG AUTHORITY_URL

# Set environment variables using ARG values
ENV JWT_ISSUER=${JWT_ISSUER}
ENV JWT_AUDIENCE=${JWT_AUDIENCE}
ENV JWT_AUTHORITY=${JWT_AUTHORITY}
ENV ALLOWED_ORIGINS=${ALLOWED_ORIGINS}
ENV DB_CONNECTION_STRING=${DB_CONNECTION_STRING}
ENV AUTHORITY_URL=${AUTHORITY_URL}

# Copy the published output from the build step
COPY --from=build-env /out .

# Expose the port your application runs on
EXPOSE 5050

# Set the entry point for the container
ENTRYPOINT ["dotnet", "EventManagementServer.dll"]
