# Use the official ASP.NET Core SDK image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy the project file and restore any dependencies (use .csproj for the project name)
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . .

# Publish the application
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose port 8080
EXPOSE 8080

# Start the application
CMD ["dotnet", "patchikatcha-backend.dll"]
