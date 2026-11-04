# AgentBrowser runtime image
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build

WORKDIR /src
COPY AgentBrowser.sln .
COPY src/ ./src/
COPY global.json ./

RUN dotnet restore AgentBrowser.sln --runtime linux-x64 \
    && dotnet publish src/AgentBrowser.Api/AgentBrowser.Api.csproj \
       --configuration Release \
       --runtime linux-x64 \
       --self-contained false \
       --output /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       ca-certificates \
       fontconfig \
       fonts-liberation \
       libasound2t64 \
       libatk-bridge2.0-0 \
       libatk1.0-0 \
       libcups2 \
       libdbus-1-3 \
       libdrm2 \
       libgbm1 \
       libglib2.0-0 \
       libgtk-3-0 \
       libnspr4 \
       libnss3 \
       libu2f-udev \
       libx11-xcb1 \
       libxcb-dri3-0 \
       libxcomposite1 \
       libxdamage1 \
       libxext6 \
       libxfixes3 \
       libxrandr2 \
       libxshmfence1 \
       lsb-release \
       xdg-utils \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /out ./
ENV ASPNETCORE_URLS=http://0.0.0.0:8080 \
    AGENTBROWSER_BROWSER_CACHE=/app/.agentbrowser-cache

EXPOSE 8080
ENTRYPOINT ["dotnet", "AgentBrowser.Api.dll"]
