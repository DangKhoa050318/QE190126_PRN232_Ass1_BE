# Render.com has no native .NET runtime, so the API is deployed as a Docker web service.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY QE190126_PRN232_Ass1_BE.sln .
COPY TaskTrack.API/TaskTrack.API.csproj TaskTrack.API/
COPY TaskTrack.Repo/TaskTrack.Repo.csproj TaskTrack.Repo/
COPY TaskTrack.Service/TaskTrack.Service.csproj TaskTrack.Service/
RUN dotnet restore TaskTrack.API/TaskTrack.API.csproj

COPY . .
RUN dotnet publish TaskTrack.API/TaskTrack.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    TZ=Asia/Ho_Chi_Minh
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskTrack.API.dll"]
