# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Kelas.Api/Kelas.Api.csproj", "Kelas.Api/"]
COPY ["Kelas.Domain/Kelas.Domain.csproj", "Kelas.Domain/"]
COPY ["Kelas.Repositories/Kelas.Repositories.csproj", "Kelas.Repositories/"]
COPY ["Kelas.Services/Kelas.Services.csproj", "Kelas.Services/"]
COPY ["Kelas.IoC.Resolver/Kelas.IoC.Resolver.csproj", "Kelas.IoC.Resolver/"]
RUN dotnet restore "Kelas.Api/Kelas.Api.csproj"
COPY . .
WORKDIR "/src/Kelas.Api"
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["sh", "-c", "dotnet Kelas.Api.dll --urls http://0.0.0.0:${PORT:-5000}"]
