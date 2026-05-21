FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore
COPY PRN232.Lab1.Services/PRN232.Lab1.Services.csproj ./PRN232.Lab1.Services/
COPY PRN232.Lab1.Repositories/PRN232.Lab1.Repositories.csproj ./PRN232.Lab1.Repositories/
COPY PRN232.Lab1.API/PRN232.Lab1.API.csproj ./PRN232.Lab1.API/
RUN dotnet restore PRN232.Lab1.API/PRN232.Lab1.API.csproj

# copy everything and publish
COPY . .
RUN dotnet publish PRN232.Lab1.API/PRN232.Lab1.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "PRN232.Lab1.API.dll"]
