FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./

# Copy everything else and build
COPY . ./
RUN dotnet publish TCPSocketServer.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 2323
ENTRYPOINT ["dotnet", "TCPSocketServer.dll"]