# 1. Aşama: Derleme (Build) Aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyasını kopyalayıp bağımlılıkları (NuGet paketlerini) indiriyoruz
COPY ["StudyPlatformAPI.csproj", "./"]
RUN dotnet restore "StudyPlatformAPI.csproj"

# Kalan tüm kaynak kodlarını kopyalayıp projeyi release modunda derliyoruz
COPY . .
RUN dotnet publish "StudyPlatformAPI.csproj" -c Release -o /app/publish

# 2. Aşama: Çalıştırma (Runtime) Aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Konteyner ayağa kalktığında çalışacak komut
ENTRYPOINT ["dotnet", "StudyPlatformAPI.dll"]