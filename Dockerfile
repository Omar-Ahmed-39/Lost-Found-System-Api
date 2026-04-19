FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copy only the solution and project files first to cache NuGet dependencies
COPY ["src/LostAndFound.Api/LostAndFound.Api.csproj", "src/LostAndFound.Api/"]
COPY ["src/LostAndFound.Core/LostAndFound.Core.csproj", "src/LostAndFound.Core/"]
COPY ["src/LostAndFound.Infrastructure/LostAndFound.Infrastructure.csproj", "src/LostAndFound.Infrastructure/"]

# 2. Restore the API project (which will also restore Core and Infrastructure)
RUN dotnet restore "src/LostAndFound.Api/LostAndFound.Api.csproj"

# 3. Copy the rest of the source code
COPY . .

# 4. Build and Publish
WORKDIR "/src/src/LostAndFound.Api"
RUN dotnet publish "LostAndFound.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 5. Final run image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final 
WORKDIR /app

EXPOSE 8080 

COPY --from=build /app/publish .

# Set the entry point to run the API
ENTRYPOINT ["dotnet", "LostAndFound.Api.dll"]