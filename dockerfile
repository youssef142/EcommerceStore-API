
# Development stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS development

WORKDIR /src

COPY EcommerceStore/EcommerceStore.csproj ./EcommerceStore/
RUN dotnet restore EcommerceStore/EcommerceStore.csproj

COPY . .

WORKDIR /src/EcommerceStore

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

CMD ["dotnet", "watch", "run", "--no-launch-profile", "--urls", "http://0.0.0.0:8080"]


# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

COPY EcommerceStore/EcommerceStore.csproj ./EcommerceStore/
RUN dotnet restore EcommerceStore/EcommerceStore.csproj

COPY . .

RUN dotnet publish EcommerceStore/EcommerceStore.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# Production stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS production

WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "EcommerceStore.dll"]
