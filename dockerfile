FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app
#
## copy csproj and restore as distinct layers
COPY *.sln .
COPY apc.api.core/*.csproj ./apc.api.core/
copy apc.businesslayer.core/*.csproj ./apc.businesslayer.core/
RUN dotnet restore
#

## copy everything else and build app
COPY apc.api.core/. ./apc.api.core/
copy apc.businesslayer.core/. ./apc.businesslayer.core/
WORKDIR /app/apc.api.core
#
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 
WORKDIR /app
RUN dir
COPY --from=build-env /app/apc.api.core/out ./out
ENTRYPOINT ["dotnet", "apc.api.core.dll"]
