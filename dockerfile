FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app
#
## copy csproj and restore as distinct layers
COPY *.sln .
COPY apc.api.core/*.csproj ./apc.api.core/
COPY apc.businesslayer.core/*.csproj ./apc.businesslayer.core/
COPY apc.api.core/AmazonSites.xml ./apc.api.core/
RUN dotnet restore
#

## copy everything else and build app
COPY apc.api.core/. ./apc.api.core/
COPY apc.businesslayer.core/. ./apc.businesslayer.core/
WORKDIR /app/apc.api.core
#
RUN dotnet publish -c Release -o out -r linux-x64


FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 
WORKDIR /app
COPY --from=build-env /app/apc.api.core/out ./out
COPY --from=build-env /app/apc.api.core/AmazonSites.xml ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "/app/out/apc.api.core.dll"]
