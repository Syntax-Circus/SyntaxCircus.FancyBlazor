# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["global.json", "Directory.Build.props", "Directory.Packages.props", "GitVersion.yml", "./"]
COPY ["src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj", "src/SyntaxCircus.FancyBlazor/"]
COPY ["src/SyntaxCircus.FancyBlazor.WebGL/SyntaxCircus.FancyBlazor.WebGL.csproj", "src/SyntaxCircus.FancyBlazor.WebGL/"]
COPY ["samples/FancyBlazor.Demo.Client/FancyBlazor.Demo.Client.csproj", "samples/FancyBlazor.Demo.Client/"]
COPY ["samples/FancyBlazor.Demo/FancyBlazor.Demo.csproj", "samples/FancyBlazor.Demo/"]
RUN dotnet restore "samples/FancyBlazor.Demo/FancyBlazor.Demo.csproj"

COPY . .
RUN dotnet publish "samples/FancyBlazor.Demo/FancyBlazor.Demo.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:DisableGitVersionTask=true
RUN test -f /app/publish/wwwroot/_framework/blazor.web.js

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "FancyBlazor.Demo.dll"]
