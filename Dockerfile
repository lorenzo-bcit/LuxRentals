FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["LuxRentals.csproj", "./"]
RUN dotnet restore "LuxRentals.csproj"

COPY . .
RUN dotnet publish "LuxRentals.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

RUN mkdir -p /app/wwwroot/uploads/cars

EXPOSE 8080

ENTRYPOINT ["dotnet", "LuxRentals.dll"]
