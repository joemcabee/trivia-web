# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["TriviaApp.API.csproj", "./"]
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "TriviaApp.API.csproj"

# Copy everything else and build
COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "TriviaApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Configure the app to listen on port 80
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "TriviaApp.API.dll"]
